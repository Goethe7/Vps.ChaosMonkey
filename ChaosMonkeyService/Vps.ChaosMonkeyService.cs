using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Timers;
using Vps.Chaos.Models;

namespace ChaosMonkeyService
{
    public class ChaosMonkeyService
    {
        private const string EventSource = "Chaos Monkey Service v1.0";
        private Timer _chaosTimer;
        private static Random randomChaos = new Random();
        private static int _randomControlNumberMax;

        public ChaosMonkeyService()
        {
            try
            {
                EventLogSetup();

                WriteToEventLog(string.Format("{0} Started", EventSource), EventLogEntryType.SuccessAudit);
                WriteToEventLog(string.Format("{0} Event Log configured", EventSource), EventLogEntryType.SuccessAudit);

                var chaosTimerInterval = double.Parse(ConfigurationManager.AppSettings["ChaosTimerInterval-mins"]);
                WriteToEventLog(string.Format("Timer interval: {0}m", chaosTimerInterval), EventLogEntryType.SuccessAudit);

                _randomControlNumberMax = int.Parse(ConfigurationManager.AppSettings["RandomControlNumberMax"]);

                _chaosTimer = new Timer(chaosTimerInterval * 60000) { AutoReset = true };
                _chaosTimer.Elapsed += (sender, eventArgs) => ChaosCasino();
                _chaosTimer.Start();
            }
            catch (Exception ex)
            {
                WriteToEventLog(string.Format("ERROR: {0}", ex.Message), EventLogEntryType.Error);
                throw;
            }
        }

        private void ChaosCasino()
        {
            try
            {
                WriteToEventLog("Chaos Service Ping", EventLogEntryType.SuccessAudit);

                var currentDateTime = DateTime.Now;
                var chaosTimeWindowStart = int.Parse(ConfigurationManager.AppSettings["ChaosTimeWindowStart"]);
                var chaosTimeWindowEnd = int.Parse(ConfigurationManager.AppSettings["ChaosTimeWindowEnd"]);
                var chaosDaysOfWeek = ConfigurationManager.AppSettings["ChaosDaysOfWeek"].Split(',').ToList();
                var chaosDays = new List<ChaosDay>();

                chaosDays.AddRange(chaosDaysOfWeek.Select(day => new ChaosDay { DayOfWeek = int.Parse(day) }));

                if (chaosDays.All(d => d.DayOfWeek != (int)currentDateTime.DayOfWeek) ||
                    currentDateTime.Hour < chaosTimeWindowStart ||
                    currentDateTime.Hour > chaosTimeWindowEnd - 1) // max if chaosTimeWindowEnd = 17, 16:59
                {
                    WriteToEventLog("Out of hours", EventLogEntryType.SuccessAudit);
                    return;
                }

                if (CauseRandomChaos())
                {
                    WriteToEventLog(":} Chaos Monkey is out to cause mischief ... cheeky little monkey!", EventLogEntryType.SuccessAudit);
                    ReleaseChaosMonkey();
                }
                else
                {
                    WriteToEventLog("Chaos Monkey is not feeling mischievous, but maybe next time .... ;}", EventLogEntryType.SuccessAudit);
                }

            }
            catch (Exception ex)
            {
                WriteToEventLog(string.Format("ERROR: {0}", ex.Message), EventLogEntryType.Error);
                throw;
            }

        }

        public void Start()
        {
        }

        public void Stop()
        {
            _chaosTimer.Stop();
        }

        private void EventLogSetup()
        {
            if (!EventLog.SourceExists(EventSource))
            {
                EventLog.CreateEventSource(EventSource, "Application");
            }
        }

        private void WriteToEventLog(string info, EventLogEntryType entryType)
        {
            Console.Write("{0} : {1}\n", DateTime.Now, info);
            EventLog.WriteEntry(EventSource, string.Format("{0} : {1}", DateTime.Now, info), entryType);
        }

        private bool CauseRandomChaos()
        {
            var valueForChaos = new Random().Next(1, _randomControlNumberMax + 1);
            var randomValue = randomChaos.Next(1, _randomControlNumberMax + 1);
            WriteToEventLog(string.Format("{0}/{1}", valueForChaos, randomValue), EventLogEntryType.Information);
            return randomValue == valueForChaos;
        }

        private void ReleaseChaosMonkey()
        {
            var chaosUri = ConfigurationManager.AppSettings["ChaosUri"];
            var chaosWebApiClient = new HttpClient();

            var response = chaosWebApiClient.GetAsync(string.Format("{0}/CauseChaos", chaosUri)).Result;
            WriteToEventLog(string.Format("{0}/CauseChaos", chaosUri), EventLogEntryType.SuccessAudit);
            var content = response.Content.ReadAsStringAsync().Result;
            WriteToEventLog("[Response Content] " + content, EventLogEntryType.SuccessAudit);
        }
    }
}
