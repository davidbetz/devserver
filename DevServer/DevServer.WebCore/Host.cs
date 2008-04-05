//++
//+ Host.cs
//+
//+ Portions of this file were adapted from the Cassini Web Server
//+ copyrighted by Microsoft.
//+
//++
using System;
using System.Globalization;
using System.Security.Permissions;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Hosting;
//+
namespace DevServer.WebCore
{
    internal sealed class Host : MarshalByRefObject, IRegisteredObject
    {
        private volatile Int32 pendingCallsCount;

        //+
        //- $Configuration -//
        private HostConfiguration Configuration { get; set; }

        //- ~InstallPath -//
        internal String InstallPath { get; private set; }

        //- $LowerCasedClientScriptPathWithTrailingSlash -//
        private String LowerCasedClientScriptPathWithTrailingSlash { get; set; }

        //- $LowerCasedVirtualPath -//
        private String LowerCasedVirtualPath { get; set; }

        //- $LowerCasedVirtualPathWithTrailingSlash -//
        private String LowerCasedVirtualPathWithTrailingSlash { get; set; }

        //- $Server -//
        private Server Server { get; set; }

        //- ~Port -//
        internal Int32 Port { set; get; }

        //- ~VirtualPath -//
        internal String VirtualPath { get; private set; }

        //- ~RequireAuthentication -//
        internal Boolean RequireAuthentication { get; private set; }

        //- ~PhysicalClientScriptPath -//
        internal String PhysicalClientScriptPath { get; private set; }

        //- ~PhysicalPath -//
        internal String PhysicalPath { get; private set; }

        //- ~InstanceId -//
        internal String InstanceId { get; private set; }

        //- ~NormalizedClientScriptPath -//
        internal String NormalizedClientScriptPath
        {
            get
            {
                return this.LowerCasedClientScriptPathWithTrailingSlash;
            }
        }

        //- ~NormalizedVirtualPath -//
        internal String NormalizedVirtualPath
        {
            get
            {
                return this.LowerCasedVirtualPathWithTrailingSlash;
            }
        }

        //+
        //- @Ctor -//
        public Host()
        {
            HostingEnvironment.RegisterObject(this);
        }

        //+
        //- $AddPendingCall -//
        private void AddPendingCall()
        {
            Interlocked.Increment(ref this.pendingCallsCount);
        }

        //- @Configure -//
        public void Configure(Server server, Int32 port, String virtualPath, String physicalPath, Boolean requireAuthentication, String instanceId, HostConfiguration configuration)
        {
            this.Configuration = configuration;
            this.InstanceId = instanceId;
            this.Server = server;
            this.Port = port;
            this.InstallPath = null;
            this.VirtualPath = virtualPath;
            this.RequireAuthentication = requireAuthentication;
            this.LowerCasedVirtualPath = CultureInfo.InvariantCulture.TextInfo.ToLower(this.VirtualPath);
            this.LowerCasedVirtualPathWithTrailingSlash = virtualPath.EndsWith("/", StringComparison.Ordinal) ? virtualPath : (virtualPath + "/");
            this.LowerCasedVirtualPathWithTrailingSlash = CultureInfo.InvariantCulture.TextInfo.ToLower(this.LowerCasedVirtualPathWithTrailingSlash);
            this.PhysicalPath = physicalPath;
            this.PhysicalClientScriptPath = HttpRuntime.AspClientScriptPhysicalPath + @"\";
            this.LowerCasedClientScriptPathWithTrailingSlash = CultureInfo.InvariantCulture.TextInfo.ToLower(HttpRuntime.AspClientScriptVirtualPath + "/");
        }

        //- @GetProcessSID -//
        public SecurityIdentifier GetProcessSID()
        {
            using (WindowsIdentity identity = new WindowsIdentity(this.Server.GetProcessToken()))
            {
                return identity.User;
            }
        }

