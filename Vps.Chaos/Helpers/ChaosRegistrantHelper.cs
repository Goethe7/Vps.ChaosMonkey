using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web.Hosting;
using Newtonsoft.Json;
using Vps.Monkey.Common.Enums;
using Vps.Monkey.Common.Models;

namespace Vps.Chaos.Helpers
{
    public class ChaosRegistrantHelper : IChaosRegistrantHelper
    {
        public List<Registrant> ChaoticRegistrantsList()
        {
            var jsonRegistrants = ReadChaoticRegistrantsFile();
            return JsonConvert.DeserializeObject<List<Registrant>>(jsonRegistrants) ?? new List<Registrant>();
        }

        public string ReadChaoticRegistrantsFile()
        {
            return File.ReadAllText(HostingEnvironment.MapPath(ConfigurationManager.AppSettings["ChaoticRegistrantsFile"]));
        }

        public void Save(string serviceLocation, string serviceName, string serviceDescription, string serviceType)
        {
            var chaosRegistrant = CreateRegistrant(serviceLocation, serviceName, serviceDescription, serviceType);
            var registrants = ChaoticRegistrantsList();

            if (registrants.Any(r => r.ServiceHost.ToLower() == serviceLocation.ToLower() &&
                                     r.ServiceName.ToLower() == serviceName.ToLower()))
                return;

            registrants.Add(chaosRegistrant);
            UpdateChaoticRegistrantsFile(registrants);
        }

        public void Remove(string serviceHost, string serviceName, string serviceType)
        {
            var registrants = ChaoticRegistrantsList();
            registrants.Remove(registrants.FirstOrDefault(c => c.ServiceHost == serviceHost &&
                                                               c.ServiceName == serviceName));
            UpdateChaoticRegistrantsFile(registrants);
        }

        public Registrant CreateRegistrant(string serviceHost, string serviceName, string serviceDescription, string serviceType)
        {
            return new Registrant
            {
                ServiceHost = serviceHost,
                ServiceName = serviceName,
                ServiceDescription = serviceDescription,
                RegistrantServiceType = ParseEnum<RegistrantType>(serviceType),
                RegisteredTimeStamp = DateTime.Now
            };
        }

        public void UpdateChaoticRegistrantsFile(List<Registrant> registrants)
        {
            var jsonData = JsonConvert.SerializeObject(registrants);
            File.WriteAllText(HostingEnvironment.MapPath(ConfigurationManager.AppSettings["ChaoticRegistrantsFile"]), jsonData);
        }

        public static T ParseEnum<T>(string value)
        {
            return (T)Enum.Parse(typeof(T), value, ignoreCase: true);
        }
    }
}