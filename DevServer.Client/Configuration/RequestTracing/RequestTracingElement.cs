using System.Configuration;
//+
namespace DevServer.Configuration
{
    public class RequestTracingElement : ConfigurationElement
    {
        //- @AllowedContentTypes -//
        [ConfigurationProperty("allowedContentTypes")]
        [ConfigurationCollection(typeof(AllowedContentTypeElement), AddItemName="add")]
        public AllowedContentTypeCollection AllowedContentTypes
        {
            get
            {
                return (AllowedContentTypeCollection)this["allowedContentTypes"];
            }
            set
            {
                this["allowedContentTypes"] = value;
            }
        }
    }
}