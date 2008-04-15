//++
//+ Request.cs
//+
//+ Portions of this file were adapted from the Cassini Web Server
//+ copyrighted by Microsoft.
//+
//++
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Web;
using System.Web.Hosting;
using Microsoft.Win32.SafeHandles;
using System.Text.RegularExpressions;
using DevServer.Service;
//+
namespace DevServer.WebCore
{
    internal sealed class Request : SimpleWorkerRequest
    {
        //- ~ByteParser -//
        internal sealed class ByteParser
        {
            //- $Bytes --/
            private Byte[] Bytes { get; set; }

            //- $Position --/
            private Int32 Position { get; set; }

            //- ~ByteParser --/
            internal ByteParser(Byte[] bytes)
            {
                this.Bytes = bytes;
                this.Position = 0;
            }

            //- ~ReadLine --/
            internal ByteString ReadLine()
            {
                ByteString str = null;
                for (Int32 i = this.Position; i < this.Bytes.Length; i++)
                {
                    if (this.Bytes[i] == 10)
                    {
                        Int32 length = i - this.Position;
                        if (length > 0 && (this.Bytes[i - 1] == 13))
                        {
                            length--;
                        }
                        str = new ByteString(this.Bytes, this.Position, length);
                        this.Position = i + 1;
                        return str;
                    }
                }
                if (this.Position < this.Bytes.Length)
                {
                    str = new ByteString(this.Bytes, this.Position, this.Bytes.Length - this.Position);
                }
                this.Position = this.Bytes.Length;
                return str;
            }

            //- ~CurrentOffset --/
            internal Int32 CurrentOffset
            {
                get
                {
                    return this.Position;
                }
            }
        }

        //- ~ByteString -//
        internal sealed class ByteString
        {
            //- @Bytes --/
            public Byte[] Bytes { get; private set; }

            //- @Length --/
            public Int32 Length { get; private set; }

            //- @Offset --/
            public Int32 Offset { get; private set; }

            //- @[Indexer] --/
            public Byte this[Int32 index]
            {
                get
                {
                    return this.Bytes[this.Offset + index];
                }
            }

            //- @ByteString --/
            public ByteString(Byte[] bytes, Int32 offset, Int32 length)
            {
                this.Bytes = bytes;
                if (((this.Bytes != null) && (offset >= 0)) && ((length >= 0) && ((offset + length) <= this.Bytes.Length)))
                {
                    this.Offset = offset;
                    this.Length = length;
                }
            }

            //- @GetBytes --/
            public Byte[] GetBytes()
            {
                Byte[] dst = new Byte[this.Length];
                if (this.Length > 0)
                {
                    Buffer.BlockCopy(this.Bytes, this.Offset, dst, 0, this.Length);
                }
                return dst;
            }

            //- @GetString --/
            public String GetString()
            {
                return this.GetString(Encoding.UTF8);
            }

            //- @GetString --/
            public String GetString(Encoding enc)
            {
                if (this.IsEmpty)
                {
                    return String.Empty;
                }
                return enc.GetString(this.Bytes, this.Offset, this.Length);
            }

            //- @IndexOf --/
            public Int32 IndexOf(Char ch)
            {
                return this.IndexOf(ch, 0);
            }

            //- @IndexOf --/
            public Int32 IndexOf(Char ch, Int32 offset)
            {
                for (Int32 i = offset; i < this.Length; i++)
                {
                    if (this[i] == (Byte)ch)
                    {
                        return i;
                    }
                }
                return -1;
            }

            //- @IsEmpty --/
            public Boolean IsEmpty
            {
                get
                {
                    if (this.Bytes != null)
                    {
                        return this.Length == 0;
                    }
                    return true;
                }
            }

            //- @Split --/
            public ByteString[] Split(Char sep)
            {
                List<ByteString> list = new List<ByteString>();
                Int32 offset = 0;
                while (offset < this.Length)
                {
                    Int32 index = this.IndexOf(sep, offset);
                    if (index < 0)
                    {
                        list.Add(this.Substring(offset));
                        break;
                    }
                    list.Add(this.Substring(offset, index - offset));
                    for (offset = index + 1; (offset < this.Length) && (this[offset] == ((Byte)sep)); offset++)
                    {
                    }
                }
                //Int32 count = list.Count;
                //ByteString[] strArray = new ByteString[count];
                //for (Int32 i = 0; i < count; i++)
                //{
                //    strArray[i] = (ByteString)list[i];
                //}
                return list.ToArray();
            }

            //- @Substring --/
            public ByteString Substring(Int32 offset)
            {
                return this.Substring(offset, this.Length - offset);
            }

            //- @Substring --/
            public ByteString Substring(Int32 offset, Int32 len)
            {
                return new ByteString(this.Bytes, this.Offset + offset, len);
            }
        }

        //+
        private Boolean headersSent;
        private Int32 responseStatus;
        private Boolean isClientScriptPath;
        private static Char[] badPathChars = new char[] { '%', '>', '<', ':', '\\' };
        private static Char[] IntToHex = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };
        private const Int32 MaxChunkLength = 0x10000;
        private const Int32 maxHeaderBytes = 0x8000;
        private static String[] restrictedDirs = new String[] { "/bin", "/app_browsers", "/app_code", "/app_data", "/app_localresources", "/app_globalresources", "/app_webreferences" };

