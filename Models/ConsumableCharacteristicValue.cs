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

        [Column("consumable_id")]
        public int ConsumableId { get; set; }

        [Column("characteristic_id")]
        public int CharacteristicId { get; set; }

        [Column("value")]
        [StringLength(500)]
        public string? Value { get; set; }
    }
}
