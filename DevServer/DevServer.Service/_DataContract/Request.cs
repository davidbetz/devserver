using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
//+
namespace DevServer.Service
{
    [DataContract(Namespace = Information.Namespace.DevServer)]
    public class Request
    {
        //- @ContentLength -//
        [DataMember]
        public Int32 ContentLength { get; set; }

        //- @ContentType -//
        [DataMember]
        public String ContentType { get; set; }

        //- @ControlState -//
        [DataMember]
        public String ControlState { get; set; }

        //- @Data -//
        [DataMember]
        public String Data { get; set; }

        //- @DateTime -//
        [DataMember]
        public DateTime DateTime { get; set; }

        //- @IPAddress -//
        [DataMember]
        public String IPAddress { get; set; }

        //- @HeaderList -//
        [DataMember]
        public List<Header> HeaderList { get; set; }

        //- @StatusCode -//
        [DataMember]
        public Int32 StatusCode { get; set; }

        //- @Url -//
        [DataMember]
        public String Url { get; set; }

        //- @Verb -//
        [DataMember]
        public String Verb { get; set; }

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