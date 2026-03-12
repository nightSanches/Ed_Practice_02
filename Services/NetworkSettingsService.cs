using EquipmentAccounting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EquipmentAccounting.Services
{
    public class NetworkSettingsService : ApiClient
    {
        public async Task<List<NetworkSettings>?> GetNetworkAsync(
            string? search = null,
            string? sortBy = "id",
            string? sortOrder = "asc")
        {
            try
            {
                var endpoint = $"NetworkSettings";
                var queryParams = new List<string>();

                if (!string.IsNullOrEmpty(UserSession.Token))
                {
                    endpoint += $"?token={Uri.EscapeDataString(UserSession.Token)}";
                }

                if (!string.IsNullOrEmpty(search))
                    queryParams.Add($"search={Uri.EscapeDataString(search)}");

                if (!string.IsNullOrEmpty(sortBy))
                    queryParams.Add($"sortBy={Uri.EscapeDataString(sortBy)}");

                if (!string.IsNullOrEmpty(sortOrder))
                    queryParams.Add($"sortOrder={Uri.EscapeDataString(sortOrder)}");

                if (queryParams.Any())
                    endpoint += $"&{string.Join("&", queryParams)}";

                var response = await GetAsync(endpoint);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    return JsonSerializer.Deserialize<List<NetworkSettings>>(json, options);
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

        public async Task<NetworkSettings?> GetNetworkByIdAsync(int id)
        {
            try
            {
                var endpoint = $"NetworkSettings/{id}";
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
                    return JsonSerializer.Deserialize<NetworkSettings>(json, options);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    Console.WriteLine($"Оборудование с ID {id} не найдено");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    Console.WriteLine("Ошибка авторизации при получении оборудования");
                }
                else
                {
                    Console.WriteLine($"Ошибка при получении оборудования: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Исключение при получении оборудования: {ex.Message}");
            }

            return null;
        }

        public async Task<(NetworkSettings? equipment, string? error)> CreateNetworkAsync(NetworkSettings equipment)
        {
            try
            {
                var endpoint = $"NetworkSettings";
                if (!string.IsNullOrEmpty(UserSession.Token))
                {
                    endpoint += $"?token={Uri.EscapeDataString(UserSession.Token)}";
                }
                var response = await PostAsync(endpoint, equipment);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    var createdEquipment = JsonSerializer.Deserialize<NetworkSettings>(json, options);
                    return (createdEquipment, null);
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

        public async Task<string?> UpdateNetworkAsync(int id, NetworkSettings equipment)
        {
            try
            {
                equipment.Id = id;

                var endpoint = $"NetworkSettings/{id}";
                if (!string.IsNullOrEmpty(UserSession.Token))
                {
                    endpoint += $"?token={Uri.EscapeDataString(UserSession.Token)}";
                }
                var response = await PutAsync(endpoint, equipment);

                if (response.IsSuccessStatusCode)
                {
                    return null; // Успешно
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return $"Ошибка валидации: {errorContent}";
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
                    return $"Ошибка при обновлении оборудования: {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                return $"Исключение при обновлении оборудования: {ex.Message}";
            }
        }
        public async Task<string?> DeleteNetworkAsync(int id)
        {
            try
            {
                var endpoint = $"NetworkSettings/{id}";
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
