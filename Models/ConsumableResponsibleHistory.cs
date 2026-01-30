using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    [Table("consumable_responsible_history")]
    public class ConsumableResponsibleHistory
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("consumable_id")]
        public int ConsumableId { get; set; }

        [Column("responsible_user_id")]
        public int ResponsibleUserId { get; set; }

        [Column("assigned_at")]
        public DateTime? AssignedAt { get; set; }

        [Column("assigned_by_user_id")]
        public int? AssignedByUserId { get; set; }

        [Column("comment")]
        public string? Comment { get; set; }
    }
}
