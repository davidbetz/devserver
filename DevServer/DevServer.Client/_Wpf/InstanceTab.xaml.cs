using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using DevServer.Client.Filter;
using DevServer.Service;
using DevServer.Service.Client;
//+
namespace DevServer.Client
{
    public partial class InstanceTab : TabItem
    {
        //- @TabControl -//
        private TabControl TabControl
        {
            get
            {
                return this.Parent as TabControl;
            }
        }

        //- @Instance -//
        public Instance Instance { get; set; }

        //- @RequestResponseSet -//
        public ObservableCollection<RequestResponseSet> RequestResponseSet { get; set; }

        //- $RequestCount -//
        private Int32 RequestCount { get; set; }

        //- @WindowRequestResponseSet -//
        private ObservableCollection<RequestResponseSet> WindowRequestResponseSet
        {
            get
            {
                if (this.TabControl != null)
                {
                    if (this.TabControl.Window != null)
                    {
                        if (this.TabControl.Window.RequestResponseSet != null)
                        {
                            if (this.TabControl.Window.RequestResponseSet.Count(p => p.Key == this.Instance.Id) > 0)
                            {
                                return this.TabControl.Window.RequestResponseSet[this.Instance.Id];
                            }
                        }
                    }
                }
                return new ObservableCollection<RequestResponseSet>();
            }
        }

        //- @TimerClock -//
        public System.Windows.Threading.DispatcherTimer TimerClock { get; set; }

        //- @Instances -//
        public List<Instance> Instances
        {
            get
            {
                return this.TabControl.Window.Instances;
            }
        }

        //- @Ctor -//
        public InstanceTab(Instance initInstance)
        {
            this.Instance = initInstance;
            //+
            InitializeComponent();
            //+
            if (this.RequestResponseSet == null)
            {
                this.RequestResponseSet = new ObservableCollection<RequestResponseSet>();
            }
            this.DataContext = this;
            this.spMetadata.DataContext = this;
            spToolBox.DataContext = this;
            gMain.DataContext = this.RequestResponseSet;
            //+
            ICollectionView view = CollectionViewSource.GetDefaultView(this.RequestResponseSet);
            //+ filter
            String filter = String.Empty;
            view.Filter = (Object data) =>
            {
                RequestResponseSet set = (RequestResponseSet)data;
                return FilterManager.UpdateFilter(set, filter);
            };
            //+ sort
            SortDescription dateSortDescription = new SortDescription("Request.DateTime", ListSortDirection.Descending);
            view.SortDescriptions.Add(dateSortDescription);
            //+ setup request monitor timer
            this.TimerClock = new System.Windows.Threading.DispatcherTimer();
            this.TimerClock.Interval = new TimeSpan(0, 0, 1);
            this.TimerClock.IsEnabled = true;
            this.TimerClock.Tick += new EventHandler(TimerClock_Tick);
            //+
            btnLink.Content = String.Format("http://localhost:{0}{1}", this.Instance.Port, this.Instance.VirtualPath);
            //+ quick events
            txtFilter.TextChanged += delegate
            {
                filter = txtFilter.Text;
                view.Refresh();
            };
            btnClearTracing.Click += delegate
            {
                RequestResponseSet.Clear();
            };
            chkEnableTracing.Click += delegate
            {
                UpdateConfiguration();
            };
            chkVerboseTypeTracing.Click += delegate
            {
                UpdateConfiguration();
            };
            btnLink.Click += delegate
            {
                try
                {
                    System.Diagnostics.Process.Start(btnLink.Content.ToString());
                }
                catch(Exception ex)
                {
                    if (!(ex is Win32Exception))
                    {
                        throw;
                    }
                    //+
                    //+ sometimes an exception is thrown for no reason
                }
            };
            btnStopInstance.Click += delegate(Object sender, RoutedEventArgs e)
            {
                if (this.Instance.State == Instance.InstanceState.Started)
                {
                    StopInstance();
                }
                else
                {
                    StartupExistingInstance();
                }
            };
            btnKillInstance.Click += delegate(object sender, RoutedEventArgs e)
            {
                KillInstance();
            };
            btnRestartInstance.Click += new RoutedEventHandler(btnRestartInstance_Click);
        }

        //- $btnRestartInstance_Click -//
        private void btnRestartInstance_Click(Object sender, RoutedEventArgs e)
        {
            using (ManagementClient client = new ManagementClient("NetPipeManagementServiceEndpoint"))
            {
                String name = this.Instance.Name;
                Instance instance = client.RestartInstance(this.Instance.Id);
                //+
                this.Instance.State = instance.State;
                this.Instance.OperationStatus = instance.OperationStatus;
                this.Instance.LastActionMessage = instance.LastActionMessage;
                //+
                switch (this.Instance.OperationStatus)
                {
                    case Instance.InstanceStatus.OperationSuccess:
                        this.TabControl.Window.ReportActionMessage(String.Format("Instance {0} ({1}) successfully restarted.", this.Instance.Name, this.Instance.Id), String.Empty);
                        break;
                    case Instance.InstanceStatus.ExceptionThrown:
                        this.TabControl.Window.ReportActionMessage(String.Format("Instance {0} ({1}) restart threw and exception (see message log for details).", this.Instance.Name, this.Instance.Id), this.Instance.LastActionMessage);
                        break;
                    case Instance.InstanceStatus.CouldNotStop:
                        this.TabControl.Window.ReportActionMessage(String.Format("Unable to stop instance {0} ({1}).", this.Instance.Name, this.Instance.Id), String.Empty);
                        break;
                    case Instance.InstanceStatus.StoppedButCouldNotStart:
                        this.TabControl.Window.ReportActionMessage(String.Format("Instance {0} ({1}) was stopped and was unable to start.", this.Instance.Name, this.Instance.Id), String.Empty);
                        break;
                    default:
                        break;
                }
            }
        }

