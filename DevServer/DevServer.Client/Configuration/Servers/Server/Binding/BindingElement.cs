using System;
using System.Configuration;
//+
namespace DevServer.Configuration
{
    public class BindingElement : ConfigurationElement
    {
        //- @Address -//
        [ConfigurationProperty("address", DefaultValue = "loopback")]
        public String Address
        {
            get
            {
                return (String)this["address"];
            }
            set
            {
                this["address"] = value;
            }
        }
    }
}