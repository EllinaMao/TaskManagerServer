﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagerServer
{
    public class ProcessInfo
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public ProcessInfo()
        {
            Name = string.Empty;
            Id = 0;
        }
    }
}
