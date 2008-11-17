using System;
using System.Collections.Generic;
using System.Linq;
//+
namespace DevServer.WebCore
{
    internal sealed class ContentType
    {
        //- ~GetContentype --/
        internal static String GetContentype(String extension, Dictionary<String, String> contentTypeMappings)
        {
            String contentType = contentTypeMappings.FirstOrDefault(p => p.Key == extension).Value;
            if (String.IsNullOrEmpty(contentType))
            {
                return null;
            }
            return contentType;
        }
    }
}