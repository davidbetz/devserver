using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web;
//+
namespace DevServer.WebCore
{
    internal class Messages
    {
        private static String _dirListingTail = ("</PRE>\r\n            <hr width=100% size=1 color=silver>\r\n\r\n              <b>{0}:</b>&nbsp;{1} " + VersionString + "\r\n\r\n            </font>\r\n\r\n    </body>\r\n</html>\r\n");
        private static String _httpErrorFormat2 = ("    </head>\r\n    <body bgcolor=\"white\">\r\n\r\n            <span><h1>{0}<hr width=100% size=1 color=silver></h1>\r\n\r\n            <h2> <i>{1}</i> </h2></span>\r\n\r\n            <hr width=100% size=1 color=silver>\r\n\r\n            <b>{2}:</b>&nbsp;{3} " + VersionString + "\r\n\r\n            </font>\r\n\r\n    </body>\r\n</html>\r\n");
        public static String VersionString = typeof(Server).Assembly.GetName().Version.ToString();

        //+
        //- @FormatDirectoryListing- //
        public static String FormatDirectoryListing(String dirPath, String parentPath, FileSystemInfo[] elements)
        {
            StringBuilder builder = new StringBuilder();
            String str = string.Format("Directory Listing -- {0}", dirPath);
            String str2 = "Version Information";
            String str3 = "NetFXHarmonics DevServer";
            String str4 = String.Format(CultureInfo.InvariantCulture, _dirListingTail, new Object[] { str2, str3 });
            builder.Append(String.Format(CultureInfo.InvariantCulture, "<html>\r\n    <head>\r\n    <title>{0}</title>\r\n", new Object[] { str }));
            builder.Append("        <style>\r\n        \tbody {font-family:\"Verdana\";font-weight:normal;font-size: 8pt;color:black;} \r\n        \tp {font-family:\"Verdana\";font-weight:normal;color:black;margin-top: -5px}\r\n        \tb {font-family:\"Verdana\";font-weight:bold;color:black;margin-top: -5px}\r\n        \th1 { font-family:\"Verdana\";font-weight:normal;font-size:18pt;color:red }\r\n        \th2 { font-family:\"Verdana\";font-weight:normal;font-size:14pt;color:maroon }\r\n        \tpre {font-family:\"Lucida Console\";font-size: 8pt}\r\n        \t.marker {font-weight: bold; color: black;text-decoration: none;}\r\n        \t.version {color: gray;}\r\n        \t.error {margin-bottom: 10px;}\r\n        \t.expandable { text-decoration:underline; font-weight:bold; color:navy; cursor:hand; }\r\n        </style>\r\n");
            builder.Append(String.Format(CultureInfo.InvariantCulture, "    </head>\r\n    <body bgcolor=\"white\">\r\n\r\n    <h2> <i>{0}</i> </h2></span>\r\n\r\n            <hr width=100% size=1 color=silver>\r\n\r\n<PRE>\r\n", new Object[] { str }));
            if (parentPath != null)
            {
                if (!parentPath.EndsWith("/", StringComparison.Ordinal))
                {
                    parentPath = parentPath + "/";
                }
                builder.Append(String.Format(CultureInfo.InvariantCulture, "<A href=\"{0}\">[To Parent Directory]</A>\r\n\r\n", new Object[] { parentPath }));
            }
            if (elements != null)
            {
                for (Int32 i = 0; i < elements.Length; i++)
                {
                    if (elements[i] is FileInfo)
                    {
                        FileInfo info = (FileInfo)elements[i];
                        builder.Append(String.Format(CultureInfo.InvariantCulture, "{0,38:dddd, MMMM dd, yyyy hh:mm tt} {1,12:n0} <A href=\"{2}\">{3}</A>\r\n", new Object[] { info.LastWriteTime, info.Length, info.Name, info.Name }));
                    }
                    else if (elements[i] is DirectoryInfo)
                    {
                        DirectoryInfo info2 = (DirectoryInfo)elements[i];
                        builder.Append(String.Format(CultureInfo.InvariantCulture, "{0,38:dddd, MMMM dd, yyyy hh:mm tt}        &lt;dir&gt; <A href=\"{1}/\">{2}</A>\r\n", new Object[] { info2.LastWriteTime, info2.Name, info2.Name }));
                    }
                }
            }
            builder.Append(str4);
            return builder.ToString();
        }

        //- @FormatErrorMessageBody- //
        public static String FormatErrorMessageBody(Int32 statusCode, String appName)
        {
            String statusDescription = HttpWorkerRequest.GetStatusDescription(statusCode);
            String text1 = String.Format("Server Error in '{0}' Application.", appName);
            String text2 = String.Format("HTTP Error {0} - {1}.", statusCode, statusDescription);
            String text3 = "Version Information";
            String text4 = "NetFXHarmonics DevServer";
            return (String.Format(CultureInfo.InvariantCulture, "<html>\r\n    <head>\r\n        <title>{0}</title>\r\n", new Object[] { statusDescription }) + "        <style>\r\n        \tbody {font-family:\"Verdana\";font-weight:normal;font-size: 8pt;color:black;} \r\n        \tp {font-family:\"Verdana\";font-weight:normal;color:black;margin-top: -5px}\r\n        \tb {font-family:\"Verdana\";font-weight:bold;color:black;margin-top: -5px}\r\n        \th1 { font-family:\"Verdana\";font-weight:normal;font-size:18pt;color:red }\r\n        \th2 { font-family:\"Verdana\";font-weight:normal;font-size:14pt;color:maroon }\r\n        \tpre {font-family:\"Lucida Console\";font-size: 8pt}\r\n        \t.marker {font-weight: bold; color: black;text-decoration: none;}\r\n        \t.version {color: gray;}\r\n        \t.error {margin-bottom: 10px;}\r\n        \t.expandable { text-decoration:underline; font-weight:bold; color:navy; cursor:hand; }\r\n        </style>\r\n" + String.Format(CultureInfo.InvariantCulture, _httpErrorFormat2, new Object[] { text1, text2, text3, text4 }));
        }
    }
}

