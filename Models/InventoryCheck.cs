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

        [Required(ErrorMessage = "ID инвентаризации обязателен для заполнения")]
        [Column("inventory_id")]
        [Range(1, int.MaxValue, ErrorMessage = "ID инвентаризации должен быть положительным числом")]
        public int InventoryId { get; set; }

        [Required(ErrorMessage = "ID оборудования обязательно для заполнения")]
        [Column("equipment_id")]
        [Range(1, int.MaxValue, ErrorMessage = "ID оборудования должен быть положительным числом")]
        public int EquipmentId { get; set; }

        [Column("checked_by_user_id")]
        public int? CheckedByUserId { get; set; }

        [Column("checked_at")]
        public DateTime? CheckedAt { get; set; }

        [Column("comment")]
        [StringLength(500, ErrorMessage = "Комментарий не может превышать 500 символов")]
        public string? Comment { get; set; }
    }
}
