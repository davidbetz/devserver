using System;

namespace DevServer.Service
{
    public interface IRequestSubmission
    {
        //- SubmitRequest -//
        String SubmitRequest(String instanceId, Request request, Response response);
    }
}