using System;
using System.Runtime.Serialization;
//+
namespace DevServer.Service
{
    [DataContract(Namespace = "http://www.netfxharmonics.com/Service/DevServer/2008/04/")]
    public class Header
    {
        //- @Name -//
        [DataMember]
        public String Name { get; set; }

        //- @Data -//
        [DataMember]
        public String Data { get; set; }
    }
}