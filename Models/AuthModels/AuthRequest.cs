using System.Text.Json.Serialization;

namespace testEquipmentAccounting.Models.AuthModels
{
    public class AuthRequest
    {
        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;
    }
}
