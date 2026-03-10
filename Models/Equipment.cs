using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    [Table("equipment")]
    public class Equipment
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("photo")]
        public string? Photo { get; set; }

        [Column("inventory_number")]
        public int InventoryNumber { get; set; }

        [Column("room_id")]
        public int? RoomId { get; set; }

        [Column("responsible_user_id")]
        public int? ResponsibleUserId { get; set; }

        [Column("temp_responsible_user_id")]
        public int? TempResponsibleUserId { get; set; }

        [Column("cost", TypeName = "decimal(10,2)")]
        public decimal? Cost { get; set; }

        [Column("direction_id")]
        public int? DirectionId { get; set; }

        [Column("status_id")]
        public int? StatusId { get; set; }

        [Column("model_id")]
        public int? ModelId { get; set; }

        [Column("comment")]
        public string? Comment { get; set; }
    }
}
