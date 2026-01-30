using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    [Table("equipment_room_history")]
    public class EquipmentRoomHistory
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "ID оборудования обязательно для заполнения")]
        [Column("equipment_id")]
        [Range(1, int.MaxValue, ErrorMessage = "ID оборудования должен быть положительным числом")]
        public int EquipmentId { get; set; }

        [Required(ErrorMessage = "ID аудитории обязательно для заполнения")]
        [Column("room_id")]
        [Range(1, int.MaxValue, ErrorMessage = "ID аудитории должен быть положительным числом")]
        public int RoomId { get; set; }

        [Column("moved_at")]
        public DateTime? MovedAt { get; set; }

        [Column("moved_by_user_id")]
        [Range(1, int.MaxValue, ErrorMessage = "ID пользователя должен быть положительным числом")]
        public int? MovedByUserId { get; set; }

        [Column("comment")]
        [StringLength(1000, ErrorMessage = "Комментарий не может превышать 1000 символов")]
        public string? Comment { get; set; }
    }
}
