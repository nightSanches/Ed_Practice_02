using EquipmentAccounting.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EquipmentAccounting.Services
{
    public class EquipmentResponsibleHistoryService : ApiClient
    {
        public async Task<List<EquipmentResponsibleHistory>?> GetEquipmentResponsibleHistoryAsync(int Id)
        {
            try
            {
                var endpoint = $"EquipmentResponsibleHistory";

                endpoint += $"?equipmentId=" + Id;

                if (!string.IsNullOrEmpty(UserSession.Token))
                {
                    endpoint += $"&token={Uri.EscapeDataString(UserSession.Token)}";
                }

                var response = await GetAsync(endpoint);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    return JsonSerializer.Deserialize<List<EquipmentResponsibleHistory>>(json, options);
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

        public async Task<(EquipmentResponsibleHistory? equipmentSoftware, string? error)> CreateEquipmentResponsibleHistoryAsync(EquipmentResponsibleHistory equipmentSoftware)
        {
            try
            {
                var endpoint = $"EquipmentResponsibleHistory";
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
                    var createdEquipmentSoftware = JsonSerializer.Deserialize<EquipmentResponsibleHistory>(json, options);
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
                    return (null, $"Ошибка: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                return (null, $"Исключение при создании оборудования: {ex.Message}");
            }
        }

        public async Task<string?> DeleteEquipmentResponsibleHistoryAsync(int id)
        {
            try
            {
                var endpoint = $"EquipmentResponsibleHistory";

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
                    return $"Запись с ID {id} не найдена";
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
