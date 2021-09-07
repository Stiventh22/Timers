using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace Timers.Fuctions.Entities
{
    public class TimersEntity: TableEntity
    {
        public int IdEmployee { get; set; }
        public DateTime WorkTime { get; set; }
        public int Type { get; set; }
        public bool Consolidated { get; set; }
    }
}
