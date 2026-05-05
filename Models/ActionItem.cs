using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NewAPP.Models
{
    public class ActionItem
    {
        public string Name { get; set; }
        public ICommand Command { get; set; }

        public override string ToString ( ) => Name;
    }
}
