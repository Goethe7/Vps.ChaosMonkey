using System;
using Vps.Monkey.Common.Enums;

namespace Vps.Monkey.Common.Models
{
    public class Registrant
    {
        public string ServiceHost { get; set; }
        public string ServiceName { get; set; }
        public string ServiceDescription { get; set; }
        public RegistrantType RegistrantServiceType { get; set; }
        public DateTime RegisteredTimeStamp { get; set; }
    }
}