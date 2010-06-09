using System;
using System.ServiceModel;
//+
namespace DevServer.Service.Client
{
    public class RequestManagementClient : ClientBase<IRequestManagementService>, IRequestManagementService
    {
        //- @Ctor -//
        public RequestManagementClient()
            : base(new NetNamedPipeBinding(), new EndpointAddress("net.pipe://localhost/RequestManagementService"))
        {
        }

        //- @Ctor -//
        public RequestManagementClient(String endpointConfigurationName)
            : base(endpointConfigurationName)
        {
        }

        #region IRequestManagement

        //- @SubmitRequest -//
        public String SubmitRequest(String instanceId, Request request, Response response)
        {
            return base.Channel.SubmitRequest(instanceId, request, response);
        }

        #endregion IRequestManagement
    }
}