using Vps.Monkey.Common.Models;

namespace Vps.Chaos.Services
{
    public interface IChaosService
    {
        string CauseChaos();

        void StopService(Registrant target);

        void StartService(Registrant target);

        void StopSite(Registrant target);

        void StartSite(Registrant target);

    }
}