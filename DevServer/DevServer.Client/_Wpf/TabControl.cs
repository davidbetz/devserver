using System;
using System.Windows;
using DevServer.Service;
//+
namespace DevServer.Client
{
    class TabControl : System.Windows.Controls.TabControl
    {
        //- @Window -//
        public MainWindow Window
        {
            get
            {
                return ((FrameworkElement)this.Parent).Parent as MainWindow;
            }
        }

        //- @SubmitRequest -//
        public void SubmitRequest(String instanceId, Request request, Response Response)
        {
            for (int i = 0; i < this.Items.Count; i++)
            {
                InstanceTab tab = this.Items[i] as InstanceTab;
                if (tab != null && tab.Instance.Id == instanceId)
                {
                    tab.SubmitRequest(request, Response);
                }
            }
        }

        //- ~CloseTab -//
        internal void CloseTab(InstanceTab instanceTab)
        {
            this.Items.Remove(instanceTab);
        }
    }
}
