using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    [Table("directions")]
    public class Direction
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Наименование направления обязательно для заполнения")]
        [Column("name")]
        [StringLength(100, ErrorMessage = "Наименование не может превышать 100 символов")]
        public string Name { get; set; }
    }
}
