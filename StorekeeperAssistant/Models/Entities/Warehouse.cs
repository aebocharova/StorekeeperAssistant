using System;

namespace StorekeeperAssistant.Models
{
    public class Warehouse
    {
        public Warehouse()
        {
        }

        public Warehouse(int id, string name)
        {
            this.id = id;
            this.name = name;
        }

        public int id { get; set; }
        public String name { get; set; }
    }
}
