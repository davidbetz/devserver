using System;
using System.ServiceModel;
//+
namespace DevServer.Service
{
    [ServiceContract(Namespace = Information.Namespace.DevServer)]
    public interface IRequestManagement
    {
        //- SubmitRequest -//
        [OperationContract]
        String SubmitRequest(String instanceId, Request request, Response response);
    }
}