        //+
        //- $AllRawHeaders -//
        private String AllRawHeaders { get; set; }

        //- $Configuration -//
        private HostConfiguration Configuration { get; set; }

        //- $Connection -//
        private Connection Connection { get; set; }

        //- $ConnectionPermission -//
        private IStackWalk ConnectionPermission { get; set; }

        //- $ContentLength -//
        private Int32 ContentLength { get; set; }

        //- $EndHeadersOffset -//
        private Int32 EndHeadersOffset { get; set; }

        //- $FilePath -//
        private String FilePath { get; set; }

        //- $HeaderBytes -//
        private Byte[] HeaderBytes { get; set; }

        //- $Host -//
        private Host Host { get; set; }

        //- $HeaderByteStrings -//
        private ArrayList HeaderByteStrings { get; set; }

        //- $KnownRequestHeaders -//
        private String[] KnownRequestHeaders { get; set; }

        //- $LocalIpAddress -//
        private String LocalIpAddress { get; set; }

        //- $Path -//
        private String Path { get; set; }

        //- $PathInfo -//
        private String PathInfo { get; set; }

        //- $PathTranslated -//
        private String PathTranslated { get; set; }

        //- $PreloadedContent -//
        private Byte[] PreloadedContent { get; set; }

        //- $Protocol -//
        private String Protocol { get; set; }

        //- $QueryString -//
        private String QueryString { get; set; }

        //- $QueryStringBytes -//
        private Byte[] QueryStringBytes { get; set; }

        //- $PreloadedContentLength -//
        private Int32 PreloadedContentLength { get; set; }

        //- $ResponseBodyBytes -//
        private ArrayList ResponseBodyBytes { get; set; }

        //- $ResponseContentLength -//
        private Int32 ResponseContentLength { get; set; }

        //- $RequestContentType -//
        private String RequestContentType { get; set; }

        //- $ResponseContentType -//
        private String ResponseContentType { get; set; }

        //- $ResponseData -//
        private String ResponseData { get; set; }

        //- $RequestData -//
        private String RequestData { get; set; }

        //- $ResponseHeadersBuilder -//
        private StringBuilder ResponseHeadersBuilder { get; set; }

        //- $RequestHeaderList -//
        private List<DevServer.Service.Header> RequestHeaderList { get; set; }

        //- $ResponseHeaderList -//
        private List<DevServer.Service.Header> ResponseHeaderList { get; set; }

        //- $RemoteIpAddress -//
        private String RemoteIpAddress { get; set; }

        //- $SpecialCaseStaticFileHeaders -//
        private Boolean SpecialCaseStaticFileHeaders { get; set; }

        //- $StartHeadersOffset -//
        private Int32 StartHeadersOffset { get; set; }

        //- $UnknownRequestHeaders -//
        private String[][] UnknownRequestHeaders { get; set; }

        //- $Url -//
        private String Url { get; set; }

        //- $Verb -//
        private String Verb { get; set; }

        //- ~InstanceId -//
        internal String InstanceId
        {
            get
            {
                return this.Host.InstanceId;
            }
        }

        //+
        //- @Ctor -//
        public Request(Host host, Connection connection, HostConfiguration configuration)
            : base(String.Empty, String.Empty, null)
        {
            this.Configuration = configuration;
            this.ConnectionPermission = new PermissionSet(PermissionState.Unrestricted);
            this.Host = host;
            this.LocalIpAddress = connection.LocalIP;
            this.RemoteIpAddress = connection.RemoteIP;
            this.Connection = connection;
            this.RequestHeaderList = new List<DevServer.Service.Header>();
            this.ResponseHeaderList = new List<DevServer.Service.Header>();
        }

        //+
        //- @CoseConnection -//
        public override void CloseConnection()
        {
            this.ConnectionPermission.Assert();
            this.Connection.Close();
        }

        //- @EndOfRequest -//
        public override void EndOfRequest()
        {
        }

