using System.Collections.Generic;

namespace StorekeeperAssistant.Models
{
    public class HomeModel
    {
        public Dictionary<int, Warehouse> warehouses { get; set; }
        public List<Movement> movements { get; set; }
    }
}
