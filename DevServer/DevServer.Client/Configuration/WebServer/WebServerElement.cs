using System.Configuration;
//+
namespace DevServer.Configuration
{
    public class WebServerElement : ConfigurationElement
    {
        //- @DefaultDocuments -//
        [ConfigurationProperty("defaultDocuments")]
        [ConfigurationCollection(typeof(DocumentElement), AddItemName = "add")]
        public DefaultDocumentCollection DefaultDocuments
        {
            get
            {
                return (DefaultDocumentCollection)this["defaultDocuments"];
            }
            set
            {
                this["defaultDocuments"] = value;
            }
        }

        //- @ContentTypeMappings -//
        [ConfigurationProperty("contentTypeMappings")]
        [ConfigurationCollection(typeof(ContentTypeMappingElement), AddItemName = "add")]
        public ContentTypeMappingCollection ContentTypeMappings
        {
            get
            {
                return (ContentTypeMappingCollection)this["contentTypeMappings"];
            }
            set
            {
                this["contentTypeMappings"] = value;
            }
        }
    }
}