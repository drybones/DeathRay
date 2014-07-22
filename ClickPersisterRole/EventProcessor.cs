using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using System.Diagnostics;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Configuration;
using StackExchange.Redis;

using DeathRay.Shared.Models;

namespace ClickPersisterRole
{
    public class SimpleEventProcessor : IEventProcessor
    {
        PartitionContext partitionContext;
        Stopwatch checkpointStopWatch;
        CloudTable table;
        private static ConnectionMultiplexer redisConnection;
        private static ConnectionMultiplexer RedisConnection
        {
            get
            {
                if (redisConnection == null || !redisConnection.IsConnected)
                {
                    redisConnection = ConnectionMultiplexer.Connect("deathray.redis.cache.windows.net,password=u5DcJnI3cD4L39Ol1mnsqDQPPcteEzqbATxIouk92qk=");
                }
                return redisConnection;
            }
        }

        public SimpleEventProcessor()
        {
            var account = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["AzureStorageConnectionString"]);
            var client = account.CreateCloudTableClient();
            table = client.GetTableReference("MinionClicks");
            table.CreateIfNotExists();
            Trace.WriteLine("Found storage table " + table.Uri.ToString());
        }

        public Task OpenAsync(PartitionContext context)
        {
            Trace.WriteLine(string.Format("SimpleEventProcessor initialize.  Partition: '{0}', Offset: '{1}'", context.Lease.PartitionId, context.Lease.Offset));
            this.partitionContext = context;
            this.checkpointStopWatch = new Stopwatch();
            this.checkpointStopWatch.Start();
            return Task.FromResult<object>(null);
        }

        public async Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> events)
        {
            try
            {
                foreach (EventData eventData in events)
                {
                    var newData = this.DeserializeEventData(eventData);
                    string key = eventData.PartitionKey;

                    var minionClickEntity = new MinionClickEntity()
                        {
                            PartitionKey = newData.ClickTimestamp.ToString("yyyyMMdd") + "-" + newData.Minion,
                            RowKey = newData.ClickTimestamp.ToString("HHmmss.fff") + "-" + Guid.NewGuid().ToString(),
                            Minion = newData.Minion,
                            ClickTimestamp = newData.ClickTimestamp,
                            EventHubPartition = this.partitionContext.Lease.PartitionId
                        };

                    var insertOp = TableOperation.Insert(minionClickEntity);
                    table.Execute(insertOp);

                    IDatabase cache = RedisConnection.GetDatabase();
                    long value = cache.StringIncrement("MinionTotal-" + newData.Minion);

                    Trace.WriteLine(string.Format("Message received.  Partition: '{0}', User: '{1}', Clicks: '{2}'",
                        this.partitionContext.Lease.PartitionId, key, value));
                }

                //Call checkpoint every 5 minutes, so that worker can resume processing from the 5 minutes back if it restarts. 
                if (this.checkpointStopWatch.Elapsed > TimeSpan.FromMinutes(5))
                {
                    await context.CheckpointAsync();
                    lock (this)
                    {
                        this.checkpointStopWatch.Reset();
                    }
                }
            }
            catch (Exception exp)
            {
                Trace.WriteLine("Error in processing: " + exp.Message);
            }
        }

        public async Task CloseAsync(PartitionContext context, CloseReason reason)
        {
            Trace.WriteLine(string.Format("Processor Shuting Down.  Partition '{0}', Reason: '{1}'.", this.partitionContext.Lease.PartitionId, reason.ToString()));
            if (reason == CloseReason.Shutdown)
            {
                await context.CheckpointAsync();
            }
        }

        MinionClickEvent DeserializeEventData(EventData eventData)
        {
            return JsonConvert.DeserializeObject<MinionClickEvent>(Encoding.Unicode.GetString(eventData.GetBytes()));
        }
    }
}