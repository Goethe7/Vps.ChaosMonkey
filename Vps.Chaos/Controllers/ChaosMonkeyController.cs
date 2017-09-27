using System;
using System.Web.Http;
using System.Web.Http.Results;
using Vps.Chaos.Helpers;
using Vps.Chaos.Services;

namespace Vps.Chaos.Controllers
{
    public class ChaosMonkeyController : ApiController
    {
        private readonly IChaosRegistrantHelper _registrantHelper;
        private readonly IChaosService _chaosService;

        public ChaosMonkeyController(IChaosRegistrantHelper registrantHelper, IChaosService chaosService)
        {
            _registrantHelper = registrantHelper;
            _chaosService = chaosService;
        }

        [AcceptVerbs("POST")]
        public void Register(string serviceHost, string serviceName, string serviceDescription, string serviceType)
        {
            _registrantHelper.Save(serviceHost, serviceName, serviceDescription, serviceType);
        }

        [AcceptVerbs("POST")]
        public void UnRegister(string serviceHost, string serviceName, string serviceType)
        {
            _registrantHelper.Remove(serviceHost, serviceName, serviceType);
        }

        [AcceptVerbs("GET")]
        public JsonResult<string> Registrants() 
        {
            return Json(_registrantHelper.ReadChaoticRegistrantsFile());
        }

        [AcceptVerbs("GET", "POST")]
        [HttpGet]
        public string CauseChaos()
        {
            return _chaosService.CauseChaos();
        }

        [AcceptVerbs("GET", "POST")]
        [HttpGet]
        public void StopService(string host, string service)
        {
           _chaosService.StopService(_registrantHelper.CreateRegistrant(host, service));
        }

        [AcceptVerbs("GET", "POST")]
        [HttpGet]
        public void StartService(string host, string service)
        {
            _chaosService.StartService(_registrantHelper.CreateRegistrant(host, service));
        }

        [AcceptVerbs("GET", "POST")]
        [HttpGet]
        public void StopSite(string host, string service)
        {
            _chaosService.StopSite(_registrantHelper.CreateRegistrant(host, service));
        }

        [AcceptVerbs("GET", "POST")]
        [HttpGet]
        public void StartSite(string host, string service)
        {
            _chaosService.StartSite(_registrantHelper.CreateRegistrant(host, service));
        }

        [AcceptVerbs("GET")]
        public string HeartBeatMessage()
        {
            return $"Chaos on {this.HeartBeat():dd-MMM-yyyy @ HH:mmm:ss}";
        }

        [AcceptVerbs("GET")]
        public DateTime HeartBeat()
        {
            return DateTime.Now;
        }
    }
}
