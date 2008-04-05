using System;
using System.Configuration;
//+
namespace DevServer.Configuration
{
    public class AllowedContentTypeElement : ConfigurationElement
    {
        //- @Value -//
        [ConfigurationProperty("value", IsRequired = true)]
        public String Value
        {
            get
            {
                return (String)this["value"];
            }
            set
            {
                this["value"] = value;
            }
        }
    }
}