using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using EquipmentAccounting.Models;

namespace EquipmentAccounting.Services
{
    public class DropdownsService : ApiClient
    {
        public async Task<DropdownData?> LoadAllDropdownDataAsync()
        {
            try
            {
                var dropdownData = new DropdownData();

                var tasks = new List<Task>();

                tasks.Add(Task.Run(async () =>
                    dropdownData.ConsumableTypes = await GetDropdownListAsync("dropdown/consumable-types")));

                tasks.Add(Task.Run(async () =>
                    dropdownData.Users = await GetDropdownListAsync("dropdown/users")));

                tasks.Add(Task.Run(async () =>
                    dropdownData.Consumables = await GetDropdownListAsync("dropdown/consumables")));

                tasks.Add(Task.Run(async () =>
                    dropdownData.ConsumableCharacteristics = await GetDropdownListAsync("dropdown/consumable-characteristics")));

                tasks.Add(Task.Run(async () =>
                    dropdownData.Equipment = await GetDropdownListAsync("dropdown/equipment")));

                tasks.Add(Task.Run(async () =>
                    dropdownData.Rooms = await GetDropdownListAsync("dropdown/rooms")));

                tasks.Add(Task.Run(async () =>
                    dropdownData.Software = await GetDropdownListAsync("dropdown/software")));

                tasks.Add(Task.Run(async () =>
                    dropdownData.Inventories = await GetDropdownListAsync("dropdown/inventories")));

                tasks.Add(Task.Run(async () =>
                    dropdownData.EquipmentTypes = await GetDropdownListAsync("dropdown/equipment-types")));

                tasks.Add(Task.Run(async () =>
                    dropdownData.Developers = await GetDropdownListAsync("dropdown/developers")));

                tasks.Add(Task.Run(async () =>
                    dropdownData.Statuses = await GetDropdownListAsync("dropdown/statuses")));

                tasks.Add(Task.Run(async () =>
                    dropdownData.Directions = await GetDropdownListAsync("dropdown/directions")));

                tasks.Add(Task.Run(async () =>
                    dropdownData.Models = await GetDropdownListAsync("dropdown/models")));

                await Task.WhenAll(tasks);
                return dropdownData;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке данных выпадающих списков: {ex.Message}");
                return null;
            }
        }

        private async Task<List<DropdownItem>> GetDropdownListAsync(string endpoint)
        {
            try
            {
                var response = await SendRequestAsync(HttpMethod.Get, endpoint, null);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    return JsonSerializer.Deserialize<List<DropdownItem>>(json, options) ?? new List<DropdownItem>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении списка {endpoint}: {ex.Message}");
            }

            return new List<DropdownItem>();
        }
    }
}
