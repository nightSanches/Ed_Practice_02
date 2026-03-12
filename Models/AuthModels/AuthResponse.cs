using System.Text.Json.Serialization;

namespace EquipmentAccounting.Models.AuthModels
{
    public class AuthResponse
    {
        [JsonPropertyName("token")]
        public string Token { get; set; } = string.Empty;

        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("id")]
        public int Id { get; set; } = int.MaxValue;

        [JsonPropertyName("fullName")]
        public string FullName { get; set; } = string.Empty;
    }
}
