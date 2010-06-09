using System;
using System.Configuration;
//+
namespace DevServer.Configuration
{
    public class ContentTypeMappingElement : ConfigurationElement
    {
        //- @Extension -//
        [ConfigurationProperty("extension", IsRequired = true)]
        public String Extension
        {
            get
            {
                return (String)this["extension"];
            }
            set
            {
                this["extension"] = value;
            }
        }

        //- @Type -//
        [ConfigurationProperty("type", IsRequired = true)]
        public String Type
        {
            get
            {
                return (String)this["type"];
            }
            set
            {
                this["type"] = value;
            }
        }

        //- @Override -//
        [ConfigurationProperty("override")]
        public Boolean Override
        {
            get
            {
                return (Boolean)this["override"];
            }
            set
            {
                this["override"] = value;
            }
        }
    }
}