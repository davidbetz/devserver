using System;
using System.Configuration;
//+
namespace DevServer.Configuration
{
    public class ServerRequestTracingElement : ConfigurationElement
    {
        //- @Enable -//
        [ConfigurationProperty("enable", DefaultValue = false)]
        public Boolean Enable
        {
            get
            {
                return (Boolean)this["enable"];
            }
            set
            {
                this["enable"] = value;
            }
        }

        //- @EnableFaviconTracing -//
        [ConfigurationProperty("enableFaviconTracing", DefaultValue = false)]
        public Boolean EnableFaviconTracing
        {
            get
            {
                return (Boolean)this["enableFaviconTracing"];
            }
            set
            {
                this["enableFaviconTracing"] = value;
            }
        }

        //- @EnableVerboseTypeTracing -//
        [ConfigurationProperty("enableVerboseTypeTracing", DefaultValue = false)]
        public Boolean EnableVerboseTypeTracing
        {
            get
            {
                return (Boolean)this["enableVerboseTypeTracing"];
            }
            set
            {
                this["enableVerboseTypeTracing"] = value;
            }
        }
    }
}