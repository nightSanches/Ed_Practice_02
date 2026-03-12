using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EquipmentAccounting.Models
{
    public class EquipmentResponsibleHistory
    {
        public int Id { get; set; }
        public int EquipmentId { get; set; }
        public int ResponsibleUserId { get; set; }
        public DateTime? AssignedAt { get; set; }
        public int? AssignedByUserId { get; set; }
        public string? Comment { get; set; }
    }
}
