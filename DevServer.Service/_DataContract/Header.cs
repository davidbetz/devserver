using System;
using System.Runtime.Serialization;
//+
namespace DevServer.Service
{
    [DataContract(Namespace = DevServer.Service.Information.Namespace.DevServer)]
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