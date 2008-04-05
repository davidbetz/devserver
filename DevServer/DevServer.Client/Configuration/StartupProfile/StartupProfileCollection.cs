using System;
using System.Collections.Generic;
using System.Configuration;
//+
namespace DevServer.Configuration
{
    public class StartupProfileCollection : ConfigurationElementCollection, IEnumerable<ProfileCollection>
    {
        //- @ActiveProfile -//
        [ConfigurationProperty("activeProfile", IsRequired = false)]
        public String ActiveProfile
        {
            get
            {
                return (String)this["activeProfile"];
            }
            set
            {
                this["activeProfile"] = value;
            }
        }

        //- @[Indexer] -//
        public ProfileCollection this[int index]
        {
            get
            {
                return (ProfileCollection)base.BaseGet(index);
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
            return new ProfileCollection();
        }

        //- #GetElementKey -//
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ProfileCollection)element).Name;
        }

        #region IEnumerable<ServerConfigurationElement> Members

        //- @GetEnumerator -//
        public new IEnumerator<ProfileCollection> GetEnumerator()
        {
            for (int i = 0; i < base.Count; i++)
            {
                yield return (ProfileCollection)this[i];
            }
        }

        #endregion
    }
}