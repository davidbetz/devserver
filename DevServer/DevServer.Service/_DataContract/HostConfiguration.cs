using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
//+
namespace DevServer
{
    [Serializable]
    [DataContract(Namespace = "http://www.netfxharmonics.com/Service/DevServer/2008/04/")]
    public class HostConfiguration : INotifyPropertyChanged
    {
        private String instanceId;
        private List<String> allowedContentTypes;
        private Dictionary<String, String> contentTypeMappings;
        private List<String> defaultDocuments;
        private Boolean enableTracing;
        private Boolean enableVerboseTypeTracing;
        private Boolean enableFaviconTracing;

        //- @PropertyChanged -//
        public event PropertyChangedEventHandler PropertyChanged;

        //- @AllowedContentTypes -//
        [DataMember]
        public List<String> AllowedContentTypes
        {
            get
            {
                return allowedContentTypes;
            }
            set
            {
                allowedContentTypes = value;
                UpdateProperty("AllowedContentTypes");
            }
        }

        //- @ContentTypeMappings -//
        [DataMember]
        public Dictionary<String, String> ContentTypeMappings
        {
            get
            {
                return contentTypeMappings;
            }
            set
            {
                contentTypeMappings = value;
                UpdateProperty("ContentTypeMappings");
            }
        }

        //- @DefaultDocuments -//
        [DataMember]
        public List<String> DefaultDocuments
        {
            get
            {
                return defaultDocuments;
            }
            set
            {
                defaultDocuments = value;
                UpdateProperty("DefaultDocuments");
            }
        }

        //- @EnableFaviconTracing -//
        [DataMember]
        public Boolean EnableFaviconTracing
        {
            get
            {
                return enableFaviconTracing;
            }
            set
            {
                enableFaviconTracing = value;
                UpdateProperty("EnableFaviconTracing");
            }
        }

        //- @EnableTracing -//
        [DataMember]
        public Boolean EnableTracing
        {
            get
            {
                return enableTracing;
            }
            set
            {
                enableTracing = value;
                UpdateProperty("EnableTracing");
            }
        }

        //- @EnableVerboseTypeTracing -//
        [DataMember]
        public Boolean EnableVerboseTypeTracing
        {
            get
            {
                return enableVerboseTypeTracing;
            }
            set
            {
                enableVerboseTypeTracing = value;
                UpdateProperty("EnableVerboseTypeTracing");
            }
        }

        //- @InstanceId -//
        [DataMember]
        public String InstanceId
        {
            get
            {
                return instanceId;
            }
            set
            {
                instanceId = value;
                UpdateProperty("InstanceId");
            }
        }

        //- $UpdateProperty -//
        private void UpdateProperty(string p)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(p));
            }
        }
    }
}