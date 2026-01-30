using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    [Table("inventory_checks")]
    public class InventoryCheck
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("inventory_id")]
        public int InventoryId { get; set; }

        [Column("equipment_id")]
        public int EquipmentId { get; set; }

        [Column("checked_by_user_id")]
        public int? CheckedByUserId { get; set; }

        [Column("checked_at")]
        public DateTime? CheckedAt { get; set; }

        [Column("comment")]
        public string? Comment { get; set; }
    }
}
