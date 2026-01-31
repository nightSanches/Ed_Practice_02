using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    [Table("consumable_characteristic_values")]
    public class ConsumableCharacteristicValue
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "ID расходного материала обязателен для заполнения")]
        [Column("consumable_id")]
        [Range(1, int.MaxValue, ErrorMessage = "ID расходного материала должен быть положительным числом")]
        public int ConsumableId { get; set; }

        [Required(ErrorMessage = "ID характеристики обязателен для заполнения")]
        [Column("characteristic_id")]
        [Range(1, int.MaxValue, ErrorMessage = "ID характеристики должен быть положительным числом")]
        public int CharacteristicId { get; set; }

        [Column("value")]
        [StringLength(500, ErrorMessage = "Значение характеристики не может превышать 500 символов")]
        public string? Value { get; set; }
    }
}
