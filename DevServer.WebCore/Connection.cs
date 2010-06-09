//++
//+ Connection.cs
//+
//+ Portions of this file were adapted from the Cassini Web Server
//+ copyrighted by Microsoft.
//+
//++
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Web;
//+
namespace DevServer.WebCore
{
    internal sealed class Connection : MarshalByRefObject
    {
        private static String localServerIP;

        //- $Server -//
        private Server Server { get; set; }

        //- $Socket -//
        private Socket Socket { get; set; }

        //- ~Ctor -//
        internal Connection(Server server, Socket socket)
        {
            this.Server = server;
            this.Socket = socket;
        }

        //- ~Close -//
        internal void Close()
        {
            try
            {
                this.Socket.Shutdown(SocketShutdown.Both);
                this.Socket.Close();
            }
            catch
            {
            }
            finally
            {
                this.Socket = null;
            }
        }

        //- $GetErrorResponseBody -//
        private String GetErrorResponseBody(Int32 statusCode, String message)
        {
            String str = Messages.FormatErrorMessageBody(statusCode, this.Server.VirtualPath);
            if ((message != null) && (message.Length > 0))
            {
                str = str + "\r\n<!--\r\n" + message + "\r\n-->";
            }
            return str;
        }

        //- @InitializeLifetimeService -//
        public override Object InitializeLifetimeService()
        {
            return null;
        }

        //- $MakeContentTypeHeader -//
        private static String MakeContentTypeHeader(String fileName, Dictionary<String, String> contentTypeMappings)
        {
            String str = null;
            FileInfo info = new FileInfo(fileName);
            str = ContentType.GetContentype(info.Extension.ToLowerInvariant(), contentTypeMappings);
            if (str == null)
            {
                return null;
            }
            return "Content-Type: " + str + "\r\n";
        }

        //- $MakeResponseHeaders -//
        private static String MakeResponseHeaders(Int32 statusCode, String moreHeaders, Int32 contentLength, Boolean keepAlive)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(String.Concat(new Object[] { "HTTP/1.1 ", statusCode, " ", HttpWorkerRequest.GetStatusDescription(statusCode), "\r\n" }));
            builder.Append("Server: NetFXHarmonics DevServer /" + Messages.VersionString + "\r\n");
            builder.Append("Date: " + DateTime.Now.ToUniversalTime().ToString("R", DateTimeFormatInfo.InvariantInfo) + "\r\n");
            if (contentLength >= 0)
            {
                builder.Append("Content-Length: " + contentLength + "\r\n");
            }
            if (moreHeaders != null)
            {
                builder.Append(moreHeaders);
            }
            if (!keepAlive)
            {
                builder.Append("Connection: Close\r\n");
            }
            builder.Append("\r\n");
            return builder.ToString();
        }

        //- ~ReadRequestBytes -//
        internal Byte[] ReadRequestBytes(Int32 maxBytes)
        {
            try
            {
                if (this.WaitForRequestBytes() == 0)
                {
                    return null;
                }
                //+
                Int32 available = this.Socket.Available;
                if (available > maxBytes)
                {
                    available = maxBytes;
                }
                Int32 count = 0;
                Byte[] buffer = new Byte[available];
                if (available > 0)
                {
                    count = this.Socket.Receive(buffer, 0, available, SocketFlags.None);
                }
                if (count < available)
                {
                    Byte[] dst = new Byte[count];
                    if (count > 0)
                    {
                        Buffer.BlockCopy(buffer, 0, dst, 0, count);
                    }
                    buffer = dst;
                }
                //+
                return buffer;
            }
            catch
            {
                return null;
            }
        }

        //- ~WaitForRequestBytes -//
        internal Int32 WaitForRequestBytes()
        {
            Int32 available = 0;
            try
            {
                if (this.Socket.Available == 0)
                {
                    this.Socket.Poll(0x186a0, SelectMode.SelectRead);
                    if ((this.Socket.Available == 0) && this.Socket.Connected)
                    {
                        this.Socket.Poll(0x1c9c380, SelectMode.SelectRead);
                    }
                }
                available = this.Socket.Available;
            }
            catch
            {
            }
            //+
            return available;
        }

        //- ~Write100Continue -//
        internal String Write100Continue(out String headers)
        {
            return this.WriteEntireResponseFromString(100, null, null, true, out headers);
        }

        //- $AddHeadersToResponseHeaderList -//
        private void AddHeadersToResponseHeaderList(String headers)
        {
        }

        //- ~WriteBody -//
        internal String WriteBody(Byte[] data, Int32 offset, Int32 length)
        {
            try
            {
                this.Socket.Send(data, offset, length, SocketFlags.None);
                //+
                MemoryStream stream = new MemoryStream();
                stream.Read(data, offset, length);
                return ASCIIEncoding.UTF8.GetString(stream.ToArray());
            }
            catch (SocketException)
            {
                return String.Empty;
            }
        }

