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
        private readonly Timer _timer;

        private readonly IMonkeyHelper _monkeyHelper;

        public DoctorMonkeyService()
        {
            try
            {
                _monkeyHelper = new MonkeyHelper();
                EventLogSetup();

                WriteToEventLog($"{EventSource} Started", EventLogEntryType.SuccessAudit);
                WriteToEventLog("The Surgery is Open", EventLogEntryType.SuccessAudit);

                var timerInterval = double.Parse(ConfigurationManager.AppSettings["DoctorMonkeyTimerInterval-mins"]);
                WriteToEventLog($"Timer interval: {timerInterval}m", EventLogEntryType.SuccessAudit);

                _timer = new Timer(timerInterval * 60000) { AutoReset = true };
                _timer.Elapsed += (sender, eventArgs) => CarryOnDoctor();
                _timer.Start();
            }
            catch (Exception ex)
            {
                WriteToEventLog($"DoctorMonkeyService() => ERROR:: {ex.Message}", EventLogEntryType.Error);
                throw;
            }
        }

        private void CarryOnDoctor()
        {
            try
            {
                var doctorUri = ConfigurationManager.AppSettings["DoctorMonkeyUri"];
                var doctorWebApiClient = new HttpClient();

                var registrants = doctorWebApiClient.GetAsync($"{doctorUri}/Registrants").Result;
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
                                        .GetAsync($"{doctorUri}/Start{patient.Type}?host={patient.Host}&service={patient.Service}")
                                        .Result;

                        var patientStatus = _monkeyHelper.PatientCheckUp(patient);

                        WriteToEventLog($"{patient.Service} on {patient.Host} : {patientStatus}", EventLogEntryType.Information);
                    }
                }
                else
                {
                    WriteToEventLog("All registered patients are feeling happy :)", EventLogEntryType.SuccessAudit);
                }
            }
            catch (Exception ex)
            {
                WriteToEventLog($"CarryOnDoctor() ERROR: {ex.Message}", EventLogEntryType.Error);
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