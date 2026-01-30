using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    [Table("equipment_responsible_history")]
    public class EquipmentResponsibleHistory
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "ID оборудования обязательно для заполнения")]
        [Column("equipment_id")]
        [Range(1, int.MaxValue, ErrorMessage = "ID оборудования должно быть положительным числом")]
        public int EquipmentId { get; set; }

        [Required(ErrorMessage = "ID ответственного пользователя обязательно для заполнения")]
        [Column("responsible_user_id")]
        [Range(1, int.MaxValue, ErrorMessage = "ID ответственного пользователя должно быть положительным числом")]
        public int ResponsibleUserId { get; set; }

        [Column("assigned_at")]
        public DateTime? AssignedAt { get; set; }

        [Column("assigned_by_user_id")]
        [Range(1, int.MaxValue, ErrorMessage = "ID назначившего пользователя должно быть положительным числом")]
        public int? AssignedByUserId { get; set; }

        [Column("comment")]
        [StringLength(500, ErrorMessage = "Комментарий не может превышать 500 символов")]
        public string? Comment { get; set; }
    }
}
