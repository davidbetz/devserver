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
                String address = (String)this["address"];
                if (address.ToLower() == "localhost")
                {
                    address = "loopback";
                }
                //+
                return address;
            }
            set
            {
                this["address"] = value;
            }
        }
    }
}