using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    [Table("consumable_responsible_history")]
    public class ConsumableResponsibleHistory
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "ID расходного материала обязателен для заполнения")]
        [Column("consumable_id")]
        [Range(1, int.MaxValue, ErrorMessage = "ID расходного материала должен быть положительным числом")]
        public int ConsumableId { get; set; }

        [Required(ErrorMessage = "ID ответственного пользователя обязателен для заполнения")]
        [Column("responsible_user_id")]
        [Range(1, int.MaxValue, ErrorMessage = "ID ответственного пользователя должен быть положительным числом")]
        public int ResponsibleUserId { get; set; }

        [Column("assigned_at")]
        public DateTime? AssignedAt { get; set; }

        [Column("assigned_by_user_id")]
        [Range(1, int.MaxValue, ErrorMessage = "ID пользователя, назначившего ответственного, должен быть положительным числом")]
        public int? AssignedByUserId { get; set; }

        [Column("comment")]
        [StringLength(500, ErrorMessage = "Комментарий не может превышать 500 символов")]
        public string? Comment { get; set; }
    }
}