        //- @FlushResponse -//
        public override void FlushResponse(Boolean finalFlush)
        {
            this.ConnectionPermission.Assert();
            if (!this.headersSent)
            {
                String headers = this.ResponseHeadersBuilder.ToString();
                String headersLowerCase = headers.ToLower().ToString();
                Int32 contentTypeIndex = headersLowerCase.IndexOf("content-type:");
                if (contentTypeIndex > -1)
                {
                    Int32 newLineIndex = headersLowerCase.IndexOf("\r\n", contentTypeIndex);
                    if (newLineIndex == -1)
                    {
                        newLineIndex = headersLowerCase.Length;
                    }
                    //+
                    Int32 extensionIndex = this.Path.LastIndexOf(".");
                    if (extensionIndex > -1)
                    {
                        String extension = this.Path.Substring(extensionIndex, this.Path.Length - extensionIndex);
                        Regex regex = new Regex("\\.([_\\-a-z0-9]+$)", RegexOptions.IgnoreCase);
                        //+ is this an extension or just everything after the last period including slash and other stuff (i.e. .svc/mex)?
                        if (regex.IsMatch(extension))
                        {
                            String contentType = ContentType.GetContentype(extension, this.Configuration.ContentTypeMappings);
                            if (!String.IsNullOrEmpty(contentType))
                            {
                                String before = headers.Substring(0, contentTypeIndex);
                                String after = headers.Substring(newLineIndex + 2, headers.Length - newLineIndex - 2);
                                this.ResponseHeadersBuilder = new StringBuilder(before);
                                this.ResponseHeadersBuilder.Append("Content-Type: " + contentType + "\r\n");
                                this.ResponseHeadersBuilder.Append(after);
                                //+ modify existing header information
                                this.ResponseContentType = contentType;
                                Header header = this.ResponseHeaderList.Find(p => p.Name == "Content-Type");
                                header.Data = contentType;
                            }
                        }
                    }
                }
                this.Connection.WriteHeaders(this.responseStatus, this.ResponseHeadersBuilder.ToString());
                this.headersSent = true;
            }
            for (Int32 i = 0; i < this.ResponseBodyBytes.Count; i++)
            {
                Byte[] data = (Byte[])this.ResponseBodyBytes[i];
                this.Connection.WriteBody(data, 0, data.Length);
                this.ResponseData = ASCIIEncoding.UTF8.GetString(data);
            }
            this.ResponseBodyBytes = new ArrayList();
            if (finalFlush)
            {
                this.Connection.Close();
            }
        }

        //- @GetAppPath -//
        public override String GetAppPath()
        {
            return this.Host.VirtualPath;
        }

        //- @GetAppPathTranslated -//
        public override String GetAppPathTranslated()
        {
            return this.Host.PhysicalPath;
        }

        //- @GetFilePath -//
        public override String GetFilePath()
        {
            return this.FilePath;
        }

        //- @GetFilePathTranslated -//
        public override String GetFilePathTranslated()
        {
            return this.PathTranslated;
        }

        //- @GetHttpVerbName -//
        public override String GetHttpVerbName()
        {
            return this.Verb;
        }

        //- @GetHttpVersion -//
        public override String GetHttpVersion()
        {
            return this.Protocol;
        }

        //- @GetKnownRequestHeader -//
        public override String GetKnownRequestHeader(Int32 index)
        {
            return this.KnownRequestHeaders[index];
        }

        //- @GetLocalAddress -//
        public override String GetLocalAddress()
        {
            this.ConnectionPermission.Assert();
            //+
            return this.Connection.LocalIP;
        }

        //- @GetLocalPort -//
        public override Int32 GetLocalPort()
        {
            return this.Host.Port;
        }

        //- @GetPathInfo -//
        public override String GetPathInfo()
        {
            return this.PathInfo;
        }

        //- @GetPreloadedEntityBody -//
        public override Byte[] GetPreloadedEntityBody()
        {
            return this.PreloadedContent;
        }

        //- @GetQueryString -//
        public override String GetQueryString()
        {
            return this.QueryString;
        }

        //- @GetQueryStringRawBytes -//
        public override Byte[] GetQueryStringRawBytes()
        {
            return this.QueryStringBytes;
        }

        //- @GetRawUrl -//
        public override String GetRawUrl()
        {
            return this.Url;
        }

        //- @GetRemoteAddress -//
        public override String GetRemoteAddress()
        {
            this.ConnectionPermission.Assert();
            return this.Connection.RemoteIP;
        }

        //- GetRemotePort -//
        public override Int32 GetRemotePort()
        {
            return 0;
        }

        //- @GetServerName -//
        public override String GetServerName()
        {
            String localAddress = this.GetLocalAddress();
            if (localAddress.Equals("127.0.0.1"))
            {
                return "localhost";
            }
            //+
            return localAddress;
        }

        //- @GetServerVariable -//
        public override String GetServerVariable(String name)
        {
            String processUser = String.Empty;
            String data = name;
            if (String.IsNullOrEmpty(data))
            {
                return processUser;
            }
            if (!(data == "ALL_RAW"))
            {
                if (data != "SERVER_PROTOCOL")
                {
                    if (data == "LOGON_USER")
                    {
                        if (this.GetUserToken() != IntPtr.Zero)
                        {
                            processUser = this.Host.GetProcessUser();
                        }
                        return processUser;
                    }
                    if ((data == "AUTH_TYPE") && (this.GetUserToken() != IntPtr.Zero))
                    {
                        processUser = "NTLM";
                    }
                    //+
                    return processUser;
                }
            }
            else
            {
                return this.AllRawHeaders;
            }
            //+
            return this.Protocol;
        }

