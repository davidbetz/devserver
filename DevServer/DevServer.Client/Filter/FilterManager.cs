using System;
//+
namespace DevServer.Client.Filter
{
    internal static class FilterManager
    {
        //- ~UpdateFilter -//
        internal static Boolean UpdateFilter(RequestResponseSet set, String filter)
        {
            //++
            // syntax: verb:POST;statuscode:200;file:css;contentType:text/css
            //         -- syntax is NOT case sensitive
            //++
            Boolean result = true;
            if (!String.IsNullOrEmpty(filter))
            {
                filter = filter.Replace(" ", "");
                filter = filter.ToLower();
                String[] segments = filter.Split(';');
                //+
                foreach (String segment in segments)
                {
                    if (segment.Contains(":"))
                    {
                        Boolean temp = true;
                        String[] parts = segment.Split(':');
                        if (parts.Length == 2)
                        {
                            switch (parts[0])
                            {
                                case "verb":
                                    try
                                    {
                                        temp = FilterManager.ApplyVerbFilter(set, parts[1]);
                                    }
                                    catch
                                    {
                                        continue;
                                    }
                                    break;
                                case "status":
                                case "statuscode":
                                    temp = FilterManager.ApplyStatusCodeFilter(set, parts[1]);
                                    break;
                                case "file":
                                    temp = FilterManager.ApplyFileFilter(set, parts[1]);
                                    break;
                                case "contenttype":
                                    temp = FilterManager.ApplyContentType(set, parts[1]);
                                    break;
                            }
                            if (!temp)
                            {
                                result = false;
                            }
                        }
                    }
                    continue;
                }
            }
            return result;
        }

        //- $ApplyVerbFilter -//
        private static Boolean ApplyVerbFilter(RequestResponseSet set, String verb)
        {
            Boolean result;
            switch (verb.ToLower())
            {
                case "get":
                case "post":
                    break;
                default:
                    throw new FormatException("Invalid verb");
            }
            result = set.Request.Verb.ToLower().Contains(verb);
            return result;
        }

        //- $ApplyStatusCodeFilter -//
        private static Boolean ApplyStatusCodeFilter(RequestResponseSet set, String statusCode)
        {
            Boolean result;
            Int32 statusCodeInput;
            if (Int32.TryParse(statusCode, out statusCodeInput))
            {
                result = set.Request.StatusCode == statusCodeInput;
            }
            else
            {
                return true;
            }
            return result;
        }

        //- $ApplyFileFilter -//
        private static Boolean ApplyFileFilter(RequestResponseSet set, String filename)
        {
            Boolean result;
            Path path = new Path(set.Request.Url);
            String filenamePotion = path.GetFileNamePortion();
            if (!String.IsNullOrEmpty(filenamePotion))
            {
                result = filenamePotion.ToLower().Contains(filename);
            }
            else
            {
                return true;
            }
            return result;
        }

        //- $ApplyContentType -//
        private static Boolean ApplyContentType(RequestResponseSet set, String contentType)
        {
            Boolean result;
            String responseContentType = set.Response.ContentType;
            if (!String.IsNullOrEmpty(responseContentType))
            {
                result = responseContentType.ToLower().Contains(contentType);
            }
            else
            {
                return true;
            }
            return result;
        }
    }
}
