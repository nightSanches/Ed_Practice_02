using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EquipmentAccounting.Models
{
    public class NetworkSettings
    {
        public int Id { get; set; }
        public int EquipmentId { get; set; }
        public string IpAddress { get; set; }
        public string SubnetMask { get; set; }
        public string? DefaultGateway { get; set; }
        public string? DnsPrimary { get; set; }
        public string? DnsSecondary { get; set; }
        public string? MacAddress { get; set; }
    }
}