        //- @GetUnknownRequestHeader -//
        public override String GetUnknownRequestHeader(String name)
        {
            Int32 length = this.UnknownRequestHeaders.Length;
            for (Int32 i = 0; i < length; i++)
            {
                if (String.Compare(name, this.UnknownRequestHeaders[i][0], StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return this.UnknownRequestHeaders[i][1];
                }
            }
            //+
            return null;
        }

        //- @GetUnknownRequestHeaders -//
        public override String[][] GetUnknownRequestHeaders()
        {
            return this.UnknownRequestHeaders;
        }

        //- @GetUriPath -//
        public override String GetUriPath()
        {
            return this.Path;
        }

        //- @GetUserToken -//
        public override IntPtr GetUserToken()
        {
            return this.Host.GetProcessToken();
        }

        //- @HeadersSent -//
        public override Boolean HeadersSent()
        {
            return this.headersSent;
        }

        //- $IsBadPath -//
        private Boolean IsBadPath()
        {
            return ((this.Path.IndexOfAny(badPathChars) >= 0) || ((CultureInfo.InvariantCulture.CompareInfo.IndexOf(this.Path, "..", CompareOptions.Ordinal) >= 0) || (CultureInfo.InvariantCulture.CompareInfo.IndexOf(this.Path, "//", CompareOptions.Ordinal) >= 0)));
        }

        //- @IsClientConnected -//
        public override Boolean IsClientConnected()
        {
            this.ConnectionPermission.Assert();
            //+
            return this.Connection.Connected;
        }

        //- @IsEntireEntityBodyIsPreloaded -//
        public override Boolean IsEntireEntityBodyIsPreloaded()
        {
            return this.ContentLength == this.PreloadedContentLength;
        }

        //- $IsRequestForRestrictedDirectory -//
        private Boolean IsRequestForRestrictedDirectory()
        {
            String str = CultureInfo.InvariantCulture.TextInfo.ToLower(this.Path);
            if (this.Host.VirtualPath != "/")
            {
                str = str.Substring(this.Host.VirtualPath.Length);
            }
            foreach (String str2 in restrictedDirs)
            {
                if (str.StartsWith(str2, StringComparison.Ordinal) && ((str.Length == str2.Length) || (str[str2.Length] == '/')))
                {
                    return true;
                }
            }
            //+
            return false;
        }

        //- @MapPath -//
        public override String MapPath(String path)
        {
            String physicalPath = String.Empty;
            Boolean isClientScriptPath = false;
            if ((path == null || path.Length == 0) || path.Equals("/"))
            {
                if (this.Host.VirtualPath == "/")
                {
                    physicalPath = this.Host.PhysicalPath;
                }
                else
                {
                    physicalPath = Environment.SystemDirectory;
                }
            }
            else if (this.Host.IsVirtualPathAppPath(path))
            {
                physicalPath = this.Host.PhysicalPath;
            }
            else if (this.Host.IsVirtualPathInApp(path, out isClientScriptPath))
            {
                if (isClientScriptPath)
                {
                    physicalPath = this.Host.PhysicalClientScriptPath + path.Substring(this.Host.NormalizedClientScriptPath.Length);
                }
                else
                {
                    physicalPath = this.Host.PhysicalPath + path.Substring(this.Host.NormalizedVirtualPath.Length);
                }
            }
            else if (path.StartsWith("/", StringComparison.Ordinal))
            {
                physicalPath = this.Host.PhysicalPath + path.Substring(1);
            }
            else
            {
                physicalPath = this.Host.PhysicalPath + path;
            }
            physicalPath = physicalPath.Replace('/', '\\');
            if (physicalPath.EndsWith(@"\", StringComparison.Ordinal) && !physicalPath.EndsWith(@":\", StringComparison.Ordinal))
            {
                physicalPath = physicalPath.Substring(0, physicalPath.Length - 1);
            }
            //+
            return physicalPath;
        }

        //- $ParseHeaders -//
        private void ParseHeaders()
        {
            this.KnownRequestHeaders = new String[40];
            ArrayList list = new ArrayList();
            for (Int32 i = 1; i < this.HeaderByteStrings.Count; i++)
            {
                String str = ((ByteString)this.HeaderByteStrings[i]).GetString();
                Int32 index = str.IndexOf(':');
                if (index >= 0)
                {
                    String header = str.Substring(0, index).Trim();
                    String data = str.Substring(index + 1).Trim();
                    Int32 knownRequestHeaderIndex = HttpWorkerRequest.GetKnownRequestHeaderIndex(header);
                    if (knownRequestHeaderIndex >= 0)
                    {
                        this.KnownRequestHeaders[knownRequestHeaderIndex] = data;
                    }
                    else
                    {
                        list.Add(header);
                        list.Add(data);
                    }
                    if (header == CommonHeaderName.ContentType)
                    {
                        this.RequestContentType = data;
                    }
                    this.RequestHeaderList.Add(new DevServer.Service.Header
                    {
                        Name = header,
                        Data = data
                    });
                }
            }
            Int32 num4 = list.Count / 2;
            this.UnknownRequestHeaders = new String[num4][];
            Int32 num5 = 0;
            for (Int32 j = 0; j < num4; j++)
            {
                this.UnknownRequestHeaders[j] = new String[] { (String)list[num5++], (String)list[num5++] };
            }
            if (this.HeaderByteStrings.Count > 1)
            {
                this.AllRawHeaders = Encoding.UTF8.GetString(this.HeaderBytes, this.StartHeadersOffset, this.EndHeadersOffset - this.StartHeadersOffset);
            }
            else
            {
                this.AllRawHeaders = String.Empty;
            }
        }

        //- $ParsePostedContent -//
        private void ParsePostedContent()
        {
            this.ContentLength = 0;
            this.PreloadedContentLength = 0;
            String s = this.KnownRequestHeaders[11];
            if (s != null)
            {
                try
                {
                    this.ContentLength = Int32.Parse(s, CultureInfo.InvariantCulture);
                }
                catch
                {
                }
            }
            if (this.HeaderBytes.Length > this.EndHeadersOffset)
            {
                this.PreloadedContentLength = this.HeaderBytes.Length - this.EndHeadersOffset;
                if (this.PreloadedContentLength > this.ContentLength)
                {
                    this.PreloadedContentLength = this.ContentLength;
                }
                if (this.PreloadedContentLength > 0)
                {
                    this.PreloadedContent = new Byte[this.PreloadedContentLength];
                    Buffer.BlockCopy(this.HeaderBytes, this.EndHeadersOffset, this.PreloadedContent, 0, this.PreloadedContentLength);
                }
            }
            if (this.PreloadedContent != null)
            {
                this.RequestData = ASCIIEncoding.UTF8.GetString(this.PreloadedContent);
            }
        }

        //- $ParseRequestLine -//
        private void ParseRequestLine()
        {
            ByteString[] strArray = ((ByteString)this.HeaderByteStrings[0]).Split(' ');
            if (((strArray == null) || (strArray.Length < 2)) || (strArray.Length > 3))
            {
                String headers;
                this.ResponseData = this.Connection.WriteErrorAndClose(400, out headers);
                this.responseStatus = 100;
                AddHeadersToResponseHeaderList(headers);
            }
            else
            {
                this.Verb = strArray[0].GetString();
                ByteString str2 = strArray[1];
                this.Url = str2.GetString();
                if (this.Url.IndexOf(Convert.ToChar(0xfffd)) >= 0)
                {
                    this.Url = str2.GetString(Encoding.Default);
                }
                if (strArray.Length == 3)
                {
                    this.Protocol = strArray[2].GetString();
                }
                else
                {
                    this.Protocol = "HTTP/1.0";
                }
                Int32 index = str2.IndexOf('?');
                if (index > 0)
                {
                    this.QueryStringBytes = str2.Substring(index + 1).GetBytes();
                }
                else
                {
                    this.QueryStringBytes = new Byte[0];
                }
                index = this.Url.IndexOf('?');
                if (index > 0)
                {
                    this.Path = this.Url.Substring(0, index);
                    this.QueryString = this.Url.Substring(index + 1);
                }
                else
                {
                    this.Path = this.Url;
                    this.QueryString = String.Empty;
                }
                if (this.Path.IndexOf('%') >= 0)
                {
                    this.Path = HttpUtility.UrlDecode(this.Path, Encoding.UTF8);
                    index = this.Url.IndexOf('?');
                    if (index >= 0)
                    {
                        this.Url = this.Path + this.Url.Substring(index);
                    }
                    else
                    {
                        this.Url = this.Path;
                    }
                }
                Int32 startIndex = this.Path.LastIndexOf('.');
                Int32 num3 = this.Path.LastIndexOf('/');
                if (((startIndex >= 0) && (num3 >= 0)) && (startIndex < num3))
                {
                    Int32 length = this.Path.IndexOf('/', startIndex);
                    this.FilePath = this.Path.Substring(0, length);
                    this.PathInfo = this.Path.Substring(length);
                }
                else
                {
                    this.FilePath = this.Path;
                    this.PathInfo = String.Empty;
                }
                this.PathTranslated = this.MapPath(this.FilePath);
            }
        }

        //- $PrepareResponse -//
        private void PrepareResponse()
        {
            this.headersSent = false;
            this.responseStatus = 200;
            this.ResponseHeadersBuilder = new StringBuilder();
            this.ResponseBodyBytes = new ArrayList();
            this.ResponseHeaderList = new List<DevServer.Service.Header>();
            this.ResponseData = String.Empty;
            this.ResponseContentType = String.Empty;
            this.ResponseContentLength = 0;
        }

        //- @Process -//
        public void Process()
        {
            if (this.TryParseRequest())
            {
                if (((this.Verb == "POST") && (this.ContentLength > 0)) && (this.PreloadedContentLength < this.ContentLength))
                {
                    String headers;
                    this.ResponseData = this.Connection.Write100Continue(out headers);
                    this.responseStatus = 100;
                    AddHeadersToResponseHeaderList(headers);
                }
                if (!this.Host.RequireAuthentication)
                {
                    if (this.isClientScriptPath)
                    {
                        String headers;
                        this.ResponseData = this.Connection.WriteEntireResponseFromFile(this.Host.PhysicalClientScriptPath + this.Path.Substring(this.Host.NormalizedClientScriptPath.Length), false, out this.responseStatus, out headers, this.Configuration.ContentTypeMappings);
                        this.responseStatus = 100;
                        AddHeadersToResponseHeaderList(headers);
                    }
                    else if (this.IsRequestForRestrictedDirectory())
                    {
                        String headers;
                        this.ResponseData = this.Connection.WriteErrorAndClose(0x193, out headers);
                        this.responseStatus = 0x193;
                        AddHeadersToResponseHeaderList(headers);
                    }
                    else if (!this.ProcessDirectoryListingRequest())
                    {
                        this.PrepareResponse();
                        try
                        {
                            HttpRuntime.ProcessRequest(this);
                        }
                        catch
                        {
                        }
                    }
                }
                //+
                SubmitRequest();
            }
        }

        //- $SubmitRequest -//
        private void SubmitRequest()
        {
            DevServer.WebCore.Agent.ManagementAgent.SubmitRequest(this.InstanceId, this.Configuration,
                new DevServer.Service.Request
                {
                    Url = this.Url,
                    DateTime = DateTime.Now,
                    ContentLength = this.ContentLength,
                    Data = this.RequestData,
                    HeaderList = this.RequestHeaderList,
                    IPAddress = this.RemoteIpAddress,
                    StatusCode = this.responseStatus,
                    Verb = this.Verb,
                    ContentType = this.RequestContentType,

                },
                new DevServer.Service.Response
                {
                    ContentLength = this.ResponseContentLength > 0 ? this.ResponseContentLength : this.ResponseData.Length,
                    HeaderList = this.ResponseHeaderList,
                    Data = this.ResponseData,
                    ContentType = this.ResponseContentType
                }
            );
        }

        //- AddHeadersToResponseHeaderList -//
        private void AddHeadersToResponseHeaderList(String headers)
        {
            if (!String.IsNullOrEmpty(headers))
            {
                headers = headers.Replace("\r", "");
                String[] headerSet = headers.Split('\n');
                foreach (String header in headerSet)
                {
                    String[] headerParts = header.Split(':');
                    if (headerParts.Length == 2)
                    {
                        this.ResponseHeaderList.Add(new DevServer.Service.Header
                        {
                            Name = headerParts[0],
                            Data = headerParts[1]
                        });
                    }
                }
            }
        }

        //- $ProcessDirectoryListingRequest -//
        private Boolean ProcessDirectoryListingRequest()
        {
            if (this.Verb != "GET")
            {
                return false;
            }
            //+
            String path = this.PathTranslated;
            if (this.PathInfo.Length > 0)
            {
                path = this.MapPath(this.Path);
            }
            if (!Directory.Exists(path))
            {
                return false;
            }
            String headers;
            if (!this.Path.EndsWith("/", StringComparison.Ordinal))
            {
                String str2 = this.Path + "/";
                String location = UrlEncodeRedirect(str2);
                String extraHeaders = "Location: " + location + "\r\n";
                String body = "<html><head><title>Object moved</title></head><body>\r\n<h2>Object moved to <a href='" + str2 + "'>here</a>.</h2>\r\n</body></html>\r\n";
                //+ add headers
                this.ResponseContentType = "text/html; charset=utf-8";
                this.ResponseHeaderList.Add(new DevServer.Service.Header
                {
                    Name = "Location",
                    Data = location
                });
                this.ResponseHeaderList.Add(new DevServer.Service.Header
                {
                    Name = "Content-type",
                    Data = this.ResponseContentType
                });
                this.ResponseData = this.Connection.WriteEntireResponseFromString(0x12e, extraHeaders, body, false, out headers);
                AddHeadersToResponseHeaderList(headers);
                this.responseStatus = 0x12e;
                //+
                return true;
            }
            //+
            foreach (String defaultFileName in this.Configuration.DefaultDocuments)
            {
                String fullFileName = path + @"\" + defaultFileName;
                if (File.Exists(fullFileName))
                {
                    this.Path = this.Path + defaultFileName;
                    this.FilePath = this.Path;
                    this.Url = (this.QueryString != null) ? (this.Path + "?" + this.QueryString) : this.Path;
                    this.PathTranslated = fullFileName;
                    //+
                    return false;
                }
            }
            //+
            FileSystemInfo[] elements = null;
            try
            {
                elements = new DirectoryInfo(path).GetFileSystemInfos();
            }
            catch
            {
            }
            String str7 = null;
            if (this.Path.Length > 1)
            {
                Int32 length = this.Path.LastIndexOf('/', this.Path.Length - 2);
                str7 = (length > 0) ? this.Path.Substring(0, length) : "/";
                if (!this.Host.IsVirtualPathInApp(str7))
                {
                    str7 = null;
                }
            }
            //+
            this.ResponseData = this.Connection.WriteEntireResponseFromString(200, "Content-type: text/html; charset=utf-8\r\n", Messages.FormatDirectoryListing(this.Path, str7, elements), false, out headers);
            AddHeadersToResponseHeaderList(headers);
            //+
            this.ResponseContentType = "text/html; charset=utf-8";
            this.responseStatus = 200;
            //+
            return true;
        }

        //- $ReadAllHeaders -//
        private void ReadAllHeaders()
        {
            this.HeaderBytes = null;
            do
            {
                if (!this.TryReadAllHeaders())
                {
                    return;
                }
            }
            while (this.EndHeadersOffset < 0);
        }

        //- @ReadEntityBody -//
        public override Int32 ReadEntityBody(Byte[] buffer, Int32 size)
        {
            Int32 count = 0;
            this.ConnectionPermission.Assert();
            Byte[] src = this.Connection.ReadRequestBytes(size);
            if (src != null && src.Length > 0)
            {
                count = src.Length;
                Buffer.BlockCopy(src, 0, buffer, 0, count);
            }
            //+
            return count;
        }

        //- $Reset -//
        private void Reset()
        {
            this.StartHeadersOffset = 0;
            this.HeaderBytes = null;
            this.EndHeadersOffset = 0;
            this.HeaderByteStrings = null;
            this.isClientScriptPath = false;
            this.Verb = null;
            this.Url = null;
            this.Protocol = null;
            this.Path = null;
            this.FilePath = null;
            this.PathInfo = null;
            this.PathTranslated = null;
            this.QueryString = null;
            this.QueryStringBytes = null;
            this.ContentLength = 0;
            this.PreloadedContentLength = 0;
            this.PreloadedContent = null;
            this.AllRawHeaders = null;
            this.UnknownRequestHeaders = null;
            this.KnownRequestHeaders = null;
            this.SpecialCaseStaticFileHeaders = false;
        }

        //- @SendCalculatedContentLength -//
        public override void SendCalculatedContentLength(Int32 contentLength)
        {
            if (!this.headersSent)
            {
                this.ResponseHeadersBuilder.Append("Content-Length: ");
                this.ResponseContentLength = contentLength;
                this.ResponseHeadersBuilder.Append(contentLength.ToString(CultureInfo.InvariantCulture));
                this.ResponseHeadersBuilder.Append("\r\n");
            }
        }

        //- @SendKnownResponseHeader -//
        public override void SendKnownResponseHeader(Int32 index, String value)
        {
            if (!this.headersSent)
            {
                switch (index)
                {
                    case 1:
                    case 2:
                    case 0x1a:
                        return;

                    case 0x12:
                    case 0x13:
                        if (!this.SpecialCaseStaticFileHeaders)
                        {
                            break;
                        }
                        return;

                    case 20:
                        if (!(value == "bytes"))
                        {
                            break;
                        }
                        this.SpecialCaseStaticFileHeaders = true;
                        return;
                }
                String name = HttpWorkerRequest.GetKnownResponseHeaderName(index);
                if (name == CommonHeaderName.ContentType)
                {
                    this.ResponseContentType = value;
                }
                this.ResponseHeadersBuilder.Append(name);
                this.ResponseHeadersBuilder.Append(": ");
                this.ResponseHeadersBuilder.Append(value);
                this.ResponseHeadersBuilder.Append("\r\n");
                //+
                ResponseHeaderList.Add(new DevServer.Service.Header
                {
                    Name = name,
                    Data = value
                });
            }
        }

        //- @SendResponseFromFile -//
        public override void SendResponseFromFile(IntPtr handle, Int64 offset, Int64 length)
        {
            if (length != 0L)
            {
                FileStream f = null;
                try
                {
                    SafeFileHandle handle2 = new SafeFileHandle(handle, false);
                    f = new FileStream(handle2, FileAccess.Read);
                    this.SendResponseFromFileStream(f, offset, length);
                }
                finally
                {
                    if (f != null)
                    {
                        f.Close();
                        f = null;
                    }
                }
            }
        }

        //- @SendResponseFromFile -//
        public override void SendResponseFromFile(String filename, Int64 offset, Int64 length)
        {
            if (length != 0L)
            {
                FileStream f = null;
                try
                {
                    f = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
                    this.SendResponseFromFileStream(f, offset, length);
                }
                finally
                {
                    if (f != null)
                    {
                        f.Close();
                    }
                }
            }
        }

        //- $SendResponseFromFileStream -//
        private void SendResponseFromFileStream(FileStream f, Int64 offset, Int64 length)
        {
            Int64 num = f.Length;
            if (length == -1L)
            {
                length = num - offset;
            }
            if (((length != 0L) && (offset >= 0L)) && (length <= (num - offset)))
            {
                if (offset > 0L)
                {
                    f.Seek(offset, SeekOrigin.Begin);
                }
                if (length <= 0x10000L)
                {
                    Byte[] buffer = new Byte[(int)length];
                    Int32 num2 = f.Read(buffer, 0, (int)length);
                    this.SendResponseFromMemory(buffer, num2);
                }
                else
                {
                    Byte[] buffer2 = new Byte[0x10000];
                    Int32 num3 = (int)length;
                    while (num3 > 0)
                    {
                        Int32 count = (num3 < 0x10000) ? num3 : 0x10000;
                        Int32 num5 = f.Read(buffer2, 0, count);
                        this.SendResponseFromMemory(buffer2, num5);
                        num3 -= num5;
                        if ((num3 > 0) && (num5 > 0))
                        {
                            this.FlushResponse(false);
                        }
                    }
                }
            }
        }

        //- @SendResponseFromMemory -//
        public override void SendResponseFromMemory(Byte[] data, Int32 length)
        {
            if (length > 0)
            {
                Byte[] dst = new Byte[length];
                Buffer.BlockCopy(data, 0, dst, 0, length);
                this.ResponseBodyBytes.Add(dst);
            }
        }

        //- @SendStatus -//
        public override void SendStatus(Int32 statusCode, String statusDescription)
        {
            this.responseStatus = statusCode;
        }

        //- @SendUnknownResponseHeader -//
        public override void SendUnknownResponseHeader(String name, String value)
        {
            if (!this.headersSent)
            {
                this.ResponseHeadersBuilder.Append(name);
                this.ResponseHeadersBuilder.Append(": ");
                this.ResponseHeadersBuilder.Append(value);
                this.ResponseHeadersBuilder.Append("\r\n");
                //+
                ResponseHeaderList.Add(new DevServer.Service.Header
                {
                    Name = name,
                    Data = value
                });
            }
        }

        //- $SkipAllPostedContent -//
        private void SkipAllPostedContent()
        {
            if ((this.ContentLength > 0) && (this.PreloadedContentLength < this.ContentLength))
            {
                Byte[] buffer;
                for (Int32 i = this.ContentLength - this.PreloadedContentLength; i > 0; i -= buffer.Length)
                {
                    buffer = this.Connection.ReadRequestBytes(i);
                    if ((buffer == null) || (buffer.Length == 0))
                    {
                        return;
                    }
                }
            }
        }

        //- $TryParseRequest -//
        private Boolean TryParseRequest()
        {
            this.Reset();
            this.ReadAllHeaders();
            if (!this.Connection.IsLocal)
            {
                // this._connection.WriteErrorAndClose(0x193);
                // return false;
            }
            if (((this.HeaderBytes == null) || (this.EndHeadersOffset < 0)) || ((this.HeaderByteStrings == null) || (this.HeaderByteStrings.Count == 0)))
            {
                String headers;
                this.ResponseData = this.Connection.WriteErrorAndClose(400, out headers);
                this.responseStatus = 400;
                AddHeadersToResponseHeaderList(headers);
                return false;
            }
            this.ParseRequestLine();
            if (this.IsBadPath())
            {
                String headers;
                this.ResponseData = this.Connection.WriteErrorAndClose(400, out headers);
                this.responseStatus = 400;
                AddHeadersToResponseHeaderList(headers);
                return false;
            }
            if (!this.Host.IsVirtualPathInApp(this.Path, out this.isClientScriptPath))
            {
                String headers;
                this.ResponseData = this.Connection.WriteErrorAndClose(0x194, out headers);
                this.responseStatus = 0x194;
                AddHeadersToResponseHeaderList(headers);
                return false;
            }
            this.ParseHeaders();
            this.ParsePostedContent();
            //+
            return true;
        }

        //- $TryReadAllHeaders -//
        private Boolean TryReadAllHeaders()
        {
            Byte[] src = this.Connection.ReadRequestBytes(0x8000);
            if ((src == null) || (src.Length == 0))
            {
                return false;
            }
            if (this.HeaderBytes != null)
            {
                Int32 num = src.Length + this.HeaderBytes.Length;
                if (num > 0x8000)
                {
                    return false;
                }
                Byte[] dst = new Byte[num];
                Buffer.BlockCopy(this.HeaderBytes, 0, dst, 0, this.HeaderBytes.Length);
                Buffer.BlockCopy(src, 0, dst, this.HeaderBytes.Length, src.Length);
                this.HeaderBytes = dst;
            }
            else
            {
                this.HeaderBytes = src;
            }
            //+
            this.StartHeadersOffset = -1;
            this.EndHeadersOffset = -1;
            this.HeaderByteStrings = new ArrayList();
            //+
            ByteParser parser = new ByteParser(this.HeaderBytes);
            while (true)
            {
                ByteString str = parser.ReadLine();
                if (str == null)
                {
                    break;
                }
                if (this.StartHeadersOffset < 0)
                {
                    this.StartHeadersOffset = parser.CurrentOffset;
                }
                if (str.IsEmpty)
                {
                    this.EndHeadersOffset = parser.CurrentOffset;
                    break;
                }
                //+
                this.HeaderByteStrings.Add(str);
            }
            return true;
        }

        //- $UrlEncodeRedirect -//
        private static String UrlEncodeRedirect(String path)
        {
            Byte[] bytes = Encoding.UTF8.GetBytes(path);
            Int32 length = bytes.Length;
            Int32 n2 = 0;
            for (Int32 i = 0; i < length; i++)
            {
                if ((bytes[i] & 0x80) != 0)
                {
                    n2++;
                }
            }
            if (n2 > 0)
            {
                Byte[] buffer = new Byte[length + (n2 * 2)];
                Int32 n4 = 0;
                for (Int32 j = 0; j < length; j++)
                {
                    Byte num6 = bytes[j];
                    if ((num6 & 0x80) == 0)
                    {
                        buffer[n4++] = num6;
                    }
                    else
                    {
                        buffer[n4++] = 0x25;
                        buffer[n4++] = (Byte)IntToHex[(num6 >> 4) & 15];
                        buffer[n4++] = (Byte)IntToHex[num6 & 15];
                    }
                }
                path = Encoding.ASCII.GetString(buffer);
            }
            if (path.IndexOf(' ') >= 0)
            {
                path = path.Replace(" ", "%20");
            }
            //+
            return path;
        }
    }
}