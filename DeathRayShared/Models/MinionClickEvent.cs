using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DeathRay.Shared.Models
{
    public class MinionClickEvent
    {
        public string Minion { get; set; }
        public DateTime ClickTimestamp { get; set; }
    }
}