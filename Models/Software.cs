using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    [Table("software")]
    public class Software
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Наименование программного обеспечения обязательно для заполнения")]
        [Column("name")]
        [StringLength(200, ErrorMessage = "Наименование не может превышать 200 символов")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Разработчик обязателен для заполнения")]
        [Column("developer_id")]
        public int DeveloperId { get; set; }

        [Column("version")]
        [StringLength(50, ErrorMessage = "Версия не может превышать 50 символов")]
        [RegularExpression(@"^[a-zA-Z0-9.\-\s]+$", ErrorMessage = "Версия может содержать только буквы, цифры, точки, дефисы и пробелы")]
        public string? Version { get; set; }

        [ForeignKey("DeveloperId")]
        public Developer? Developer { get; set; }
    }
}
