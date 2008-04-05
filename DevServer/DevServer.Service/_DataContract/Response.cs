using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Text;
//+
namespace DevServer.Service
{
    [DataContract(Namespace = "http://www.netfxharmonics.com/Service/DevServer/2008/04/")]
    public class Response
    {
        //- @ContentLength -//
        [DataMember]
        public Int32 ContentLength { get; set; }

        //- @ControlState -//
        [DataMember]
        public String ControlState { get; set; }

        //- @ContentType -//
        [DataMember]
        public String ContentType { get; set; }

        //- @Data -//
        [DataMember]
        public String Data { get; set; }

        //- @HeaderList -//
        [DataMember]
        public List<Header> HeaderList { get; set; }

        //- @ViewState -//
        [DataMember]
        public String ViewState { get; set; }

        //- @ToString -//
        public string GetHeaderListText()
        {
            StringBuilder b = new StringBuilder();
            foreach (Header header in this.HeaderList)
            {
                b.AppendLine(String.Format("{0}: {1}", header.Name, header.Data));
            }
            return b.ToString();
        }
    }
}