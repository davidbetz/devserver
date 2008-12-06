using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using DevServer.Service;
using DevServer.Service.Client;
//+
namespace DevServer.Client
{
    public partial class MainWindow : Window, IRequestSubmission
    {
        //- $Message -//
        class Message
        {
            //- @DateTime -//
            public DateTime DateTime { get; set; }

            //- @PrettyDateTime -//
            public String PrettyDateTime
            {
                get
                {
                    return this.DateTime.ToString("T");
                }
            }
            //- @Summary -//
            public String Summary { get; set; }

            //- @Detail -//
            public String Detail { get; set; }
        }

        //+
        private ServiceHost managementHost = null;
        private ServiceHost requestHost = null;

        //+
        //- $MessageLog -//
        private ObservableCollection<Message> MessageLog { get; set; }

        //- ~Instances -//
        internal List<Instance> Instances { get; set; }

        //- @RequestResponseSet -//
        public Dictionary<String, ObservableCollection<RequestResponseSet>> RequestResponseSet { get; set; }

        //+
        //- @Ctor -//
        public MainWindow(List<Instance> instances)
        {
            this.Instances = instances;
            this.RequestResponseSet = new Dictionary<String, ObservableCollection<RequestResponseSet>>();
            this.MessageLog = new ObservableCollection<Message>();
            //+
            InitializeComponent();
        }

        //+
        //- #OnInitialized -//
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            //+
            StartServices();
            StartInstances();
            //+
            RefreshInstanceTabs();
            //+
            this.DataContext = this;
            this.txtMessageLog.ItemsSource = MessageLog;
            //+
            TabControl tcMain = (TabControl)FindName("tcMain");
            //+ bind data, but do not add management
            tcMain.DataContext = this.Instances.Where(p => p.Id != DevServer.Client.Program.HostInformation.ManagementGuid);
            //+
            Button btnCreateNewInstance = (Button)tcMain.FindName("btnCreateNewInstance");
            btnCreateNewInstance.Click += delegate
            {
                CreateNewInstance();
            };
            //+
            Button btnRefreshInstanceList = (Button)tcMain.FindName("btnRefreshInstanceList");
            btnRefreshInstanceList.Click += delegate
            {
                RefreshInstanceTabs();
            };
            //+
            btnAbout.Click += delegate
            {
                AboutWindow aboutWindow = new AboutWindow();
                aboutWindow.ShowDialog();
            };
        }

