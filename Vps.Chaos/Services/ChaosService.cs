﻿using System;
using System.ServiceProcess;
using Microsoft.Web.Administration;
using Vps.Chaos.Helpers;
using Vps.Monkey.Common.Enums;
using Vps.Monkey.Common.Models;

namespace Vps.Chaos.Services
{
    public class ChaosService : IChaosService
    {
        private IChaosRegistrantHelper _registrantHelper;

        public ChaosService(IChaosRegistrantHelper registrantHelper)
        {
            _registrantHelper = registrantHelper;
        }

        public string CauseChaos()
        {
            var chaosVictim = _registrantHelper.ChaoticRegistrantsList().RandomCandidateForChaos();

            try
            {
                switch (chaosVictim.RegistrantServiceType)
                {
                    case RegistrantType.Service:
                        StopService(chaosVictim);
                        break;

                    case RegistrantType.Site:
                        StopSite(chaosVictim);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                var message = string.Format("Service: {0} | {1} | {2} : STOPPED", chaosVictim.ServiceHost,
                                                                           chaosVictim.ServiceName,
                                                                           chaosVictim.ServiceDescription);

                return message;
            }
            catch (Exception ex)
            {
                return string.Format("## ERROR: Host: {0} | ServiceName: {1} | Description: {2} | ERROR: {3} ##",
                                                                              chaosVictim.ServiceHost,
                                                                              chaosVictim.ServiceName,
                                                                              chaosVictim.ServiceDescription,
                                                                              ex.Message);
            }
        }

        public void StopService(Registrant target)
        {
            var service = new ServiceController(target.ServiceName, target.ServiceHost);

            if (service.Status == ServiceControllerStatus.Stopped ||
                service.Status == ServiceControllerStatus.StopPending) return;

            var timeout = TimeSpan.FromMilliseconds(10000);

            service.Stop();
            service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
        }

        public void StartService(Registrant target)
        {
            var service = new ServiceController(target.ServiceName, target.ServiceHost);

            if (service.Status == ServiceControllerStatus.Running ||
                service.Status == ServiceControllerStatus.StartPending) return;

            var timeout = TimeSpan.FromMilliseconds(10000);

            service.Start();
            service.WaitForStatus(ServiceControllerStatus.Running, timeout);
        }

        public void StopSite(Registrant target)
        {
            ServerManager.OpenRemote(target.ServiceHost)
                         .ApplicationPools[target.ServiceName]
                         .Stop();

        }

        public void StartSite(Registrant target)
        {
            ServerManager.OpenRemote(target.ServiceHost)
                         .ApplicationPools[target.ServiceName]
                         .Start();
        }

    }
}