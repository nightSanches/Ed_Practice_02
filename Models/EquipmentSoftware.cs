using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    [Table("equipment_software")]
    public class EquipmentSoftware
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "ID оборудования обязательно для заполнения")]
        [Column("equipment_id")]
        public int EquipmentId { get; set; }

        [Required(ErrorMessage = "ID программного обеспечения обязательно для заполнения")]
        [Column("software_id")]
        public int SoftwareId { get; set; }
    }
}
