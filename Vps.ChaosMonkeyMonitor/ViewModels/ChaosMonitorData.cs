using System;
using Vps.Monkey.Common.Enums;

namespace Vps.ChaosMonkeyMonitor.ViewModels
{
    public class ChaosMonitorData
    {
        public string ServiceHost { get; set; }
        public string ServiceName { get; set; }
        public string ServiceDescription { get; set; }
        public RegistrantType RegistrantServiceType { get; set; }
        public DateTime RegisteredTimeStamp { get; set; }
        public bool HostReachable { get; set; }
        public ServiceHostStatus ServiceStatus { get; set; }
    }
}