using System.Collections.Generic;
using Vps.Monkey.Common.Enums;
using Vps.Monkey.Common.Models;

namespace Vps.Monkey.Common.Helpers
{
    public interface IMonkeyHelper
    {
        List<Registrant> Registrants(string list);
        ServiceHostStatus PatientCheckUp(HostServiceInfo patient);
        List<HostServiceInfo> GetHostServiceInfo(List<Registrant> registrants);
        ServiceHostStatus GetServiceStatus(string host, string service, RegistrantType type);
        bool HostReachable(string hostName);
        ServiceHostStatus ServiceRunning(string host, string service);
        ServiceHostStatus SiteRunning(string host, string service);
        ServiceHostStatus ServiceStatus(Registrant host);

        string PrepareList(string list);

        List<Registrant> DummyRegistrantList();
        List<HostServiceInfo> DummyHostServiceInfo();
    }
}