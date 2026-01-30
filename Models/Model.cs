using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    [Table("models")]
    public class Model
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Наименование модели обязательно для заполнения")]
        [Column("name")]
        [StringLength(200, ErrorMessage = "Наименование не может превышать 200 символов")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Тип оборудования обязателен для заполнения")]
        [Column("equipment_type_id")]
        public int EquipmentTypeId { get; set; }

        // Навигационное свойство
        [ForeignKey("EquipmentTypeId")]
        public EquipmentType? EquipmentType { get; set; }
    }
}
