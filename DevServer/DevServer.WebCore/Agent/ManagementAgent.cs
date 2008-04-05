using System;
using System.Linq;
using DevServer.Service;
using DevServer.Service.Client;
using System.Collections.Generic;
using System.Xml;
using DevServer.WebCore.ViewState;
//+
namespace DevServer.WebCore.Agent
{
    public static class ManagementAgent
    {
        //- $ContentType -//
        class ContentType
        {
            //+ Image
            public const String Png = "image/png";
            public const String Gif = "image/gif";
            public const String Jpeg = "image/jpeg";
            //+ Common Document
            public const String Pdf = "application/pdf";
            //+ Common Text
            public const String JavaScript = "text/javascript";
            public const String Css = "text/css";
            public const String Xml = "text/xml";
            public const String Html = "text/html";
            public const String Text = "text/plain";
            //+ Common Text Application
            public const String XHtml = "application/xhtml+xml";
            public const String Xaml = "application/xaml+xml";
            public const String Soap = "application/soap+xml";
            //+ Common Application
            public const String OctetStream = "application/octet-stream";
            public const String Xbap = "application/x-ms-xbap";
            public const String Swf = "application/x-shockwave-flash";
            public const String XJavaScript = "application/x-javascript";
            //+ Service
            public const String Json = "application/json";
            //+ Media
            public const String Asx1 = "video/x-ms-asf-plugin";
            public const String Asx2 = "video/x-mplayer2";
            public const String Asx3 = "video/x-ms-asf";
            public const String Wm = "video/x-ms-wm";
            public const String Wma = "video/x-ms-wma";
            public const String Wax = "video/x-ms-wax";
            public const String Wmv = "video/x-ms-mwv";
            public const String Wvx = "video/x-ms-mvw";
            public const String Mp3 = "audio/mpeg";
        }

        //- $AllowContentTypeViaConfiguration -//
        private static Boolean AllowContentTypeViaConfiguration(List<String> allowedContentTypes, String contentType)
        {
            if (allowedContentTypes.Count(p => p == contentType) > 0)
            {
                return true;
            }
            return false;
        }

        //- $SetViewStateObjects -//
        private static void SetViewStateObjects(DevServer.Service.Request request, Response response)
        {
            State stateTrees;
            if (!String.IsNullOrEmpty(request.Data))
            {
                stateTrees = ViewStateParser.ExtractViewStateTree(request.Data);
                if (stateTrees != null && (stateTrees.ControlState != null || stateTrees.ViewState != null))
                {
                    if (stateTrees.ControlState != null)
                    {
                        request.ControlState = stateTrees.ControlState.InnerXml;
                    }
                    if (stateTrees.ViewState != null)
                    {
                        request.ViewState = stateTrees.ViewState.InnerXml;
                    }
                }
            }
            //+
            if (!String.IsNullOrEmpty(response.Data))
            {
                stateTrees = ViewStateParser.ExtractViewStateTree(response.Data);
                if (stateTrees != null && (stateTrees.ControlState != null || stateTrees.ViewState != null))
                {
                    if (stateTrees.ControlState != null)
                    {
                        response.ControlState = stateTrees.ControlState.InnerXml;
                    }
                    if (stateTrees.ViewState != null)
                    {
                        response.ViewState = stateTrees.ViewState.InnerXml;
                    }
                }
            }
        }

        //- @SubmitRequest -//
        public static void SubmitRequest(String instanceId, HostConfiguration configuration, DevServer.Service.Request request, Response response)
        {
            if (configuration != null && configuration.EnableTracing)
            {
                using (RequestManagementClient client = new RequestManagementClient())
                {
                    String contentType = response.ContentType;
                    if (!String.IsNullOrEmpty(contentType))
                    {
                        Int32 i;
                        if ((i = contentType.IndexOf(";")) > -1)
                        {
                            contentType = contentType.Substring(0, i);
                        }
                    }
                    switch (contentType)
                    {
                        case ContentType.JavaScript:
                        case ContentType.XJavaScript:
                        case ContentType.Css:
                        case ContentType.Xml:
                        case ContentType.Html:
                        case ContentType.Text:
                        case ContentType.Xaml:
                        case ContentType.XHtml:
                        case ContentType.Json:
                        case ContentType.Soap:
                        case ContentType.Asx1:
                        case ContentType.Asx2:
                        case ContentType.Asx3:
                            break;
                        default:
                            if (!AllowContentTypeViaConfiguration(configuration.AllowedContentTypes, response.ContentType))
                            {
                                if (configuration.EnableVerboseTypeTracing)
                                {
                                    response.Data = String.Empty;
                                }
                                else
                                {
                                    return;
                                }
                            }
                            break;
                    }
                    try
                    {
                        SetViewStateObjects(request, response);
                    }
                    catch
                    {
                        //+ this feature isn't that important; we don't need it blowing up in the middle of our work.
                    }
                    //+ Favicon Checking
                    if (request.Url.ToLower().EndsWith("favicon.ico") && !configuration.EnableFaviconTracing)
                    {
                        return;
                    }
                    //+
                    client.SubmitRequest(instanceId, request, response);
                }
            }
        }
    }
}