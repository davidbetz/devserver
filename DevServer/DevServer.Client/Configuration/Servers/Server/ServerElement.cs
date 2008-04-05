using System;
using System.Configuration;
//+
namespace DevServer.Configuration
{
    public class ServerElement : ConfigurationElement
    {
        //- @Name -//
        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        public String Name
        {
            get
            {
                return (String)this["name"];
            }
            set
            {
                this["name"] = value;
            }
        }

        //- @Key -//
        [ConfigurationProperty("key", IsRequired = false)]
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

        //- @Disabled -//
        [ConfigurationProperty("disabled", DefaultValue = false)]
        public Boolean Disabled
        {
            get
            {
                return (Boolean)this["disabled"];
            }
            set
            {
                this["disabled"] = value;
            }
        }

        //- @Port -//
        [ConfigurationProperty("port", IsRequired = true)]
        public Int32 Port
        {
            get
            {
                return (Int32)this["port"];
            }
            set
            {
                this["port"] = value;
            }
        }

        //- @VirtualPath -//
        [ConfigurationProperty("virtualPath", IsRequired = true)]
        public String VirtualPath
        {
            get
            {
                return (String)this["virtualPath"];
            }
            set
            {
                this["virtualPath"] = value;
            }
        }

        //- @PhysicalPath -//
        [ConfigurationProperty("physicalPath", IsRequired = true)]
        public String PhysicalPath
        {
            get
            {
                return (String)this["physicalPath"];
            }
            set
            {
                this["physicalPath"] = value;
            }
        }

        //- @PhysicalPath -//
        [ConfigurationProperty("binding")]
        public BindingElement Binding
        {
            get
            {
                return (BindingElement)this["binding"];
            }
            set
            {
                this["binding"] = value;
            }
        }

        //- @RequestTracing -//
        [ConfigurationProperty("requestTracing")]
        public ServerRequestTracingElement RequestTracing
        {
            get
            {
                return (ServerRequestTracingElement)this["requestTracing"];
            }
            set
            {
                this["requestTracing"] = value;
            }
        }
    }
}