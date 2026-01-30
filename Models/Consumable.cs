using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    [Table("consumables")]
    public class Consumable
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Наименование расходного материала обязательно для заполнения")]
        [Column("name")]
        [StringLength(200, ErrorMessage = "Наименование не может превышать 200 символов")]
        public string Name { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Дата поступления обязательна для заполнения")]
        [Column("arrival_date")]
        [RegularExpression(@"^(0[1-9]|[12][0-9]|3[01])\.(0[1-9]|1[0-2])\.\d{4}$",
            ErrorMessage = "Дата поступления должна быть в формате ДД.ММ.ГГГГ")]
        public DateOnly ArrivalDate { get; set; }

        [Column("photo")]
        public byte[]? Photo { get; set; }

        [Required(ErrorMessage = "Количество обязательно для заполнения")]
        [Column("quantity")]
        [RegularExpression(@"^\d+$", ErrorMessage = "Количество должно содержать только цифры")]
        [Range(0, int.MaxValue, ErrorMessage = "Количество не может быть отрицательным")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Тип расходного материала обязателен для заполнения")]
        [Column("consumable_type_id")]
        [Range(1, int.MaxValue, ErrorMessage = "Необходимо выбрать тип расходного материала")]
        public int ConsumableTypeId { get; set; }

        [Column("responsible_user_id")]
        public int? ResponsibleUserId { get; set; }

        [Column("temp_responsible_user_id")]
        public int? TempResponsibleUserId { get; set; }
    }
}
