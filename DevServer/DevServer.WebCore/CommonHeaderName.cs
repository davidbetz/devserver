using System;
//+
namespace DevServer.WebCore
{
    //- @CommonHeaderName -//
    public class CommonHeaderName
    {
        public const String CacheControl = "Cache-Control";
        public const String Connection = "Connection";
        public const String Date = "Date";
        public const String KeepAlive = "Keep-Alive";
        public const String Pragma = "Pragma";
        public const String Trailer = "Trailer";
        public const String TransferEncoding = "Transfer-Encoding";
        public const String Upgrade = "Upgrade";
        public const String Via = "Via";
        public const String Warning = "Warning";
        public const String Allow = "Allow";
        public const String ContentLength = "Content-Length";
        public const String ContentType = "Content-Type";
        public const String ContentEncoding = "Content-Encoding";
        public const String ContentLanguage = "Content-Language";
        public const String ContentLocation = "Content-Location";
        public const String ContentMd5 = "Content-MD5";
        public const String ContentRange = "Content-Range";
        public const String Expires = "Expires";
        public const String LastModified = "Last-Modified";
    }

    //- @RequestHeaderName -//
    public class RequestHeaderName
    {
        public const String Accept = "Accept";
        public const String AcceptCharset = "Accept-Charset";
        public const String AcceptEncoding = "Accept-Encoding";
        public const String AcceptLanguage = "Accept-Language";
        public const String Authorization = "Authorization";
        public const String Cookie = "Cookie";
        public const String Expect = "Expect";
        public const String From = "From";
        public const String Host = "Host";
        public const String IfMatch = "If-Match";
        public const String IfModifiedSince = "If-Modified-Since";
        public const String IfNoneMatch = "If-None-Match";
        public const String IfRange = "If-Range";
        public const String IfUnmodifiedSince = "If-Unmodified-Since";
        public const String MaxForwards = "Max-Forwards";
        public const String ProxyAuthorization = "Proxy-Authorization";
        public const String Referer = "Referer";
        public const String Range = "Range";
        //+
        public const String SOAPAction = "SOAPAction";
        public const String Te = "TE";
        public const String UserAgent = "User-Agent";
    }

    //- @ResponseHeaderName -//
    public class ResponseHeaderName
    {
        public const String AcceptRanges = "Accept-Ranges";
        public const String Age = "Age";
        public const String Etag = "ETag";
        public const String Location = "Location";
        public const String ProxyAuthenticate = "Proxy-Authenticate";
        public const String RetryAfter = "Retry-After";
        public const String Server = "Server";
        public const String SetCookie = "Set-Cookie";
        public const String Vary = "Vary";
        public const String WwwAuthenticate = "WWW-Authenticate";
    }
}

