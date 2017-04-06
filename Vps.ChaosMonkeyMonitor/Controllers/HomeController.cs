﻿using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Web.Mvc;
using Vps.ChaosMonkeyMonitor.Helpers;
using Vps.Monkey.Common.Helpers;

namespace Vps.ChaosMonkeyMonitor.Controllers
{
    public class HomeController : Controller
    {
        private IMonkeyHelper _monkeyHelper;
        private IMonkeyMonitorHelper _monkeyMonitorHelper;

        public HomeController(IMonkeyHelper monkeyHelper, IMonkeyMonitorHelper monkeyMonitorHelper)
        {
            _monkeyHelper = monkeyHelper;
            _monkeyMonitorHelper = monkeyMonitorHelper;
        }

        [HttpGet]
        public ActionResult Index()
        {
            var chaosUri = ConfigurationManager.AppSettings["ChaosUri"];
            var chaosWebApiClient = new HttpClient();

            var response = chaosWebApiClient.GetAsync(string.Format("{0}/Registrants", chaosUri)).Result;
            var content = response.Content.ReadAsStringAsync().Result;

            var registrantList = _monkeyHelper.Registrants(content).ToList();
            var hostServiceInfo = _monkeyHelper.GetHostServiceInfo(registrantList).ToList();
            var chaosMonitorData = _monkeyMonitorHelper.MapForView(registrantList, hostServiceInfo);

            return View(chaosMonitorData);
        }

        [HttpPost]
        public ActionResult ServiceAction(string host, string service, string type, string action)
        {
            var chaosUri = ConfigurationManager.AppSettings["ChaosUri"];
            var chaosWebApiClient = new HttpClient();

            var response = chaosWebApiClient.GetAsync(string.Format("{0}/{1}{2}?host={3}&service={4}", chaosUri, action, type, host, service)).Result;

            return RedirectToAction("Index");
        }

    }
}