using System;
using System.Configuration;
//+
namespace DevServer.Configuration
{
    public class DefaultDocumentCollection : ConfigurationElementCollection
    {
        //- @[Indexer] -//
        public DocumentElement this[int index]
        {
            get
            {
                return (DocumentElement)base.BaseGet(index);
            }
            set
            {
                if (base.BaseGet(index) != null)
                    base.BaseRemoveAt(index);

                this.BaseAdd(index, value);
            }
        }

        //- #IsElementName -//
        protected override Boolean IsElementName(String elementName)
        {
            if ((String.IsNullOrEmpty(elementName)) || (elementName != "defaultDocuments"))
                return false;

            return true;
        }

        //- #CreateNewElement -//
        protected override ConfigurationElement CreateNewElement()
        {
            return new DocumentElement();
        }

        //- #GetElementKey -//
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((DocumentElement)element).Name;
        }
    }
}