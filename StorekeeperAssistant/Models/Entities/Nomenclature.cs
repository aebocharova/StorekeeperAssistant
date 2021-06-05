﻿using System;

namespace StorekeeperAssistant.Models
{
    public class Nomenclature
    {
        public Nomenclature()
        {
            this.id = 0;
            this.name = "";
        }
        public Nomenclature(int id, string name)
        {
            this.id = id;
            this.name = name;
        }

        public int id { get; set; }
        public String name { get; set; }
    }
}
