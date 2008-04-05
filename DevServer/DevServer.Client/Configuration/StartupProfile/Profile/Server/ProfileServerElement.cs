using System;
using System.Configuration;
//+
namespace DevServer.Configuration
{
    public class ProfileServerElement : ConfigurationElement
    {
        //- @Key -//
        [ConfigurationProperty("key", IsRequired = true)]
        public String Key
        {
            get
            {
                return (String)this["key"];
            }
            set
            {
                this["key"] = value;
            }
        }
    }
}