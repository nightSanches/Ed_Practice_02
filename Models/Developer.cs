using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    [Table("developers")]
    public class Developer
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Наименование разработчика обязательно для заполнения")]
        [Column("name")]
        [StringLength(100, ErrorMessage = "Наименование не может превышать 100 символов")]
        public string Name { get; set; }
    }
}
