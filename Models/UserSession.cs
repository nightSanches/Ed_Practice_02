using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EquipmentAccounting.Models
{
    public static class UserSession
    {
        public static string Token { get; set; } = string.Empty;
        public static string Role { get; set; } = string.Empty;
        public static string FullName { get; set; } = string.Empty;
        public static DropdownData DropdownData { get; set; } = new();
        public static bool IsAuthenticated => !string.IsNullOrEmpty(Token);

        public static void Clear()
        {
            Token = string.Empty;
            Role = string.Empty;
            FullName = string.Empty;
            DropdownData = new DropdownData();
        }
    }
}
