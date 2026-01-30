using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    [Table("inventories")]
    public class Inventory
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Наименование инвентаризации обязательно для заполнения")]
        [Column("name")]
        [StringLength(200, ErrorMessage = "Наименование не может превышать 200 символов")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Дата начала обязательна для заполнения")]
        [Column("start_date")]
        public DateOnly StartDate { get; set; }

        [Required(ErrorMessage = "Дата окончания обязательна для заполнения")]
        [Column("end_date")]
        public DateOnly EndDate { get; set; }

        [Column("created_by_user_id")]
        public int? CreatedByUserId { get; set; }
    }
}
