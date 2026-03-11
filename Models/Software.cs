using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    [Table("software")]
    public class Software
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("developer_id")]
        public int DeveloperId { get; set; }

        [Column("version")]
        public string? Version { get; set; }
    }
}
