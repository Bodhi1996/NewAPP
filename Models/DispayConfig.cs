using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewAPP.Models
{
    public class DispayConfig
    {
        public string Name { get; set; } //наименование
        public int Columns { get; set; } //колонки
        public int Sensors { get; set; } //датчики

        public override string ToString ( ) => Name;

    }
}
