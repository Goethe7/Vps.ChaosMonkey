using System.Collections.Generic;
using System.Linq;
using Vps.ChaosMonkeyMonitor.ViewModels;
using Vps.Monkey.Common.Models;

namespace Vps.ChaosMonkeyMonitor.Helpers
{
    public class MonkeyMonitorHelper : IMonkeyMonitorHelper
    {
        public List<ChaosMonitorData> MapForView(List<Registrant> registrantList, List<HostServiceInfo> hostServiceInfo)
        {
            return registrantList.Select(item => new ChaosMonitorData
            {
                ServiceHost = item.ServiceHost,
                ServiceName = item.ServiceName,
                ServiceDescription = item.ServiceDescription,
                RegistrantServiceType = item.RegistrantServiceType,
                RegisteredTimeStamp = item.RegisteredTimeStamp,
                HostReachable = hostServiceInfo.FirstOrDefault(h => h.Host.ToLower() == item.ServiceHost.ToLower()).HostReachable,
                ServiceStatus = hostServiceInfo.FirstOrDefault(h => h.Service.ToLower() == item.ServiceName.ToLower()).ServiceStatus
            }).ToList();
        }
    }
}