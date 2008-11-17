using System;
using System.Runtime.Serialization;
//+
namespace DevServer
{
    [DataContract(Namespace = DevServer.Service.Information.Namespace.DevServer)]
    public class ContentType
    {
        //- @Value -//
        [DataMember]
        public String Value { get; set; }
    }
}