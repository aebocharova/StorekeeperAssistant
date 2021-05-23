using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StorekeeperAssistant.Models
{
    public class MovementContent
    {
        public Movement movement { get; set; }
        public Nomenclature nomenclature { get; set; }
        public int count { get; set; }
    }
}
