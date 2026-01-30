using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    [Table("rooms")]
    public class Room
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Наименование аудитории обязательно для заполнения")]
        [Column("name")]
        [StringLength(100, ErrorMessage = "Наименование не может превышать 100 символов")]
        public string Name { get; set; }

        [Column("short_name")]
        [StringLength(20, ErrorMessage = "Сокращенное наименование не может превышать 20 символов")]
        public string? ShortName { get; set; }

        [Column("responsible_user_id")]
        public int? ResponsibleUserId { get; set; }

        [Column("temp_responsible_user_id")]
        public int? TempResponsibleUserId { get; set; }
    }
}
