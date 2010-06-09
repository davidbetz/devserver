using System;
using System.Collections.Generic;
using System.Configuration;
//+
namespace DevServer.Configuration
{
    public class AllowedContentTypeCollection : ConfigurationElementCollection, IEnumerable<AllowedContentTypeElement>
    {
        //- @[Indexer] -//
        public AllowedContentTypeElement this[int index]
        {
            get
            {
                return (AllowedContentTypeElement)base.BaseGet(index);
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
            if ((String.IsNullOrEmpty(elementName)) || (elementName != "allowedContentType"))
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
            return new AllowedContentTypeElement();
        }

        //- #GetElementKey -//
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((AllowedContentTypeElement)element).Value;
        }

        #region IEnumerable<AllowedContentTypeElement> Members

        //- @GetEnumerator -//
        public new IEnumerator<AllowedContentTypeElement> GetEnumerator()
        {
            for (int i = 0; i < base.Count; i++)
            {
                yield return (AllowedContentTypeElement)this[i];
            }
        }

        #endregion
    }
}