using System;
using System.Collections.Generic;
//+
namespace DevServer.Client
{
    class Path
    {
        //- $Uri -//
        private Uri Uri { get; set; }

        //- $Portions -//
        private List<String> Portions { get; set; }

        //- @Path -//
        public String Url
        {
            get
            {
                if (this.Uri != null)
                {
                    return this.Uri.AbsoluteUri;
                }
                return String.Empty;
            }
        }

        //- @Ctor -//
        public Path()
        {
        }

        //- @GetFileNamePortion -//
        public String GetFileNamePortion( )
        {
            if (String.IsNullOrEmpty(this.Url))
            {
                return null;
            }
            if (!this.Url.Contains("/"))
            {
                return String.Empty;
            }
            String[] parts = this.Url.Split('/');
            return parts[parts.Length - 1];
        }
    }
}
