using System.Collections.Generic;
using Vps.Monkey.Common.Models;

namespace Vps.Chaos.Helpers
{
    public interface IChaosRegistrantHelper
    {
        List<Registrant> ChaoticRegistrantsList();
        string ReadChaoticRegistrantsFile();
        void Save(string ServiceLocation, string ServiceName, string ServiceDescription, string serviceType);
        void Remove(string ServiceLocation, string ServiceName, string serviceType);
        Registrant CreateRegistrant(string ServiceLocation, string ServiceName, string ServiceDescription = "", string serviceType = "NotRequired");
        void UpdateChaoticRegistrantsFile(List<Registrant> registrants);
    }
}