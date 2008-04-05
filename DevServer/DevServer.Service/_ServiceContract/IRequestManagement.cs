using System;
using System.ServiceModel;
//+
namespace DevServer.Service
{
    [ServiceContract(Namespace = "http://www.netfxharmonics.com/Service/DevServer/2008/04/")]
    public interface IRequestManagement
    {
        //- SubmitRequest -//
        [OperationContract]
        String SubmitRequest(String instanceId, Request request, Response response);
    }
}