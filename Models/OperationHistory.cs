using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewAPP.Models
{
    public class OperationHistory
    {
        public int Id { get; set; }
        public int NomenclatureId { get; set; }
        public string OperationType { get; set; }
        public int Quantity { get; set; }
        public DateTime OperationDate { get; set; }
        public string UserName { get; set; }
    }
}
