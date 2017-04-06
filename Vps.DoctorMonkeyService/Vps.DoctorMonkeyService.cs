using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Timers;
using Newtonsoft.Json;
using Vps.Monkey.Common.Enums;
using Vps.Monkey.Common.Helpers;

namespace Vps.DoctorMonkeyService
{
    public class DoctorMonkeyService
    {
        private const string EventSource = "Doctor Monkey Service";
        private Timer _timer;

        private IMonkeyHelper _monkeyHelper;

        public DoctorMonkeyService()
        {
            try
            {
                _monkeyHelper = new MonkeyHelper();
                EventLogSetup();

                WriteToEventLog(string.Format("{0} Started", EventSource), EventLogEntryType.SuccessAudit);
                WriteToEventLog("The Surgery is Open", EventLogEntryType.SuccessAudit);

                var timerInterval = double.Parse(ConfigurationManager.AppSettings["DoctorMonkeyTimerInterval-mins"]);
                WriteToEventLog(string.Format("Timer interval: {0}m", timerInterval), EventLogEntryType.SuccessAudit);

                _timer = new Timer(timerInterval * 60000) { AutoReset = true };
                _timer.Elapsed += (sender, eventArgs) => CarryOnDoctor();
                _timer.Start();
            }
            catch (Exception ex)
            {
                WriteToEventLog(string.Format("{0} => ERROR:: {1}", "DoctorMonkeyService()", ex.Message), EventLogEntryType.Error);
                throw;
            }
        }

        private void CarryOnDoctor()
        {
            try
            {
                var doctorUri = ConfigurationManager.AppSettings["DoctorMonkeyUri"];
                var doctorWebApiClient = new HttpClient();

                var registrants = doctorWebApiClient.GetAsync(string.Format("{0}/Registrants", doctorUri)).Result;
                var content = registrants.Content.ReadAsStringAsync().Result;

                var patients = _monkeyHelper.Registrants(content).ToList();
                var sickPatients = _monkeyHelper.GetHostServiceInfo(patients)
                                               .Where(x => x.ServiceStatus == ServiceHostStatus.Stopped ||
                                                           x.ServiceStatus == ServiceHostStatus.Stopping).ToList();

                WriteToEventLog("Registered patients: " + JsonConvert.SerializeObject(patients.Select(x => x.ServiceDescription)), EventLogEntryType.SuccessAudit);

                if (sickPatients.Any())
                {
                    WriteToEventLog("Patients feeling unwell: " + JsonConvert.SerializeObject(sickPatients.Select(x => x.Service)), EventLogEntryType.SuccessAudit);

                    foreach (var patient in sickPatients)
                    {
                        WriteToEventLog("Fixing Patient: " + patient.Service + " on " + patient.Host, EventLogEntryType.SuccessAudit);
                        var reply = doctorWebApiClient
                                        .GetAsync(string.Format("{0}/Start{1}?host={2}&service={3}", doctorUri, patient.Type, patient.Host, patient.Service))
                                        .Result;

                        var patientStatus = _monkeyHelper.PatientCheckUp(patient);

                        WriteToEventLog(string.Format("{0} on {1} : {2}", patient.Service, patient.Host, patientStatus), EventLogEntryType.Information);
                    }
                }
                else
                {
                    WriteToEventLog("All registered patients are feeling happy :)", EventLogEntryType.SuccessAudit);
                }
            }
            catch (Exception ex)
            {
                WriteToEventLog(string.Format("CarryOnDoctor() ERROR: {0}", ex.Message), EventLogEntryType.Error);
                throw;
            }
        }

        public void Start()
        {
            WriteToEventLog("Start()", EventLogEntryType.SuccessAudit);
        }

        public void Stop()
        {
            WriteToEventLog("Stop()", EventLogEntryType.SuccessAudit);
            _timer.Stop();
        }

        private void EventLogSetup()
        {
            if (!EventLog.SourceExists(EventSource))
            {
                EventLog.CreateEventSource(EventSource, "Application");
                WriteToEventLog(string.Format("{0} Event Log configured", EventSource), EventLogEntryType.SuccessAudit);
            }
        }

        private void WriteToEventLog(string info, EventLogEntryType entryType)
        {
            Console.Write("{0} : {1}\n", DateTime.Now, info);
            EventLog.WriteEntry(EventSource, string.Format("{0} : {1}", DateTime.Now, info), entryType);
        }
    }
}