        //- ~WriteEntireResponseFromFile -//
        internal String WriteEntireResponseFromFile(String fileName, Boolean keepAlive, out Int32 statusCode, out String headers, Dictionary<String, String> contentTypeMappings)
        {
            if (!System.IO.File.Exists(fileName))
            {
                statusCode = 0x194;
                return this.WriteErrorAndClose(0x194, out headers);
            }
            else
            {
                String moreHeaders = MakeContentTypeHeader(fileName, contentTypeMappings);
                if (moreHeaders == null)
                {
                    statusCode = 0x193;
                    return this.WriteErrorAndClose(0x193, out headers);
                }
                else
                {
                    Boolean flag = false;
                    FileStream stream = null;
                    try
                    {
                        stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                        //+
                        Int32 length = (int)stream.Length;
                        Byte[] buffer = new Byte[length];
                        Int32 contentLength = stream.Read(buffer, 0, length);
                        //+
                        headers = MakeResponseHeaders(200, moreHeaders, contentLength, keepAlive);
                        this.Socket.Send(Encoding.UTF8.GetBytes(headers));
                        this.Socket.Send(buffer, 0, contentLength, SocketFlags.None);
                        flag = true;
                        //+
                        MemoryStream memoryStream = new MemoryStream();
                        memoryStream.Read(buffer, 0, contentLength);
                        statusCode = 200;
                        //+
                        return String.Format("{0}\n{1}", headers, ASCIIEncoding.UTF8.GetString(memoryStream.ToArray()));
                    }
                    catch (SocketException)
                    {
                        statusCode = 500;
                        headers = String.Empty;
                        //+
                        return String.Empty;
                    }
                    finally
                    {
                        if (!keepAlive || !flag)
                        {
                            this.Close();
                        }
                        if (stream != null)
                        {
                            stream.Close();
                        }
                    }
                }
            }
        }

        //- ~WriteEntireResponseFromString -//
        internal String WriteEntireResponseFromString(Int32 statusCode, String headersToSet, String body, Boolean keepAlive, out String headers)
        {
            try
            {
                Int32 contentLength = (body != null) ? Encoding.UTF8.GetByteCount(body) : 0;
                headers = MakeResponseHeaders(statusCode, headersToSet, contentLength, keepAlive);
                this.Socket.Send(Encoding.UTF8.GetBytes(headers + body));
                //+
                return headers + body;
            }
            catch (SocketException)
            {
                headers = "";
                //+
                return String.Empty;
            }
            finally
            {
                if (!keepAlive)
                {
                    this.Close();
                }
            }
        }

        //- ~WriteErrorAndClose -//
        internal String WriteErrorAndClose(Int32 statusCode, out String headers)
        {
            return this.WriteErrorAndClose(statusCode, null, out headers);
        }

        //- ~WriteErrorAndClose -//
        internal String WriteErrorAndClose(Int32 statusCode, String message, out String headers)
        {
            return this.WriteEntireResponseFromString(statusCode, null, this.GetErrorResponseBody(statusCode, message), false, out headers);
        }

        //- ~WriteErrorWithExtraHeadersAndKeepAlive -//
        internal String WriteErrorWithExtraHeadersAndKeepAlive(Int32 statusCode, String headersToSet, out String headers)
        {
            return this.WriteEntireResponseFromString(statusCode, headersToSet, this.GetErrorResponseBody(statusCode, null), true, out headers);
        }

        //- ~WriteHeaders -//
        internal void WriteHeaders(Int32 statusCode, String extraHeaders)
        {
            String s = MakeResponseHeaders(statusCode, extraHeaders, -1, false);
            try
            {
                this.Socket.Send(Encoding.UTF8.GetBytes(s));
            }
            catch (SocketException)
            {
            }
        }

        //- ~Connected -//
        internal Boolean Connected
        {
            get
            {
                return this.Socket.Connected;
            }
        }

        //- ~IsLocal -//
        internal Boolean IsLocal
        {
            get
            {
                String remoteIP = this.RemoteIP;
                //+
                return remoteIP.Equals("127.0.0.1") || LocalServerIP.Equals(remoteIP);
            }
        }

        //- ~LocalIP -//
        internal String LocalIP
        {
            get
            {
                IPEndPoint localEndPoint = (IPEndPoint)this.Socket.LocalEndPoint;
                if (localEndPoint != null && localEndPoint.Address != null)
                {
                    return localEndPoint.Address.ToString();
                }
                return "127.0.0.1";
            }
        }

        //- $LocalServerIP -//
        private static String LocalServerIP
        {
            get
            {
                if (localServerIP == null)
                {
                    localServerIP = Dns.GetHostEntry(Environment.MachineName).AddressList[0].ToString();
                }
                //+
                return localServerIP;
            }
        }

        //- ~RemoteIP -//
        internal String RemoteIP
        {
            get
            {
                IPEndPoint remoteEndPoint = (IPEndPoint)this.Socket.RemoteEndPoint;
                if (remoteEndPoint != null && remoteEndPoint.Address != null)
                {
                    return remoteEndPoint.Address.ToString();
                }
                //+
                return "127.0.0.1";
            }
        }
    }
}