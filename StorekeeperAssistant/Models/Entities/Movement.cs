using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StorekeeperAssistant.Models
{
    public class Movement
    {
        public int id { get; set; }
        public DateTime date_time { get; set; }
        public Warehouse from_warehouse { get; set; }
        public Warehouse to_warehouse { get; set; }
    }
}
