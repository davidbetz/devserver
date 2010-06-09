//++
//+ Server.cs
//+
//+ Portions of this file were adapted from the Cassini Web Server
//+ copyrighted by Microsoft.
//+
//++
using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Security.Principal;
using System.Threading;
using System.Web.Hosting;
//+
namespace DevServer.WebCore
{
    [PermissionSet(SecurityAction.LinkDemand, Name = "Everything"), PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
    public class Server : MarshalByRefObject, IDisposable
    {
        private const Int32 SecurityImpersonation = 2;
        private const Int32 TOKEN_ALL_ACCESS = 0xf01ff;
        private const Int32 TOKEN_EXECUTE = 0x20000;
        private const Int32 TOKEN_IMPERSONATE = 4;
        private const Int32 TOKEN_READ = 0x20008;
        
        //+
        private IntPtr processToken;

        //- $SocketAcceptWaitCallback- //
        private WaitCallback SocketAcceptWaitCallback;

        //+

        //- $AppManager- //
        private ApplicationManager AppManager { get; set; }

        //- @Instance- //
        public DevServer.Instance Instance { get; set; }

        //- $LockObject- //
        private Object LockObject { get; set; }

        //- @RootUrl- //
        private Host Host { get; set; }
        
        //- @Port- //
        public Int32 Port { get; private set; }

        //- @PhysicalPath- //
        public String PhysicalPath { get; private set; }
        
        //- $RequireAuthentication- //
        private Boolean RequireAuthentication { get; set; }

        //- @RootUrl- //
        public String RootUrl
        {
            get
            {
                if (this.Port != 80)
                {
                    return ("http://localhost:" + this.Port + this.VirtualPath);
                }
                return ("http://localhost" + this.VirtualPath);
            }
        }

        //- $ProcessUser- //
        private String ProcessUser { get; set; }
        
        //- $ShutdownInProgress- //
        private Boolean ShutdownInProgress { get; set; }

        //- @RootUrl- //
        private Socket Socket { get; set; }

        //- $StartCallback- //
        private WaitCallback StartCallback { get; set; }

        //- @VirtualPath- //
        public String VirtualPath { get; private set; }

        //+
        //- @Ctor- //
        public Server(Int32 port, String virtualPath, String physicalPath)
            : this(port, virtualPath, physicalPath, false)
        {
        }

        //- @Ctor- //
        public Server(Int32 port, String virtualPath, String physicalPath, Boolean requireAuthentication)
        {
            this.Instance = new DevServer.Instance();
            this.LockObject = new Object();
            this.Port = port;
            this.VirtualPath = virtualPath;
            this.PhysicalPath = physicalPath.EndsWith(@"\", StringComparison.Ordinal) ? physicalPath : (physicalPath + @"\");
            this.RequireAuthentication = requireAuthentication;
            this.SocketAcceptWaitCallback = new WaitCallback(this.OnSocketAccept);
            this.StartCallback = new WaitCallback(this.OnStart);
            this.AppManager = ApplicationManager.GetApplicationManager();
            this.ObtainProcessToken();
        }

        //+
        //- $GetCurrentThread- //
        [DllImport("KERNEL32.DLL", SetLastError = true)]
        private static extern IntPtr GetCurrentThread();

        //- $GetHost- //
        private Host GetHost()
        {
            if (this.ShutdownInProgress)
            {
                return null;
            }
            Host host = this.Host;
            if (host == null)
            {
                lock (this.LockObject)
                {
                    host = this.Host;
                    if (host == null)
                    {
                        //+ create web host
                        String appId = (this.VirtualPath + this.PhysicalPath).ToLowerInvariant().GetHashCode().ToString("x", CultureInfo.InvariantCulture);
                        this.Host = (Host)this.AppManager.CreateObject(appId, typeof(Host), this.VirtualPath, this.PhysicalPath, false);
                        this.Host.Configure(this, this.Port, this.VirtualPath, this.PhysicalPath, this.RequireAuthentication, this.Instance.Id, this.Instance.HostConfiguration);
                        host = this.Host;
                    }
                }
            }
            return host;
        }

        //- @GetProcessToken- //
        public IntPtr GetProcessToken()
        {
            return this.processToken;
        }

        //- @GetProcessUser- //
        public String GetProcessUser()
        {
            return this.ProcessUser;
        }

        //- ~HostStopped- //
        internal void HostStopped()
        {
            this.Host = null;
        }

        //- $ImpersonateSelf- //
        [DllImport("ADVAPI32.DLL", SetLastError = true)]
        private static extern Boolean ImpersonateSelf(Int32 level);

        //- @InitializeLifetimeService- //
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        public override Object InitializeLifetimeService()
        {
            return null;
        }

        //- $ObtainProcessToken- //
        private void ObtainProcessToken()
        {
            if (ImpersonateSelf(2))
            {
                OpenThreadToken(GetCurrentThread(), 0xf01ff, true, ref this.processToken);
                RevertToSelf();
                this.ProcessUser = WindowsIdentity.GetCurrent().Name;
            }
        }

        //- $OnSocketAccept- //
        private void OnSocketAccept(Object acceptedSocket)
        {
            String headers;
            if (!this.ShutdownInProgress)
            {
                DevServer.WebCore.Connection conn = new DevServer.WebCore.Connection(this, (Socket)acceptedSocket);
                if (conn.WaitForRequestBytes() == 0)
                {
                    conn.WriteErrorAndClose(400, out headers);
                }
                else
                {
                    Host host = this.GetHost();
                    if (host == null)
                    {
                        conn.WriteErrorAndClose(500, out headers);
                    }
                    else
                    {
                        try
                        {
                            host.ProcessRequest(conn);
                        }
                        catch
                        {
                            //++ .NET likes to throw "HttpException" at weird times and
                            //+  all properties of the exception including Message
                            //++ are completely blank.  So, testing isn't really possible.
                        }
                    }
                }
            }
        }

        //- $OnStart- //
        private void OnStart(Object unused)
        {
            while (!this.ShutdownInProgress)
            {
                try
                {
                    Socket state = this.Socket.Accept();
                    ThreadPool.QueueUserWorkItem(this.SocketAcceptWaitCallback, state);
                    continue;
                }
                catch
                {
                    Thread.Sleep(100);
                    continue;
                }
            }
        }

        //- $OpenThreadToken- //
        [DllImport("ADVAPI32.DLL", SetLastError = true)]
        private static extern Int32 OpenThreadToken(IntPtr thread, Int32 access, Boolean openAsSelf, ref IntPtr hToken);

        //- $RevertToSelf- //
        [DllImport("ADVAPI32.DLL", SetLastError = true)]
        private static extern Int32 RevertToSelf();

        //- @Start- //
        public void Start()
        {
            this.Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.Socket.ExclusiveAddressUse = true;
            try
            {
                BindIPAddress();
            }
            catch
            {
                this.Socket.ExclusiveAddressUse = false;
                BindIPAddress();
            }
            this.Socket.Listen(0x7fffffff);
            ThreadPool.QueueUserWorkItem(this.StartCallback);
        }

        //- $BindIPAddress- //
        private void BindIPAddress()
        {
            if (this.Instance.EnableIPAddressBinding)
            {
                //+ if they didn't set an address, default to loopback
                if (String.IsNullOrEmpty(this.Instance.BoundIPAddress))
                {
                    this.Socket.Bind(new IPEndPoint(IPAddress.Loopback, this.Port));
                }
                else
                {
                    if (this.Instance.BoundIPAddress.ToLower(System.Globalization.CultureInfo.CurrentCulture) == "loopback")
                    {
                        this.Socket.Bind(new IPEndPoint(IPAddress.Loopback, this.Port));
                    }
                    else if (this.Instance.BoundIPAddress.ToLower(System.Globalization.CultureInfo.CurrentCulture) == "any")
                    {
                        this.Socket.Bind(new IPEndPoint(IPAddress.Any, this.Port));
                    }
                    else
                    {
                        this.Socket.Bind(new IPEndPoint(IPAddress.Parse(this.Instance.BoundIPAddress), this.Port));
                    }
                }
            }
            else
            {
                this.Socket.Bind(new IPEndPoint(IPAddress.Loopback, this.Port));
            }
        }

        //- @Stop- //
        public void Stop()
        {
            this.ShutdownInProgress = true;
            try
            {
                if (this.Socket != null)
                {
                    this.Socket.Close();
                }
            }
            catch
            {
            }
            finally
            {
                this.Socket = null;
            }
            try
            {
                if (this.Host != null)
                {
                    this.Host.Shutdown();
                }
                while (this.Host != null)
                {
                    Thread.Sleep(100);
                }
            }
            catch
            {
            }
            finally
            {
                this.Host = null;
            }
        }

        //- @UpdateInstanceConfiguration- //
        public void UpdateInstanceConfiguration(HostConfiguration config)
        {
            this.GetHost().UpdateConfiguration(config);
        }

        //+
        #region IDisposable Members

        private Boolean disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(Boolean disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    this.Stop();
                }
                disposed = true;
            }
        }

        #endregion
    }
}