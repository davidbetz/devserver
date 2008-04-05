using System.Collections.Generic;
using System.Runtime.Serialization;
//+
namespace DevServer
{
    [DataContract(Namespace = "http://www.netfxharmonics.com/Service/DevServer/2008/04/")]
    public class TracingConfiguration
    {
        //- @AllowedContentTypes -//
        [DataMember]
        public List<ContentType> AllowedContentTypes { get; set; }
    }
}