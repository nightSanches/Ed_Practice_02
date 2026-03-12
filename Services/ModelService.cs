using EquipmentAccounting.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EquipmentAccounting.Services
{
    public class ModelService : ApiClient
    {
        public async Task<List<Model>?> GetModelAsync(
            string? search = null,
            string? sortBy = "id",
            string? sortOrder = "asc")
        {
            try
            {
                var endpoint = $"Model";
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
                    return JsonSerializer.Deserialize<List<Model>>(json, options);
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

        public async Task<Model?> GetModelByIdAsync(int id)
        {
            try
            {
                var endpoint = $"Model/{id}";
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
                    return JsonSerializer.Deserialize<Model>(json, options);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    Console.WriteLine($"Направление с ID {id} не найдено");
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

        public async Task<(Model? equipment, string? error)> CreateModelAsync(Model equipment)
        {
            try
            {
                var endpoint = $"Model";
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
                    var createdEquipment = JsonSerializer.Deserialize<Model>(json, options);
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

        public async Task<string?> UpdateModelAsync(int id, Model equipment)
        {
            try
            {
                equipment.Id = id;

                var endpoint = $"Model/{id}";
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
                    return $"Ошибка: {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                return $"Исключение: {ex.Message}";
            }
        }
        public async Task<string?> DeleteModelAsync(int id)
        {
            try
            {
                var endpoint = $"Model/{id}";
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
                    return $"Направление с ID {id} не найдено";
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
