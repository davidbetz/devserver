using System;
using System.Runtime.Serialization;
//+
namespace DevServer
{
    [DataContract(Namespace = "http://www.netfxharmonics.com/Service/DevServer/2008/04/")]
    public class ContentType
    {
        //- @Value -//
        [DataMember]
        public String Value { get; set; }
    }
}