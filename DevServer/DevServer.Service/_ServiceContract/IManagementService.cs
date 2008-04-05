using System;
using System.Collections.Generic;
using System.ServiceModel;
//+
namespace DevServer.Service
{
    [ServiceContract(Namespace="http://www.netfxharmonics.com/Service/DevServer/2008/04/")]
    public interface IManagementService
    {
        //- StartupInstance -//
        [OperationContract]
        Instance StartupInstance(Instance instance);

        //- StartupExistingInstance -//
        [OperationContract]
        Instance StartupExistingInstance(String instanceId);

        //- StopInstance -//
        [OperationContract]
        Instance StopInstance(String instanceId);

        //- KillInstance -//
        [OperationContract]
        void KillInstance(String instanceId);

        //- RestartInstance -//
        [OperationContract]
        Instance RestartInstance(String instanceId);

        //- GetInstances -//
        [OperationContract]
        List<Instance> GetInstances();

        //- GetInstance -//
        [OperationContract]
        Instance GetInstance(String instanceId);

        //- KillAllInstances -//
        [OperationContract]
        void KillAllInstances();

        //- UpdateInstanceConfiguration -//
        [OperationContract]
        void UpdateInstanceConfiguration(HostConfiguration config);
    }
}