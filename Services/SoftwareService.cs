using EquipmentAccounting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EquipmentAccounting.Services
{
    public class SoftwareService : ApiClient
    {
        public async Task<List<Software>?> GetAllSoftwareAsync()
        {
            try
            {
                var endpoint = $"Software";

                if (!string.IsNullOrEmpty(UserSession.Token))
                {
                    endpoint += $"?token={Uri.EscapeDataString(UserSession.Token)}";
                }

                var response = await GetAsync(endpoint);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    return JsonSerializer.Deserialize<List<Software>>(json, options);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    Console.WriteLine("Ошибка авторизации при получении списка оборудования");
                }
                else
                {
                    Console.WriteLine($"Ошибка при получении списка оборудования: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Исключение при получении списка оборудования: {ex.Message}");
            }

            return null;
        }
    }
}
