using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EquipmentAccounting.Models
{
    public class Equipment
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Photo { get; set; }
        public int? InventoryNumber { get; set; }
        public int? RoomId { get; set; }
        public int? ResponsibleUserId { get; set; }
        public int? TempResponsibleUserId { get; set; }
        public decimal? Cost { get; set; }
        public int? DirectionId { get; set; }
        public int? StatusId { get; set; }
        public int? ModelId { get; set; }
        public string? Comment { get; set; }
    }
}
