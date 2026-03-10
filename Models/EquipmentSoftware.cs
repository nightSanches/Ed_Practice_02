using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EquipmentAccounting.Models
{
    public class EquipmentSoftware
    {
        public int Id { get; set; }
        public int EquipmentId { get; set; }
        public int SoftwareId { get; set; }
    }
}
