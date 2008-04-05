using System;
using System.Collections.Generic;
using System.Configuration;
//+
namespace DevServer.Configuration
{
    public class ProfileCollection : ConfigurationElementCollection, IEnumerable<ProfileServerElement>
    {
        //- @[Indexer] -//
        public ProfileServerElement this[int index]
        {
            get
            {
                return base.BaseGet(index) as ProfileServerElement;
            }
        }

        //- @Name -//
        [ConfigurationProperty("name", IsRequired = true)]
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

        //- #CreateNewElement -//
        protected override ConfigurationElement CreateNewElement()
        {
            return new ProfileServerElement();
        }

        //- #GetElementKey -//
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ProfileServerElement)element).Key;
        }

        //- #ElementName -//
        protected override string ElementName
        {
            get
            {
                return "server";
            }
        }

        //- #IsElementName -//
        protected override Boolean IsElementName(String elementName)
        {
            return !String.IsNullOrEmpty(elementName) && elementName == "server";
        }

        //- @CollectionType -//
        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }

        #region IEnumerable<ProfileServerElement> Members

        //- @GetEnumerator -//
        public new IEnumerator<ProfileServerElement> GetEnumerator()
        {
            for (int i = 0; i < base.Count; i++)
            {
                yield return (ProfileServerElement)this[i];
            }
        }

        #endregion
    }
}