        //- @GetProcessToken -//
        public IntPtr GetProcessToken()
        {
            new SecurityPermission(PermissionState.Unrestricted).Assert();
            return this.Server.GetProcessToken();
        }

        //- @GetProcessUser -//
        public String GetProcessUser()
        {
            return this.Server.GetProcessUser();
        }

        //- @InitializeLifetimeService -//
        public override Object InitializeLifetimeService()
        {
            return null;
        }

        //- @IsVirtualPathAppPath -//
        public Boolean IsVirtualPathAppPath(String path)
        {
            if (path == null)
            {
                return false;
            }
            path = CultureInfo.InvariantCulture.TextInfo.ToLower(path);
            if (!(path == this.LowerCasedVirtualPath))
            {
                return (path == this.LowerCasedVirtualPathWithTrailingSlash);
            }
            return true;
        }

        //- @IsVirtualPathInApp -//
        public Boolean IsVirtualPathInApp(String path)
        {
            Boolean flag;
            return this.IsVirtualPathInApp(path, out flag);
        }

        //- @IsVirtualPathInApp -//
        public Boolean IsVirtualPathInApp(String path, out Boolean isClientScriptPath)
        {
            isClientScriptPath = false;
            if (path != null)
            {
                path = CultureInfo.InvariantCulture.TextInfo.ToLower(path);
                if ((this.VirtualPath == "/") && path.StartsWith("/", StringComparison.Ordinal))
                {
                    if (path.StartsWith(this.LowerCasedClientScriptPathWithTrailingSlash, StringComparison.Ordinal))
                    {
                        isClientScriptPath = true;
                    }
                    return true;
                }
                if (path.StartsWith(this.LowerCasedVirtualPathWithTrailingSlash, StringComparison.Ordinal))
                {
                    return true;
                }
                if (path == this.LowerCasedVirtualPath)
                {
                    return true;
                }
                if (path.StartsWith(this.LowerCasedClientScriptPathWithTrailingSlash, StringComparison.Ordinal))
                {
                    isClientScriptPath = true;
                    return true;
                }
            }
            return false;
        }

        //- @ProcessRequest -//
        public void ProcessRequest(Connection conn)
        {
            this.AddPendingCall();
            try
            {
                new Request(this, conn, this.Configuration).Process();
            }
            finally
            {
                this.RemovePendingCall();
            }
        }

        //- $RemovePendingCall -//
        private void RemovePendingCall()
        {
            Interlocked.Decrement(ref this.pendingCallsCount);
        }

        //- @Shutdown -//
        public void Shutdown()
        {
            HostingEnvironment.InitiateShutdown();
        }

        //- @IRegisteredObject.Stop -//
        void IRegisteredObject.Stop(Boolean immediate)
        {
            if (this.Server != null)
            {
                this.Server.HostStopped();
            }
            this.WaitForPendingCallsToFinish();
            HostingEnvironment.UnregisterObject(this);
        }

        //- ~UpdateConfiguration -//
        internal void UpdateConfiguration(HostConfiguration config)
        {
            if (config.AllowedContentTypes != null)
            {
                this.Configuration.AllowedContentTypes = config.AllowedContentTypes;
            }
            if (config.ContentTypeMappings != null)
            {
                this.Configuration.ContentTypeMappings = config.ContentTypeMappings;
            }
            if (config.DefaultDocuments != null)
            {
                this.Configuration.DefaultDocuments = config.DefaultDocuments;
            }
            //+
            this.Configuration.EnableFaviconTracing = config.EnableFaviconTracing;
            this.Configuration.EnableTracing = config.EnableTracing;
            this.Configuration.EnableVerboseTypeTracing = config.EnableVerboseTypeTracing;
        }

        //- $WaitForPendingCallsToFinish -//
        private void WaitForPendingCallsToFinish()
        {
            while (this.pendingCallsCount > 0)
            {
                Thread.Sleep(250);
            }
        }
    }
}