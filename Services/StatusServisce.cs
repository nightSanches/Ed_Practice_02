using EquipmentAccounting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EquipmentAccounting.Services
{
    public class StatusService : ApiClient
    {
        public async Task<List<Status>?> GetStatusAsync(
            string? search = null)
        {
            try
            {
                var endpoint = $"Status";
                var queryParams = new List<string>();

                if (!string.IsNullOrEmpty(UserSession.Token))
                {
                    endpoint += $"?token={Uri.EscapeDataString(UserSession.Token)}";
                }

                if (!string.IsNullOrEmpty(search))
                    queryParams.Add($"search={Uri.EscapeDataString(search)}");

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
                    return JsonSerializer.Deserialize<List<Status>>(json, options);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    Console.WriteLine("Ошибка авторизации");
                }
                else
                {
                    Console.WriteLine($"Ошибка: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Исключение: {ex.Message}");
            }

            return null;
        }

        public async Task<Status?> GetStatusByIdAsync(int id)
        {
            try
            {
                var endpoint = $"Status/{id}";
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
                    return JsonSerializer.Deserialize<Status>(json, options);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    Console.WriteLine($"Статус с ID {id} не найдено");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    Console.WriteLine("Ошибка авторизации");
                }
                else
                {
                    Console.WriteLine($"Ошибка: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Исключение: {ex.Message}");
            }

            return null;
        }

        public async Task<(Status? equipment, string? error)> CreateStatusAsync(Status equipment)
        {
            try
            {
                var endpoint = $"Status";
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
                    var createdEquipment = JsonSerializer.Deserialize<Status>(json, options);
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
                    return (null, $"Ошибка: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                return (null, $"Исключение: {ex.Message}");
            }
        }

        public async Task<string?> UpdateStatusAsync(int id, Status equipment)
        {
            try
            {
                equipment.Id = id;

                var endpoint = $"Status/{id}";
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
                    return $"Статус с ID {id} не найдено";
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return "Ошибка авторизации: недостаточно прав для выполнения операции";
                }
                else
                {
                    return $"Ошибка: {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                return $"Исключение: {ex.Message}";
            }
        }
        public async Task<string?> DeleteStatusAsync(int id)
        {
            try
            {
                var endpoint = $"Status/{id}";
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
                    return $"Статус с ID {id} не найдено";
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return "Ошибка авторизации: недостаточно прав для выполнения операции";
                }
                else
                {
                    return $"Ошибка: {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                return $"Исключение: {ex.Message}";
            }
        }
    }
}