        //- #OnClosing -//
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (managementHost != null)
            {
                KillInstances();
                if (managementHost.State == CommunicationState.Closed)
                {
                    managementHost.Close();
                }
            }
            if (requestHost.State == CommunicationState.Closed)
            {
                requestHost.Close();
            }
        }

        //- $AddTab -//
        private void AddTab(Instance instance)
        {
            TabControl tcMain = (TabControl)FindName("tcMain");
            AddTab(instance, tcMain);
        }

        //- $AddTab -//
        private void AddTab(Instance instance, TabControl tcMain)
        {
            InstanceTab tab = new InstanceTab(instance);
            //+
            tab.Style = (Style)FindResource("tabStyle");
            tcMain.Items.Add(tab);
        }

        //- $CreateNewInstance -//
        private void CreateNewInstance()
        {
            using (ManagementClient client = new ManagementClient( ))
            {
                if (Validate(txtPhysicalPath.Text, "ID is required") &&
                Validate(txtPort.Text, "Port is required") &&
                Validate(txtVirtualPath.Text, "Virtual Path is required"))
                {
                    Instance instance = new Instance()
                    {
                        Name = txtInstanceName.Text,
                        PhysicalPath = txtPhysicalPath.Text,
                        Port = Int32.Parse(txtPort.Text),
                        VirtualPath = txtVirtualPath.Text,
                        HostConfiguration = new HostConfiguration
                        {
                            //+ use defaults
                        }
                    };
                    instance = client.StartupInstance(instance);
                    if (instance != null)
                    {
                        switch (instance.OperationStatus)
                        {
                            case Instance.InstanceStatus.AlreadyExists:
                                ReportActionMessage(String.Format("Instance {0} ({1}) already exists, but was started.", instance.Name, instance.Id), String.Empty);
                                SetTabControlStartStopButtonText(instance.Id, "Stop Instance");
                                break;
                            case Instance.InstanceStatus.AlreadyStarted:
                                ReportActionMessage(String.Format("Instance {0} ({1}) is already started.", instance.Name, instance.Id), String.Empty);
                                break;
                            case Instance.InstanceStatus.OperationSuccess:
                                AddTab(instance);
                                ReportActionMessage(String.Format("Instance {0} ({1}) successfully started.", instance.Name, instance.Id), String.Empty);
                                break;
                            case Instance.InstanceStatus.PortNotAvailable:
                                ReportActionMessage(String.Format("Unable to start instance {0} ({1}) - port already bound.", instance.Name, instance.Id), String.Empty);
                                break;
                            case Instance.InstanceStatus.ExceptionThrown:
                                ReportActionMessage(String.Format("Last instance {0} ({1}) action threw an exception (see log for details).", instance.Name, instance.Id), instance.LastActionMessage);
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        ReportActionMessage(String.Format("Unable to start instance {0} ({1})", instance.Name, instance.Id), String.Empty);
                    }
                }
            }
        }

        //- $KillInstances -//
        private void KillInstances()
        {
            using (ManagementClient client = new ManagementClient( ))
            {
                client.KillAllInstances();
            }
        }

        //- $RefreshInstanceTabs -//
        private void RefreshInstanceTabs()
        {
            using (ManagementClient client = new ManagementClient( ))
            {
                this.Instances = client.GetInstances();
            }
            //+
            TabControl tcMain = (TabControl)FindName("tcMain");
            while (tcMain.Items.Count > 1)
            {
                tcMain.Items.RemoveAt(1);
            }
            //+
            foreach (var instance in this.Instances)
            {
                //+ Do not add management
                if (instance.Id != DevServer.Client.Program.HostInformation.ManagementGuid)
                {
                    AddTab(instance);
                }
            }
        }

        //- ~ReportActionMessage -//
        internal void ReportActionMessage(String summary, String detail)
        {
            TextBlock tbStatusMessage = (TextBlock)FindName("tbStatusMessage");
            tbStatusMessage.Text = summary;
            MessageLog.Insert(0, new Message
            {
                DateTime = DateTime.Now,
                Summary = summary,
                Detail = detail
            });
        }

        //- $SetTabControlStartStopButtonText -//
        private void SetTabControlStartStopButtonText(String instanceId, String text)
        {
            TabControl tcMain = (TabControl)FindName("tcMain");
            foreach (var item in tcMain.Items)
            {
                TabItem tiItem = (TabItem)item;
                TextBlock txtTabInstanceId = tiItem.FindName("txtInstanceId") as TextBlock;
                if (txtTabInstanceId != null)
                {
                    if (txtTabInstanceId.Text == instanceId)
                    {
                        Button btnStopInstance = (Button)tiItem.FindName("btnStopInstance");
                        btnStopInstance.Content = text;
                    }
                }
            }
        }

        //- $StartInstances -//
        private void StartInstances()
        {
            using (ManagementClient client = new ManagementClient( ))
            {
                for (int i = 0; i < this.Instances.Count; i++)
                {
                    this.Instances[i] = client.StartupInstance(this.Instances[i]);
                    if (this.Instances[i].OperationStatus != Instance.InstanceStatus.OperationSuccess)
                    {
                        ReportActionMessage(String.Format("Instance {0} ({1}) had a problem starting up (see message log for details).", this.Instances[i].Name, this.Instances[i].Id), this.Instances[i].LastActionMessage);
                    }
                }
            }
        }

        //- $StartServices -//
        private void StartServices()
        {
            ManagementService service = new ManagementService();
            managementHost = new ServiceHost(service, new Uri("net.pipe://localhost/ManagementService"));
            System.ServiceModel.NetNamedPipeBinding binding = new System.ServiceModel.NetNamedPipeBinding
            {
                ReceiveTimeout = TimeSpan.FromMinutes(5)
            };
            binding.ReaderQuotas.MaxStringContentLength = 1048576;
            managementHost.AddServiceEndpoint(typeof(IManagementService), binding, String.Empty);
            managementHost.Open();
            //+
            RequestManagementService requestService = new RequestManagementService(this);
            requestHost = new ServiceHost(requestService, new Uri("net.pipe://localhost/RequestManagementService"));
            requestHost.AddServiceEndpoint(typeof(IRequestManagement), binding, String.Empty);
            requestHost.Open();
        }

        //- ~UpdateConfiguration -//
        internal void UpdateConfiguration(HostConfiguration config)
        {
            using (ManagementClient client = new ManagementClient( ))
            {
                client.UpdateInstanceConfiguration(config);
            }
        }

        //- $Validate -//
        private Boolean Validate(String text, String message)
        {
            if (String.IsNullOrEmpty(text))
            {
                ReportActionMessage(message, String.Empty);
                return false;
            }
            return true;
        }

        #region IRequestSubmission

        //- @SubmitRequest -//
        public string SubmitRequest(String instanceId, Request request, Response response)
        {
            ObservableCollection<RequestResponseSet> set;
            if (!this.RequestResponseSet.TryGetValue(instanceId, out set))
            {
                set = new ObservableCollection<RequestResponseSet>();
                this.RequestResponseSet.Add(instanceId, set);
            }
            set.Insert(0, new RequestResponseSet()
            {
                Request = request,
                Response = response
            });
            return String.Empty;
        }

        #endregion
    }
}