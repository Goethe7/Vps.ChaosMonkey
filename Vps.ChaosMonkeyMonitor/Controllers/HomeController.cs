using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Web.Mvc;
using Vps.ChaosMonkeyMonitor.Helpers;
using Vps.Monkey.Common.Helpers;

namespace Vps.ChaosMonkeyMonitor.Controllers
{
    public class HomeController : Controller
    {
        private readonly IMonkeyHelper _monkeyHelper;
        private readonly IMonkeyMonitorHelper _monkeyMonitorHelper;

        public HomeController(IMonkeyHelper monkeyHelper, IMonkeyMonitorHelper monkeyMonitorHelper)
        {
            _monkeyHelper = monkeyHelper;
            _monkeyMonitorHelper = monkeyMonitorHelper;
        }

        [HttpGet]
        public ActionResult Index()
        {
            //var chaosUri = ConfigurationManager.AppSettings["ChaosUri"];
            //var chaosWebApiClient = new HttpClient();

            //var response = chaosWebApiClient.GetAsync($"{chaosUri}/Registrants").Result;
            //var content = response.Content.ReadAsStringAsync().Result;

            //var registrantList = _monkeyHelper.Registrants(content).ToList();
            //var hostServiceInfo = _monkeyHelper.GetHostServiceInfo(registrantList).ToList();

            var registrantList = _monkeyHelper.DummyRegistrantList();
            var hostServiceInfo = _monkeyHelper.DummyHostServiceInfo();


            var chaosMonitorData = _monkeyMonitorHelper.MapForView(registrantList, hostServiceInfo);

            return View(chaosMonitorData);
        }

        [HttpPost]
        public ActionResult ServiceAction(string host, string service, string type, string action)
        {
            var chaosUri = ConfigurationManager.AppSettings["ChaosUri"];
            var chaosWebApiClient = new HttpClient();

            var response = chaosWebApiClient.GetAsync($"{chaosUri}/{action}{type}?host={host}&service={service}").Result;

            return RedirectToAction("Index");
        }

        [HttpGet]
        public DateTime HeartBeat()
        {
            return DateTime.Now;
        }
    }
}