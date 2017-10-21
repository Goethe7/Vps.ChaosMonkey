using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.ServiceProcess;
using Microsoft.Web.Administration;
using Newtonsoft.Json;
using Vps.Monkey.Common.Enums;
using Vps.Monkey.Common.Models;

namespace Vps.Monkey.Common.Helpers
{
    public class MonkeyHelper : IMonkeyHelper
    {
        private const string EventSource = "Monkey Helper";

        public MonkeyHelper()
        {
            EventLogSetup();
        }

        public List<Registrant> Registrants(string list)
        {
            try
            {
                list = list.TrimStart('\"');
                list = list.TrimEnd('\"');
                list = list.Replace("\\", string.Empty);

                var registrantList = JsonConvert.DeserializeObject<List<Registrant>>(list) ?? new List<Registrant>();

                return registrantList;
            }
            catch (Exception ex)
            {
                WriteToEventLog($"Registrants() ERROR: {ex.Message}", EventLogEntryType.Error);
                throw;
            }
        }

        public List<HostServiceInfo> GetHostServiceInfo(List<Registrant> hosts)
        {
            try
            {
                return hosts.Select(host => new HostServiceInfo
                {
                    Host = host.ServiceHost,
                    Service = host.ServiceName,
                    HostReachable = HostReachable(host.ServiceHost),
                    ServiceStatus = ServiceStatus(host),
                    Type = host.RegistrantServiceType
                }).ToList();
            }
            catch (Exception ex)
            {
                WriteToEventLog($"GetHostServiceInfo() ERROR: {ex.Message}", EventLogEntryType.Error);
                throw;
            }
        }

        public bool HostReachable(string hostName)
        {
            try
            {
                var ping = new Ping();
                var reply = ping.Send(hostName, 60 * 1000);
                return reply.Status == IPStatus.Success;
            }
            catch (Exception ex)
            {
                WriteToEventLog($"HostReachable() ERROR: {ex.Message}", EventLogEntryType.Error);
                return false;
            }
        }

        public ServiceHostStatus PatientCheckUp(HostServiceInfo hostService) 
        {
            return GetServiceStatus(hostService.Host, hostService.Service, hostService.Type);
        }

        public ServiceHostStatus ServiceStatus(Registrant host)
        {
            return GetServiceStatus(host.ServiceHost, host.ServiceName, host.RegistrantServiceType);
        }

        public ServiceHostStatus GetServiceStatus(string host, string service, RegistrantType type)
        {
            var currentStatus = ServiceHostStatus.Unknown;

            try
            {
                switch (type)
                {
                    case RegistrantType.Service:
                        currentStatus = ServiceRunning(host, service);
                        break;
                    case RegistrantType.Site:
                        currentStatus = SiteRunning(host, service);
                        break;
                    case RegistrantType.NotRequired:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                return currentStatus;
            }
            catch (Exception ex)
            {
                WriteToEventLog($"GetServiceStatus() ERROR: {ex.Message}", EventLogEntryType.Error);
                throw;
            }
        }

        public ServiceHostStatus ServiceRunning(string host, string service)
        {
            try
            {
                var serviceController = new ServiceController(service, host);
                serviceController.Refresh();
                var serviceStatus = serviceController.Status;

                ServiceHostStatus status;

                switch (serviceStatus)
                {
                    case ServiceControllerStatus.ContinuePending:
                    case ServiceControllerStatus.Paused:
                    case ServiceControllerStatus.PausePending:
                        status = ServiceHostStatus.Unknown;
                        break;
                    case ServiceControllerStatus.Running:
                        status = ServiceHostStatus.Started;
                        break;
                    case ServiceControllerStatus.StartPending:
                        status = ServiceHostStatus.Starting;
                        break;
                    case ServiceControllerStatus.Stopped:
                    case ServiceControllerStatus.StopPending:
                        status = ServiceHostStatus.Stopped;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                return status;
            }
            catch (Exception ex)
            {
                WriteToEventLog($"ServiceRunning() ERROR: {ex.Message}", EventLogEntryType.Error);
                throw;
            }
        }

        public ServiceHostStatus SiteRunning(string host, string service)
        {
            try
            {
                var serverManager = ServerManager.OpenRemote(host);
                var siteStatus = serverManager.ApplicationPools[service].State;

                ServiceHostStatus status;

                switch (siteStatus)
                {
                    case ObjectState.Starting:
                        status = ServiceHostStatus.Starting;
                        break;
                    case ObjectState.Started:
                        status = ServiceHostStatus.Running;
                        break;
                    case ObjectState.Stopping:
                    case ObjectState.Stopped:
                        status = ServiceHostStatus.Stopped;
                        break;
                    case ObjectState.Unknown:
                        status = ServiceHostStatus.Unknown;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                return status;

            }
            catch (Exception ex)
            {
                WriteToEventLog($"SiteRunning() ERROR: {ex.Message + " :: " + ex.Message + " :: " + ex.StackTrace}", EventLogEntryType.Error);
                return ServiceHostStatus.Unknown;
            }
        }

        public List<Registrant> DummyRegistrantList()
        {
            var list = new List<Registrant>
                           {
                               new Registrant
                                   {
                                       RegisteredTimeStamp = DateTime.Now,
                                       RegistrantServiceType = RegistrantType.Service,
                                       ServiceDescription = "Chaos Monkey Service",
                                       ServiceHost = "GrooviHost",
                                       ServiceName = "GrooviService"
                                   },

                               new Registrant
                                   {
                                       RegisteredTimeStamp = DateTime.Now.AddMonths(-2),
                                       RegistrantServiceType = RegistrantType.Site,
                                       ServiceDescription = "Chaos Monkey Site",
                                       ServiceHost = "GrooviWebServer007",
                                       ServiceName = "GrooviSite"
                                   }
                           };

            return list;

        }

        public List<HostServiceInfo> DummyHostServiceInfo()
        {
            var list = new List<HostServiceInfo>
                           {
                               new HostServiceInfo
                                   {
                                       Host = "GrooviHost",
                                       HostReachable = true,
                                       Service = "GrooviService",
                                       ServiceStatus = ServiceHostStatus.Running,
                                       Type = RegistrantType.Service
                                   },

                               new HostServiceInfo
                                   {
                                       Host = "GrooviWebServer007",
                                       HostReachable = true,
                                       Service = "GrooviSite",
                                       ServiceStatus = ServiceHostStatus.Stopped,
                                       Type = RegistrantType.Site
                                   }
                           };


            return list;
        }

        private void EventLogSetup()
        {
            if (!EventLog.SourceExists(EventSource))
            {
                EventLog.CreateEventSource(EventSource, "Application");
                WriteToEventLog($"{EventSource} Event Log configured", EventLogEntryType.SuccessAudit);
            }
        }

        private void WriteToEventLog(string info, EventLogEntryType entryType)
        {
            Console.Write("{0} : {1}\n", DateTime.Now, info);
            EventLog.WriteEntry(EventSource, $"{DateTime.Now} : {info}", entryType);
        }
    }
}