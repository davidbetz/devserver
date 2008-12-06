using System;
using System.ServiceModel;
//+
namespace DevServer.Service
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Single, InstanceContextMode = InstanceContextMode.Single, UseSynchronizationContext=true)]
    public class RequestManagementService : IRequestManagementService
    {
        //- $RequestSubmission -//
        private IRequestSubmission RequestSubmission { get; set; }

        //- @Ctor -//
        public RequestManagementService(IRequestSubmission requestSubmission)
        {
            this.RequestSubmission = requestSubmission;
        }

        #region IRequestManagement

        //- @SubmitRequest -//
        public string SubmitRequest(String instanceId, Request request, Response response)
        {
            return this.RequestSubmission.SubmitRequest(instanceId, request, response);
        }

        #endregion
    }
}