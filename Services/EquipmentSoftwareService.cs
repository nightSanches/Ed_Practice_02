using EquipmentAccounting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EquipmentAccounting.Services
{
    public class EquipmentSoftwareService : ApiClient
    {
        public async Task<List<EquipmentSoftware>?> GetEquipmentSoftwareAsync(int Id)
        {
            try
            {
                var endpoint = $"EquipmentSoftware";

                endpoint += $"/by-equipment/" + Id;

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
                    return JsonSerializer.Deserialize<List<EquipmentSoftware>>(json, options);
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

        public async Task<(EquipmentSoftware? equipmentSoftware, string? error)> CreateEquipmentSoftwareAsync(EquipmentSoftware equipmentSoftware)
        {
            try
            {
                var endpoint = $"EquipmentSoftware";
                if (!string.IsNullOrEmpty(UserSession.Token))
                {
                    endpoint += $"?token={Uri.EscapeDataString(UserSession.Token)}";
                }
                var response = await PostAsync(endpoint, equipmentSoftware);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    var createdEquipmentSoftware = JsonSerializer.Deserialize<EquipmentSoftware>(json, options);
                    return (createdEquipmentSoftware, null);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return (null, $"Ошибка валидации: {errorContent}");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return (null, "Ошибка авторизации: недостаточно прав для выполнения операции");
                }
                else
                {
                    return (null, $"Ошибка при создании оборудования: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                return (null, $"Исключение при создании оборудования: {ex.Message}");
            }
        }

        public async Task<string?> DeleteEquipmentSoftwareAsync(int id)
        {
            try
            {
                var endpoint = $"EquipmentSoftware";

                endpoint += $"/" + id;

                if (!string.IsNullOrEmpty(UserSession.Token))
                {
                    endpoint += $"?token={Uri.EscapeDataString(UserSession.Token)}";
                }

                var response = await DeleteAsync(endpoint);

                if (response.IsSuccessStatusCode)
                {
                    return null; // Успешно
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return $"Оборудование с ID {id} не найдено";
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return "Ошибка авторизации: недостаточно прав для выполнения операции";
                }
                else
                {
                    return $"Ошибка при удалении оборудования: {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                return $"Исключение при удалении оборудования: {ex.Message}";
            }
        }
    }
}
