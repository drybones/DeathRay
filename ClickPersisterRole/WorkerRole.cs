using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using System.Configuration;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace ClickPersisterRole
{
    public class WorkerRole : RoleEntryPoint
    {
        string eventHubName;
        EventHubConsumerGroup defaultConsumerGroup;
        string eventHubConnectionString;
        EventProcessorHost eventProcessorHost;

        public override void Run()
        {
            // This is a sample worker implementation. Replace with your logic.
            Trace.TraceInformation("ClickPersisterRole entry point called");

            EventHubClient eventHubClient = EventHubClient.CreateFromConnectionString(eventHubConnectionString, this.eventHubName);

            // Get the default Consumer Group 
            defaultConsumerGroup = eventHubClient.GetDefaultConsumerGroup();
            string blobConnectionString = ConfigurationManager.AppSettings["AzureStorageConnectionString"]; // Required for checkpoint/state 
            eventProcessorHost = new EventProcessorHost("singleworker", eventHubClient.Path, defaultConsumerGroup.GroupName, this.eventHubConnectionString, blobConnectionString);
            eventProcessorHost.RegisterEventProcessorAsync<SimpleEventProcessor>().Wait();
            
            while (true)
            {
                Thread.Sleep(10000);
                Trace.TraceInformation("Working");
            }
        }

        public override bool OnStart()
        {
            this.eventHubConnectionString = GetServiceBusConnectionString();
            this.eventHubName = "deathrayhub"; 
            
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            return base.OnStart();
        }

        private static string GetServiceBusConnectionString()
        {
            string connectionString = ConfigurationManager.AppSettings["Microsoft.ServiceBus.ConnectionString"];
            if (string.IsNullOrEmpty(connectionString))
            {
                Trace.WriteLine("Did not find Service Bus connections string in appsettings (app.config)");
                return string.Empty;
            }
            ServiceBusConnectionStringBuilder builder = new ServiceBusConnectionStringBuilder(connectionString);
            builder.TransportType = Microsoft.ServiceBus.Messaging.TransportType.Amqp;
            return builder.ToString();
        } 
    
    }
}
