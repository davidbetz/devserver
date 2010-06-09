using System.Collections.Generic;
using System.Windows;
//+
namespace DevServer.Client
{
    internal class CoreApplication : System.Windows.Application
    {
        private MainWindow window = null;

        //+
        //- ~CoreApplication -//
        internal CoreApplication(List<Instance> instances) {
            window = new MainWindow(instances);
        }

        //- #OnStartup -//
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            window.Show();
        }
    }
}