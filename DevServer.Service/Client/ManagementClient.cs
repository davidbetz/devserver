using System;
using System.Collections.Generic;
using System.ServiceModel;
//+
namespace DevServer.Service.Client
{
    public class ManagementClient : ClientBase<IManagementService>, IManagementService
    {
        static System.Xml.XmlDictionaryReaderQuotas q = new System.Xml.XmlDictionaryReaderQuotas
        {
            MaxStringContentLength = 1048576
        };
        //- @Ctor -//
        public ManagementClient()
            : base(new System.ServiceModel.NetNamedPipeBinding
            {
                ReceiveTimeout = TimeSpan.FromMinutes(5),
                ReaderQuotas = q
            }, new EndpointAddress("net.pipe://localhost/ManagementService"))
        {
        }

        //- @Ctor -//
        public ManagementClient(System.ServiceModel.Channels.Binding binding, EndpointAddress address)
            : base(binding, address)
        {
        }

        //- IManagementService -//
        #region IManagementService

        //- @GetInstance -//
        public Instance GetInstance(String instanceId)
        {
            return base.Channel.GetInstance(instanceId);
        }

        //- @GetInstances -//
        public List<Instance> GetInstances()
        {
            return base.Channel.GetInstances();
        }

        //- @KillAllInstances -//
        public void KillAllInstances()
        {
            base.Channel.KillAllInstances();
        }

        //- @KillInstance -//
        public void KillInstance(string instanceId)
        {
            base.Channel.KillInstance(instanceId);
        }

        //- @RestartInstance -//
        public Instance RestartInstance(String instanceId)
        {
            return base.Channel.RestartInstance(instanceId);
        }

        //- @StartupInstance -//
        public Instance StartupInstance(Instance instance)
        {
            return base.Channel.StartupInstance(instance);
        }

        //- @StartupExistingInstance -//
        public Instance StartupExistingInstance(String instanceId)
        {
            return base.Channel.StartupExistingInstance(instanceId);
        }

        //- @StopInstance -//
        public Instance StopInstance(String instanceId)
        {
            return base.Channel.StopInstance(instanceId);
        }

        //- @UpdateInstanceConfiguration -//
        public void UpdateInstanceConfiguration(HostConfiguration config)
        {
            base.Channel.UpdateInstanceConfiguration(config);
        }

        #endregion
    }
}