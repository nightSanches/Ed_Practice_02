using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    [Table("consumable_equipment")]
    public class ConsumableEquipment
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "ID расходного материала обязателен для заполнения")]
        [Column("consumable_id")]
        [Range(1, int.MaxValue, ErrorMessage = "ID расходного материала должен быть положительным числом")]
        public int ConsumableId { get; set; }

        [Required(ErrorMessage = "ID оборудования обязательно для заполнения")]
        [Column("equipment_id")]
        [Range(1, int.MaxValue, ErrorMessage = "ID оборудования должен быть положительным числом")]
        public int EquipmentId { get; set; }

        [Required(ErrorMessage = "Количество использованных единиц обязательно для заполнения")]
        [Column("quantity_used")]
        [Range(1, int.MaxValue, ErrorMessage = "Количество использованных единиц должно быть положительным числом")]
        public int QuantityUsed { get; set; }

        [Column("attached_at")]
        public DateTime? AttachedAt { get; set; }

        [Column("attached_by_user_id")]
        [Range(1, int.MaxValue, ErrorMessage = "ID пользователя должен быть положительным числом")]
        public int? AttachedByUserId { get; set; }
    }
}
