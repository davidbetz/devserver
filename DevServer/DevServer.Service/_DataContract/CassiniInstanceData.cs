using System.Collections.Generic;
using System.Runtime.Serialization;
//+
namespace DevServer
{
    [DataContract(Namespace = "http://www.netfxharmonics.com/Service/DevServer/2008/04/")]
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