using System;
using System.Configuration;
//+
namespace DevServer.Configuration
{
    public class ServerRequestTracingElement : ConfigurationElement
    {
        //- @Enable -//
        [ConfigurationProperty("enabled", DefaultValue = false)]
        public Boolean Enable
        {
            get
            {
                return (Boolean)this["enabled"];
            }
            set
            {
                this["enabled"] = value;
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