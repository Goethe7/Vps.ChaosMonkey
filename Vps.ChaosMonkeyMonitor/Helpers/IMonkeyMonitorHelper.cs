using System.Collections.Generic;
using Vps.ChaosMonkeyMonitor.ViewModels;
using Vps.Monkey.Common.Models;

namespace Vps.ChaosMonkeyMonitor.Helpers
{
    public interface IMonkeyMonitorHelper
    {
        List<ChaosMonitorData> MapForView(List<Registrant> registrantList, List<HostServiceInfo> hostServiceInfo);
    }
}