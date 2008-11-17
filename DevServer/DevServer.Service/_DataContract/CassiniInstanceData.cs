using System.Collections.Generic;
using System.Runtime.Serialization;
//+
namespace DevServer
{
    [DataContract(Namespace = DevServer.Service.Information.Namespace.DevServer)]
    public class DevServerInstanceData
    {
        //- @Instances -//
        [DataMember]
        public List<Instance> Instances { get; set; }

        //- @TracingConfiguration -//
        [DataMember]
        public TracingConfiguration TracingConfiguration { get; set; }
    }
}