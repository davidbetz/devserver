using System;
using System.Collections.Generic;
using System.Configuration;
//+
namespace DevServer.Configuration
{
    public class ServerCollection : ConfigurationElementCollection, IEnumerable<ServerElement>
    {
        //- @[Indexer] -//
        public ServerElement this[int index]
        {
            get
            {
                return (ServerElement)base.BaseGet(index);
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
            return new ServerElement();
        }

        //- #GetElementKey -//
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ServerElement)element).Name;
        }

        #region IEnumerable<ServerConfigurationElement> Members

        //- @GetEnumerator -//
        public new IEnumerator<ServerElement> GetEnumerator()
        {
            for (int i = 0; i < base.Count; i++)
            {
                yield return (ServerElement)this[i];
            }
        }

        #endregion
    }
}