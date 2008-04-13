using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using DevServer.WebCore;
using RequestResponseDictionary = System.Collections.Generic.Dictionary<DevServer.Service.Request, DevServer.Service.Response>;

namespace DevServer.Service
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ManagementService : IManagementService
    {
        //- @Instances -//
        public List<Instance> Instances
        {
            get
            {
                return (from p in this.Servers
                        select p.Instance).ToList();
            }
        }

        //- @Servers -//
        public List<Server> Servers { get; set; }

        //- @RequestResponseSet -//
        public Dictionary<String, RequestResponseDictionary> RequestResponseSet { get; set; }

        //- @Messages -//
        public List<String> Messages { get; set; }

        //- @Ctor -//
        public ManagementService()
        {
            Servers = new List<Server>();
            RequestResponseSet = new Dictionary<String, RequestResponseDictionary>();
            Messages = new List<String>();
        }

        //- @GetInstances -//
        public List<Instance> GetInstances()
        {
            return this.Instances;
        }

        //- @GetInstance -//
        public Instance GetInstance(string instanceId)
        {
            Func<Server, Boolean> specificInstanceId = x => x.Instance.Id == instanceId;
            if (this.Servers.Count(specificInstanceId) > 0)
            {
                Instance instance = this.Servers.Single(specificInstanceId).Instance;
                instance.OperationStatus = Instance.InstanceStatus.OperationSuccess;
                return instance;
            }
            else
            {
                return null;
            }
        }

        //- @KillAllInstances -//
        public void KillAllInstances()
        {
            List<Instance> instances = this.Servers.Select(p => p.Instance).ToList();
            foreach (var instance in instances)
            {
                try
                {
                    _StopAndKillInstance(instance);
                }
                catch
                {
                    //+Don't really care...
                }
            }
        }

        //- @KillInstance -//
        public void KillInstance(String instanceId)
        {
            Func<Server, Boolean> specificInstanceId = x => x.Instance.Id == instanceId;
            if (this.Servers.Count(specificInstanceId) > 0)
            {
                Instance instance = this.Servers.Single(specificInstanceId).Instance;
                try
                {
                    _StopAndKillInstance(instance);
                }
                catch
                {
                    //+Don't really care...
                }
            }
        }

        //- @StartupInstance -//
        public Instance StartupInstance(Instance instance)
        {
            try
            {
                if (String.IsNullOrEmpty(instance.Id))
                {
                    instance.Id = Guid.NewGuid().ToString();
                }
                //+
                Server server = null;
                //+ Does the instance already exist?
                if (this.Servers.Count(p => p.Instance.Id == instance.Id) > 0)
                {
                    server = this.Servers.Single(p => p.Instance.Id == instance.Id);
                    //+ Is it already started
                    if (server.Instance.State == Instance.InstanceState.Started)
                    {
                        instance.OperationStatus = Instance.InstanceStatus.AlreadyStarted;
                        return instance;
                    }
                    else
                    {
                        StartupExistingInstance(instance.Id);
                        instance.OperationStatus = Instance.InstanceStatus.AlreadyExists;
                        return instance;
                    }
                }
                else
                {
                    server = new Server(instance.Port, instance.VirtualPath, instance.PhysicalPath);
                    server.Instance = instance;
                }
                server.Start();
                instance.State = Instance.InstanceState.Started;
                instance.OperationStatus = Instance.InstanceStatus.OperationSuccess;
                this.Servers.Add(server);
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                instance.LastActionMessage = ex.Message;
                if (ex.Message == "Only one usage of each socket address (protocol/network address/port) is normally permitted")
                {
                    instance.OperationStatus = Instance.InstanceStatus.PortNotAvailable;
                }
            }
            catch (Exception ex)
            {
                instance.LastActionMessage = ex.Message;
                instance.State = Instance.InstanceState.Stopped;
                instance.OperationStatus = Instance.InstanceStatus.ExceptionThrown;
            }
            return instance;
        }

        //- @StartupExistingInstance -//
        public Instance StartupExistingInstance(String instanceId)
        {
            Server newServer = null;
            try
            {
                Func<Server, Boolean> specificInstanceId = x => x.Instance.Id == instanceId;
                if (this.Servers.Count(specificInstanceId) > 0)
                {
                    Server server = this.Servers.Single(specificInstanceId);
                    newServer = new Server(server.Port, server.VirtualPath, server.PhysicalPath)
                    {
                        Instance = server.Instance
                    };
                    //+
                    server.Dispose();
                    this.Servers.Remove(server);
                    //+
                    newServer.Start();
                    newServer.Instance.State = Instance.InstanceState.Started;
                    this.Servers.Add(newServer);
                    newServer.Instance.OperationStatus = Instance.InstanceStatus.OperationSuccess;
                    //+
                    return newServer.Instance;
                }

                return null;
            }
            catch (Exception ex)
            {
                if (newServer != null && newServer.Instance != null)
                {
                    newServer.Instance.LastActionMessage = ex.Message;
                    newServer.Instance.OperationStatus = Instance.InstanceStatus.ExceptionThrown;
                    return newServer.Instance;
                }
                else
                {
                    return null;
                }
            }
        }

        //- $_StopAndKillInstance -//
        private void _StopAndKillInstance(Instance instance)
        {
            Func<Server, Boolean> specificInstanceId = x => x.Instance.Id == instance.Id;
            Server server = this.Servers.Single(specificInstanceId);
            //+
            server.Stop();
            server.Dispose();
            //+
            this.Servers.Remove(server);
        }

        //- @StopInstance -//
        public Instance StopInstance(String instanceId)
        {
            Func<Server, Boolean> specificInstanceId = x => x.Instance.Id == instanceId;
            if (this.Servers.Count(specificInstanceId) > 0)
            {
                Instance instance = this.Servers.Single(specificInstanceId).Instance;
                try
                {
                    instance = _StopInstance(instance);
                    instance.OperationStatus = Instance.InstanceStatus.OperationSuccess;
                    return instance;
                }
                catch (Exception ex)
                {
                    instance.OperationStatus = Instance.InstanceStatus.ExceptionThrown;
                    instance.LastActionMessage = ex.Message;
                    return instance;
                }
            }
            return null;
        }

        //- $_StopInstance -//
        private Instance _StopInstance(Instance instance)
        {
            Func<Server, Boolean> specificInstanceId = x => x.Instance.Id == instance.Id;
            Server server = this.Servers.Single(specificInstanceId);
            //+
            server.Stop();
            //+
            server.Instance.State = Instance.InstanceState.Stopped;
            server.Instance.OperationStatus = Instance.InstanceStatus.OperationSuccess;
            return server.Instance;
        }

        //- @RestartInstance -//
        public Instance RestartInstance(String instanceId)
        {
            RestartResult result = new RestartResult();
            Func<Server, Boolean> specificInstanceId = x => x.Instance.Id == instanceId;
            Instance instance = this.Servers.Single(specificInstanceId).Instance;
            try
            {
                instance = _StopInstance(instance);
            }
            catch (Exception ex)
            {
                instance.State = Instance.InstanceState.Stopped;
                instance.LastActionMessage = ex.Message;
                instance.OperationStatus = Instance.InstanceStatus.CouldNotStop;
                return instance;
            }

            try
            {
                instance = StartupInstance(instance);
                instance.OperationStatus = Instance.InstanceStatus.OperationSuccess;
            }
            catch (Exception ex)
            {

                instance.State = Instance.InstanceState.Stopped;
                instance.LastActionMessage = ex.Message;
                instance.OperationStatus = Instance.InstanceStatus.StoppedButCouldNotStart;
                return instance;
            }

            return instance;
        }

        //- @UpdateInstanceConfiguration -//
        public void UpdateInstanceConfiguration(HostConfiguration config)
        {
            Server server = this.Servers.Single(p => p.Instance.Id == config.InstanceId);
            server.UpdateInstanceConfiguration(config);
        }
    }
}