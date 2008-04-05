using System;
using System.Windows;
//+
namespace DevServer.Client
{
    public sealed class Program
    {
        //- ~HostInformation -//
        internal class HostInformation
        {
            internal const String ManagementGuid = "6a240681-2e47-4325-9407-b0f3ada274e2";
        }

        //- @Main -//
        [STAThread, LoaderOptimization(LoaderOptimization.MultiDomainHost)]
        public static void Main(String[] args)
        {
            CommandLineDictionary dictionary = CommandLineParser.Parse(args, new String[] { "activeProfile", "serverKey" });
            if (dictionary.ContainsKey("activeProfile") && dictionary.ContainsKey("serverKey"))
            {
                throw new ArgumentException("You cannot define 'activeProfile' and 'serverKey' at the same time");
            }
            try
            {
                new CoreApplication(ServerConfiguration.ReadServerInstances(dictionary)).Run();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }
        }
    }
}

