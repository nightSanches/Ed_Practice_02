using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    [Table("network_settings")]
    public class NetworkSettings
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "ID оборудования обязательно для заполнения")]
        [Column("equipment_id")]
        [Range(1, int.MaxValue, ErrorMessage = "ID оборудования должен быть положительным числом")]
        public int EquipmentId { get; set; }

        [Required(ErrorMessage = "IP адрес обязателен для заполнения")]
        [Column("ip_address")]
        [StringLength(15, ErrorMessage = "IP адрес не может превышать 15 символов")]
        [RegularExpression(@"^(\d{1,3}\.){3}\d{1,3}$",
            ErrorMessage = "Неверный формат IP адреса. Используйте формат: XXX.XXX.XXX.XXX")]
        public string IpAddress { get; set; }

        [Required(ErrorMessage = "Маска подсети обязательна для заполнения")]
        [Column("subnet_mask")]
        [StringLength(15, ErrorMessage = "Маска подсети не может превышать 15 символов")]
        [RegularExpression(@"^(\d{1,3}\.){3}\d{1,3}$",
            ErrorMessage = "Неверный формат маски подсети. Используйте формат: XXX.XXX.XXX.XXX")]
        public string SubnetMask { get; set; }

        [Column("default_gateway")]
        [StringLength(15, ErrorMessage = "Шлюз по умолчанию не может превышать 15 символов")]
        [RegularExpression(@"^(\d{1,3}\.){3}\d{1,3}$",
            ErrorMessage = "Неверный формат шлюза по умолчанию. Используйте формат: XXX.XXX.XXX.XXX")]
        public string? DefaultGateway { get; set; }

        [Column("dns_primary")]
        [StringLength(15, ErrorMessage = "Основной DNS не может превышать 15 символов")]
        [RegularExpression(@"^(\d{1,3}\.){3}\d{1,3}$",
            ErrorMessage = "Неверный формат основного DNS. Используйте формат: XXX.XXX.XXX.XXX")]
        public string? DnsPrimary { get; set; }

        [Column("dns_secondary")]
        [StringLength(15, ErrorMessage = "Вторичный DNS не может превышать 15 символов")]
        [RegularExpression(@"^(\d{1,3}\.){3}\d{1,3}$",
            ErrorMessage = "Неверный формат вторичного DNS. Используйте формат: XXX.XXX.XXX.XXX")]
        public string? DnsSecondary { get; set; }

        [Column("mac_address")]
        [StringLength(17, ErrorMessage = "MAC адрес не может превышать 17 символов")]
        [RegularExpression(@"^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$",
            ErrorMessage = "Неверный формат MAC адреса. Используйте формат: XX:XX:XX:XX:XX:XX или XX-XX-XX-XX-XX-XX")]
        public string? MacAddress { get; set; }
    }
}
