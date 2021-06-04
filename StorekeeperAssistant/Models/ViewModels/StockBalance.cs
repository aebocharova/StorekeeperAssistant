using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StorekeeperAssistant.Models
{
    public class StockBalance
    {
        public string warehouse_name { get; set; }
        public string nomenclature_name { get; set; }
        public decimal count { get; set; }
    }

}
