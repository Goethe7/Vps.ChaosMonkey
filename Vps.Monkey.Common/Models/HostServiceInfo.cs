using Vps.Monkey.Common.Enums;

namespace Vps.Monkey.Common.Models
{
    public class HostServiceInfo
    {
        public string Service { get; set; }
        public string Host { get; set; }
        public bool HostReachable { get; set; }
        public ServiceHostStatus ServiceStatus { get; set; }
        public RegistrantType Type { get; set; }

    }
}