        //- $HeaderListBoxCommandBinding_Executed -//
        private void HeaderListBoxCommandBinding_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            IEnumerable<Header> headerList = ((ListBox)e.OriginalSource).SelectedItems.Cast<Header>();
            StringBuilder b = new StringBuilder();
            foreach (Header header in headerList)
            {
                b.AppendLine(String.Format("{0}: {1}", header.Name, header.Data));
            }
            //+
            Clipboard.SetData(DataFormats.Text, b.ToString());
        }

        //- $KillInstance -//
        private void KillInstance()
        {
            using (ManagementClient client = new ManagementClient("NetPipeManagementServiceEndpoint"))
            {
                client.KillInstance(this.Instance.Id);
            }
            this.TabControl.Window.ReportActionMessage(String.Format("Instance {0} ({1}) was killed by user.", this.Instance.Name, this.Instance.Id), String.Empty);
            this.TabControl.CloseTab(this);
        }

        //- $StartupExistingInstance -//
        private void StartupExistingInstance()
        {
            using (ManagementClient client = new ManagementClient("NetPipeManagementServiceEndpoint"))
            {
                String name = this.Instance.Name;
                Instance instance = client.StartupExistingInstance(this.Instance.Id);
                //+
                this.Instance.State = instance.State;
                this.Instance.OperationStatus = instance.OperationStatus;
                this.Instance.LastActionMessage = instance.LastActionMessage;
                //+
                if (this.Instance.OperationStatus == Instance.InstanceStatus.OperationSuccess)
                {
                    this.TabControl.Window.ReportActionMessage(String.Format("Instance {0} ({1}) successfully started.", this.Instance.Name, this.Instance.Id), String.Empty);
                }
                else
                {
                    this.TabControl.Window.ReportActionMessage(String.Format("Unable to startup existing instance {0} ({1}) (see message log for details).", this.Instance.Name, this.Instance.Id), this.Instance.LastActionMessage);
                }
            }
        }

        //- $StopInstance -//
        private void StopInstance()
        {
            using (ManagementClient client = new ManagementClient("NetPipeManagementServiceEndpoint"))
            {
                String name = this.Instance.Name;
                Instance instance = client.StopInstance(this.Instance.Id);
                //+
                this.Instance.State = instance.State;
                this.Instance.OperationStatus = instance.OperationStatus;
                this.Instance.LastActionMessage = instance.LastActionMessage;
                //+
                if (this.Instance.OperationStatus == Instance.InstanceStatus.OperationSuccess)
                {
                    this.TabControl.Window.ReportActionMessage(String.Format("Instance {0} ({1}) successfully stopped.", this.Instance.Name, this.Instance.Id), String.Empty);
                }
                else
                {
                    this.TabControl.Window.ReportActionMessage(String.Format("Unable to stop instance {0} ({1}) (see message log for details).", this.Instance.Name, this.Instance.Id), this.Instance.LastActionMessage);
                }
            }
        }

        //- @SubmitRequest -//
        public void SubmitRequest(Request request, Response response)
        {
            this.RequestResponseSet.Add(new RequestResponseSet()
            {
                Request = request,
                Response = response
            });
        }

        //- @TimerClock_Tick -//
        public void TimerClock_Tick(Object sender, EventArgs ea)
        {
            if (this.WindowRequestResponseSet != null)
            {
                if (this.WindowRequestResponseSet.Count > 0)//!= this.RequestCount)
                {
                    lock (this.WindowRequestResponseSet)
                    {
                        foreach (RequestResponseSet set in this.WindowRequestResponseSet)
                        {
                            RequestResponseSet.Add(set);
                        }
                        this.WindowRequestResponseSet.Clear();
                        //this.RequestCount = this.WindowRequestResponseSet.Count;
                    }
                }
            }
        }

        //- $UpdateConfiguration -//
        private void UpdateConfiguration()
        {
            this.TabControl.Window.UpdateConfiguration(new HostConfiguration
            {
                EnableTracing = this.Instance.HostConfiguration.EnableTracing,
                EnableVerboseTypeTracing = this.Instance.HostConfiguration.EnableVerboseTypeTracing,
                InstanceId = this.Instance.Id,
                AllowedContentTypes = this.Instance.HostConfiguration.AllowedContentTypes
            });
        }
    }
}