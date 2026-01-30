using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using API.Classes;
using API.Models;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NetworkSettingsController : ControllerBase
    {
        private readonly DatabaseConnection _context;
        private readonly RoleChecker _roleChecker;

        public NetworkSettingsController(DatabaseConnection context, RoleChecker roleChecker)
        {
            _context = context;
            _roleChecker = roleChecker;
        }

        /// <summary>
        /// Получить список всех сетевых настроек
        /// </summary>
        /// <param name="token">Токен пользователя</param>
        /// <param name="search">Поиск по IP адресу</param>
        /// <param name="sortBy">Поле для сортировки (id, ipAddress, macAddress, equipmentId)</param>
        /// <param name="sortOrder">Порядок сортировки (asc, desc)</param>
        /// <returns>Список сетевых настроек</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<NetworkSettings>>> GetNetworkSettings(
            [FromQuery] string? token,
            [FromQuery] string? search = null,
            [FromQuery] string? sortBy = "id",
            [FromQuery] string? sortOrder = "asc")
        {
            // Проверка прав на чтение
            if (!await _roleChecker.CanRead(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            IQueryable<NetworkSettings> query = _context.NetworkSettings;

            // Применение поиска по IP адресу
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(n => n.IpAddress.Contains(search));
            }

            // Применение сортировки
            query = sortBy?.ToLower() switch
            {
                "ipaddress" => sortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(n => n.IpAddress)
                    : query.OrderBy(n => n.IpAddress),
                "macaddress" => sortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(n => n.MacAddress)
                    : query.OrderBy(n => n.MacAddress),
                "equipmentid" => sortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(n => n.EquipmentId)
                    : query.OrderBy(n => n.EquipmentId),
                _ => sortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(n => n.Id)
                    : query.OrderBy(n => n.Id)
            };

            return await query.ToListAsync();
        }

        /// <summary>
        /// Получить сетевую настройку по id
        /// </summary>
        /// <param name="id">ID сетевой настройки</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Сетевая настройка</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<NetworkSettings>> GetNetworkSetting(
            [FromQuery] string? token,
            int id)
        {
            // Проверка прав на чтение
            if (!await _roleChecker.CanRead(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            var networkSettings = await _context.NetworkSettings.FindAsync(id);

            if (networkSettings == null)
            {
                return NotFound($"Сетевая настройка с ID {id} не найдена");
            }

            return networkSettings;
        }

        /// <summary>
        /// Создать новую сетевую настройку
        /// </summary>
        /// <param name="networkSettings">Данные сетевой настройки</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Созданная сетевая настройка</returns>
        [HttpPost]
        public async Task<ActionResult<NetworkSettings>> CreateNetworkSetting(
            NetworkSettings networkSettings,
            [FromQuery] string? token)
        {
            // Проверка прав на запись
            if (!await _roleChecker.CanWrite(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            // Валидация модели
            var validationResult = ValidateNetworkSettings(networkSettings);
            if (validationResult != null)
            {
                return BadRequest(validationResult);
            }

            // Проверка существования оборудования
            var equipmentExists = await _context.Equipment.AnyAsync(e => e.Id == networkSettings.EquipmentId);
            if (!equipmentExists)
            {
                return BadRequest($"Оборудование с ID {networkSettings.EquipmentId} не найдено");
            }

            // Проверка уникальности IP адреса
            if (await _context.NetworkSettings.AnyAsync(n => n.IpAddress == networkSettings.IpAddress))
            {
                return BadRequest("IP адрес должен быть уникальным");
            }

            _context.NetworkSettings.Add(networkSettings);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetNetworkSetting), new { id = networkSettings.Id }, networkSettings);
        }

        /// <summary>
        /// Редактировать существующую сетевую настройку
        /// </summary>
        /// <param name="id">ID сетевой настройки</param>
        /// <param name="networkSettings">Обновленные данные сетевой настройки</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Результат операции</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNetworkSetting(
            [FromQuery] string? token,
            int id,
            NetworkSettings networkSettings)
        {
            // Проверка прав на запись
            if (!await _roleChecker.CanWrite(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            if (id != networkSettings.Id)
            {
                return BadRequest("ID в пути и в теле запроса не совпадают");
            }

            // Валидация модели
            var validationResult = ValidateNetworkSettings(networkSettings);
            if (validationResult != null)
            {
                return BadRequest(validationResult);
            }

            // Проверка существования оборудования
            var equipmentExists = await _context.Equipment.AnyAsync(e => e.Id == networkSettings.EquipmentId);
            if (!equipmentExists)
            {
                return BadRequest($"Оборудование с ID {networkSettings.EquipmentId} не найдено");
            }

            // Проверка уникальности IP адреса (исключая текущую запись)
            if (await _context.NetworkSettings.AnyAsync(n => n.IpAddress == networkSettings.IpAddress && n.Id != id))
            {
                return BadRequest("IP адрес должен быть уникальным");
            }

            _context.Entry(networkSettings).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NetworkSettingsExists(id))
                {
                    return NotFound($"Сетевая настройка с ID {id} не найдена");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        /// <summary>
        /// Удалить сетевую настройку по id
        /// </summary>
        /// <param name="id">ID сетевой настройки</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Результат операции</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNetworkSetting(
            [FromQuery] string? token,
            int id)
        {
            // Проверка прав на запись
            if (!await _roleChecker.CanWrite(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            var networkSettings = await _context.NetworkSettings.FindAsync(id);
            if (networkSettings == null)
            {
                return NotFound($"Сетевая настройка с ID {id} не найдена");
            }

            _context.NetworkSettings.Remove(networkSettings);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool NetworkSettingsExists(int id)
        {
            return _context.NetworkSettings.Any(n => n.Id == id);
        }

        private string? ValidateNetworkSettings(NetworkSettings networkSettings)
        {
            // Проверка обязательных полей
            if (string.IsNullOrWhiteSpace(networkSettings.IpAddress))
            {
                return "IP адрес обязателен для заполнения";
            }

            if (string.IsNullOrWhiteSpace(networkSettings.SubnetMask))
            {
                return "Маска подсети обязательна для заполнения";
            }

            // Проверка формата IP адреса и диапазона значений
            if (!IsValidIpAddress(networkSettings.IpAddress))
            {
                return "Неверный формат IP адреса. Используйте формат: XXX.XXX.XXX.XXX, где XXX от 0 до 255";
            }

            // Проверка формата маски подсети и диапазона значений
            if (!IsValidIpAddress(networkSettings.SubnetMask))
            {
                return "Неверный формат маски подсети. Используйте формат: XXX.XXX.XXX.XXX, где XXX от 0 до 255";
            }

            // Проверка шлюза по умолчанию (если указан)
            if (!string.IsNullOrWhiteSpace(networkSettings.DefaultGateway))
            {
                if (!IsValidIpAddress(networkSettings.DefaultGateway))
                {
                    return "Неверный формат шлюза по умолчанию. Используйте формат: XXX.XXX.XXX.XXX, где XXX от 0 до 255";
                }
            }

            // Проверка основного DNS (если указан)
            if (!string.IsNullOrWhiteSpace(networkSettings.DnsPrimary))
            {
                if (!IsValidIpAddress(networkSettings.DnsPrimary))
                {
                    return "Неверный формат основного DNS. Используйте формат: XXX.XXX.XXX.XXX, где XXX от 0 до 255";
                }
            }

            // Проверка вторичного DNS (если указан)
            if (!string.IsNullOrWhiteSpace(networkSettings.DnsSecondary))
            {
                if (!IsValidIpAddress(networkSettings.DnsSecondary))
                {
                    return "Неверный формат вторичного DNS. Используйте формат: XXX.XXX.XXX.XXX, где XXX от 0 до 255";
                }
            }

            // Проверка MAC адреса (если указан)
            if (!string.IsNullOrWhiteSpace(networkSettings.MacAddress))
            {
                if (!Regex.IsMatch(networkSettings.MacAddress,
                    @"^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$"))
                {
                    return "Неверный формат MAC адреса. Используйте формат: XX:XX:XX:XX:XX:XX или XX-XX-XX-XX-XX-XX";
                }
            }

            return null;
        }

        private bool IsValidIpAddress(string ipAddress)
        {
            // Проверка формата XXX.XXX.XXX.XXX
            var match = Regex.Match(ipAddress, @"^(\d{1,3})\.(\d{1,3})\.(\d{1,3})\.(\d{1,3})$");
            if (!match.Success)
                return false;

            // Проверка что каждая часть в диапазоне 0-255
            for (int i = 1; i <= 4; i++)
            {
                if (!int.TryParse(match.Groups[i].Value, out int part))
                    return false;

                if (part < 0 || part > 255)
                    return false;
            }

            return true;
        }
    }
}
