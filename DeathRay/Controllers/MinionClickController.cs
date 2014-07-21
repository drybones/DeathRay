using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System.Configuration;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;

using DeathRay.Shared.Models;
using DeathRay.Helpers;

using System.Text;
using System.Diagnostics;

namespace DeathRay.Controllers
{
    public class MinionClickController : Controller
    {
        private readonly EventHubClient client;

        public MinionClickController() 
        {
            string connectionString = GetServiceBusConnectionString();
            client = EventHubClient.Create("deathrayhub");
        }
        // GET: MinionClick
        [Authorize]
        [RequireHttps]
        public ActionResult Click()
        {
            var username = User.Identity.GetUserName();
            var minionClick = new MinionClickEvent() { Minion = username, ClickTimestamp = DateTime.Now };

            // Send the data to the EventHub
            Trace.WriteLine("Sending messages to Event Hub: "+ client.Path);
            var serializedString = JsonConvert.SerializeObject(minionClick);
            EventData data = new EventData(Encoding.Unicode.GetBytes(serializedString))
                {
                    PartitionKey = username
                };

            client.SendAsync(data);

            return Json(MinionClickTotalHelper.GetMinionClickTotal(username));
        }

        public ActionResult Total()
        {
            return Json(MinionClickTotalHelper.GetMinionClickTotal(User.Identity.GetUserName()), JsonRequestBehavior.AllowGet);
        }

        private static string GetServiceBusConnectionString()
        {
            string connectionString = ConfigurationManager.AppSettings["Microsoft.ServiceBus.ConnectionString"];
            if (string.IsNullOrEmpty(connectionString))
            {
                Trace.WriteLine("Did not find Service Bus connections string in appsettings.");
                return string.Empty;
            }
            ServiceBusConnectionStringBuilder builder = new ServiceBusConnectionStringBuilder(connectionString);
            builder.TransportType = TransportType.Amqp;
            return builder.ToString();
        } 
    }
}