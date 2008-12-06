using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
//+
namespace DevServer
{
    [DataContract(Namespace = DevServer.Service.Information.Namespace.DevServer)]
    public class Instance : INotifyPropertyChanged
    {
        //- @InstanceState -//
        public enum InstanceState
        {
            Started = 0,
            Stopped = 1
        }

        //- @InstanceStatus -//
        public enum InstanceStatus
        {
            AlreadyExists = 1,
            AlreadyStarted = 2,
            ExceptionThrown = 4,
            OperationSuccess = 0,
            PortNotAvailable = 3,
            CouldNotStop = 5,
            StoppedButCouldNotStart = 6
        }

        //+
        public event PropertyChangedEventHandler PropertyChanged;
        private String boundIPAddress;
        public InstanceState state;

        //- @Ctor -//
        public Instance()
        {
        }

        //- @EnableIPAddressBinding -//
        [DataMember(IsRequired = false)]
        public Boolean EnableIPAddressBinding { get; set; }

        //- @BoundIPAddress -//
        [DataMember(IsRequired = false)]
        public String BoundIPAddress
        {
            get
            {
                if (!String.IsNullOrEmpty(boundIPAddress))
                {
                    if (boundIPAddress.ToLower(System.Globalization.CultureInfo.CurrentCulture) == "any")
                    {
                        return "Any";
                    }
                    else if (boundIPAddress.ToLower(System.Globalization.CultureInfo.CurrentCulture) == "loopback")
                    {
                        return "Loopback";
                    }
                    String pattern = @"^([1-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])(\.([0-9]|[1-9][0-9]|1[0-9][0-9]|2[0-4][0-9]|25[0-5])){3}$";
                    Regex regex = new Regex(pattern, RegexOptions.Singleline | RegexOptions.ExplicitCapture);
                    if (regex.IsMatch(boundIPAddress))
                    {
                        return boundIPAddress;
                    }
                    else
                    {
                        throw new FormatException(String.Format("Invalid IP address format ({0})", boundIPAddress));
                    }
                }
                return "Loopback";
            }
            set
            {
                boundIPAddress = value;
            }
        }

        //- @Disabled -//
        [DataMember(IsRequired = false)]
        public Boolean Disabled { get; set; }

        //- @Name -//
        [DataMember]
        public String Name { get; set; }

        //- @HostConfiguration -//
        [DataMember]
        public HostConfiguration HostConfiguration { get; set; }

        //- @Port -//
        [DataMember]
        public Int32 Port { get; set; }

        //- @VirtualPath -//
        [DataMember]
        public String VirtualPath { get; set; }

        //- @PhysicalPath -//
        [DataMember]
        public String PhysicalPath { get; set; }

        //- @Id -//
        [DataMember]
        public String Id { get; set; }

        //- @LastActionMessage -//
        [DataMember]
        public String LastActionMessage { get; set; }

        //- @State -//
        [DataMember]
        public InstanceState State 
        {
            get
            {
                return state;
            }
            set
            {
                state = value;
                UpdateProperty("State");
            }
        }

        //- @OperationStatus -//
        [DataMember]
        public InstanceStatus OperationStatus { get; set; }

        //- @AllowedContentTypes -//
        [DataMember]
        public List<String> AllowedContentTypes { get; set; }

        //- $UpdateProperty -//
        private void UpdateProperty(string p)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(p));
            }
        }
    }
}