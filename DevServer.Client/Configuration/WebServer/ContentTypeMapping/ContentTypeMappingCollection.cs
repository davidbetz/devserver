using System;
using System.Configuration;
//+
namespace DevServer.Configuration
{
    public class ContentTypeMappingCollection : ConfigurationElementCollection
    {
        //- @[Indexer] -//
        public ContentTypeMappingElement this[Int32 index]
        {
            get
            {
                return (ContentTypeMappingElement)base.BaseGet(index);
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
            if ((String.IsNullOrEmpty(elementName)) || (elementName != "contentTypeMappings"))
                return false;

            return true;
        }

        //- @CollectionType -//
        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.AddRemoveClearMap; }
        }

        //- #CreateNewElement -//
        protected override ConfigurationElement CreateNewElement()
        {
            return new ContentTypeMappingElement();
        }

        //- #GetElementKey -//
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ContentTypeMappingElement)element).Extension;
        }
    }
}