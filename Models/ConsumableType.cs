using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    [Table("consumable_types")]
    public class ConsumableType
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Наименование типа расходника обязательно для заполнения")]
        [Column("name")]
        [StringLength(100, ErrorMessage = "Наименование не может превышать 100 символов")]
        public string Name { get; set; }

        [Column("description")]
        public string? Description { get; set; }
    }
}