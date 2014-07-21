using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace ClickPersisterRole
{
    public class MinionClickEntity : TableEntity
    {
        public string Minion { get; set; }
        public DateTime ClickTimestamp { get; set; }
        public string EventHubPartition { get; set; }
    }
}
