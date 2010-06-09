using System.Collections.Generic;
using System.Runtime.Serialization;
//+
namespace DevServer
{
    [DataContract(Namespace = DevServer.Service.Information.Namespace.DevServer)]
    public class TracingConfiguration
    {
        //- @AllowedContentTypes -//
        [DataMember]
        public List<ContentType> AllowedContentTypes { get; set; }
    }
}