using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    [Table("statuses")]
    public class Status
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Наименование статуса обязательно для заполнения")]
        [Column("name")]
        [StringLength(100, ErrorMessage = "Наименование статуса не может превышать 100 символов")]
        public string Name { get; set; }
    }
}
