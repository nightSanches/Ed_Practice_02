using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    [Table("equipment")]
    public class Equipment
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Наименование оборудования обязательно для заполнения")]
        [Column("name")]
        [StringLength(200, ErrorMessage = "Наименование не может превышать 200 символов")]
        public string Name { get; set; }

        [Column("photo")]
        public string? Photo { get; set; }

        [Required(ErrorMessage = "Инвентарный номер обязателен для заполнения")]
        [Column("inventory_number")]
        [RegularExpression(@"^\d+$", ErrorMessage = "Инвентарный номер должен содержать только цифры")]
        public int InventoryNumber { get; set; }

        [Column("room_id")]
        public int? RoomId { get; set; }

        [Column("responsible_user_id")]
        public int? ResponsibleUserId { get; set; }

        [Column("temp_responsible_user_id")]
        public int? TempResponsibleUserId { get; set; }

        [Column("cost", TypeName = "decimal(10,2)")]
        public decimal? Cost { get; set; }

        [Column("direction_id")]
        public int? DirectionId { get; set; }

        [Column("status_id")]
        public int? StatusId { get; set; }

        [Column("model_id")]
        public int? ModelId { get; set; }

        [Column("comment")]
        public string? Comment { get; set; }
    }
}
