using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    [Table("consumable_characteristics")]
    public class ConsumableCharacteristic
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "ID типа расходного материала обязателен для заполнения")]
        [Column("consumable_type_id")]
        public int ConsumableTypeId { get; set; }

        [Required(ErrorMessage = "Наименование характеристики обязательно для заполнения")]
        [Column("name")]
        [StringLength(100, ErrorMessage = "Наименование характеристики не может превышать 100 символов")]
        public string Name { get; set; }
    }
}
