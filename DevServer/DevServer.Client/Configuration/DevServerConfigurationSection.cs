using System.Configuration;
using System;
using System.Collections.Generic;
//+
namespace DevServer.Configuration
{
    public class DevServerConfigurationSection : ConfigurationSection
    {
        //- @RequestTracing -//
        [ConfigurationProperty("requestTracing")]
        public RequestTracingElement RequestTracing
        {
            get
            {
                return (RequestTracingElement)this["requestTracing"];
            }
            set
            {
                this["requestTracing"] = value;
            }
        }

        //- @WebServer -//
        [ConfigurationProperty("webServer")]
        public WebServerElement WebServer
        {
            get
            {
                return (WebServerElement)this["webServer"];
            }
            set
            {
                this["webServer"] = value;
            }
        }

        //- @Servers -//
        [ConfigurationProperty("servers")]
        [ConfigurationCollection(typeof(ServerElement), AddItemName="server")]
        public ServerCollection Servers
        {
            get
            {
                return (ServerCollection)this["servers"];
            }
            set
            {
                this["servers"] = value;
            }
        }

        //- @StartupProfiles -//
        [ConfigurationProperty("startupProfiles")]
        [ConfigurationCollection(typeof(ProfileCollection), AddItemName = "profile")]
        public StartupProfileCollection StartupProfiles
        {
            get
            {
                return (StartupProfileCollection)this["startupProfiles"];
            }
            set
            {
                this["startupProfiles"] = value;
            }
        }
    }
}