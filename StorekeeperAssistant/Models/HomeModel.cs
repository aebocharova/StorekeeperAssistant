using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StorekeeperAssistant.Models;

namespace StorekeeperAssistant.Models
{
    public class HomeModel
    {
        public List<Warehouse> warehouses { get; set; }
        public List<Movement> movements { get; set; }
    }
}
