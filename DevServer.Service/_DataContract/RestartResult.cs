using System;
using System.Runtime.Serialization;
//+
namespace DevServer
{
    [DataContract(Namespace = DevServer.Service.Information.Namespace.DevServer)]
    public class RestartResult
    {
        //- @RestartState -//
        public enum RestartState
        {
            NoStop = 2,
            StopNoStart = 1,
            Success = 0
        }

        //- @Result -//
        [DataMember]
        public RestartState Result { get; set; }

        //- @Message -//
        [DataMember]
        public String Message { get; set; }
    }
}