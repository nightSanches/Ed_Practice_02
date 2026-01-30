using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using API.Classes;
using API.Models;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EquipmentController : ControllerBase
    {
        private readonly DatabaseConnection _context;
        private readonly RoleChecker _roleChecker;

        public EquipmentController(DatabaseConnection context, RoleChecker roleChecker)
        {
            _context = context;
            _roleChecker = roleChecker;
        }

        /// <summary>
        /// Получить список всего оборудования
        /// </summary>
        /// <param name="token">Токен пользователя</param>
        /// <param name="search">Поиск по наименованию</param>
        /// <param name="sortBy">Поле для сортировки (id, name, inventoryNumber, cost)</param>
        /// <param name="sortOrder">Порядок сортировки (asc, desc)</param>
        /// <returns>Список оборудования</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Equipment>>> GetEquipment(
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

            IQueryable<Equipment> query = _context.Equipment;

            // Применение поиска по наименованию
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(e => e.Name.Contains(search));
            }

            // Применение сортировки
            query = sortBy?.ToLower() switch
            {
                "name" => sortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(e => e.Name)
                    : query.OrderBy(e => e.Name),
                "inventorynumber" => sortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(e => e.InventoryNumber)
                    : query.OrderBy(e => e.InventoryNumber),
                "cost" => sortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(e => e.Cost)
                    : query.OrderBy(e => e.Cost),
                _ => sortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(e => e.Id)
                    : query.OrderBy(e => e.Id)
            };

            return await query.ToListAsync();
        }

        /// <summary>
        /// Получить оборудование по id
        /// </summary>
        /// <param name="id">ID оборудования</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Оборудование</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Equipment>> GetEquipment(
            [FromQuery] string? token,
            int id)
        {
            // Проверка прав на чтение
            if (!await _roleChecker.CanRead(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            var equipment = await _context.Equipment.FindAsync(id);

            if (equipment == null)
            {
                return NotFound($"Оборудование с ID {id} не найдено");
            }

            return equipment;
        }

        /// <summary>
        /// Создать новое оборудование
        /// </summary>
        /// <param name="equipment">Данные оборудования</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Созданное оборудование</returns>
        [HttpPost]
        public async Task<ActionResult<Equipment>> CreateEquipment(
            Equipment equipment,
            [FromQuery] string? token)
        {
            // Проверка прав на запись
            if (!await _roleChecker.CanWrite(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            // Валидация
            var validationResult = ValidateEquipment(equipment);
            if (validationResult != null)
            {
                return BadRequest(validationResult);
            }

            // Проверка уникальности инвентарного номера
            if (await _context.Equipment.AnyAsync(e => e.InventoryNumber == equipment.InventoryNumber))
            {
                return BadRequest("Инвентарный номер должен быть уникальным");
            }

            _context.Equipment.Add(equipment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEquipment), new { id = equipment.Id }, equipment);
        }

        /// <summary>
        /// Редактировать существующее оборудование
        /// </summary>
        /// <param name="id">ID оборудования</param>
        /// <param name="equipment">Обновленные данные оборудования</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Результат операции</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEquipment(
            [FromQuery] string? token,
            int id,
            Equipment equipment)
        {
            // Проверка прав на запись
            if (!await _roleChecker.CanWrite(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            if (id != equipment.Id)
            {
                return BadRequest("ID в пути и в теле запроса не совпадают");
            }

            // Валидация
            var validationResult = ValidateEquipment(equipment);
            if (validationResult != null)
            {
                return BadRequest(validationResult);
            }

            // Проверка уникальности инвентарного номера (исключая текущую запись)
            if (await _context.Equipment.AnyAsync(e => e.InventoryNumber == equipment.InventoryNumber && e.Id != id))
            {
                return BadRequest("Инвентарный номер должен быть уникальным");
            }

            _context.Entry(equipment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EquipmentExists(id))
                {
                    return NotFound($"Оборудование с ID {id} не найдено");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        /// <summary>
        /// Проверка наличия связей оборудования с другими таблицами
        /// </summary>
        /// <param name="id">ID оборудования</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>true если есть связи, false если нет</returns>
        [HttpGet("{id}/check-relations")]
        public async Task<ActionResult<bool>> CheckEquipmentRelations(
            [FromQuery] string? token,
            int id)
        {
            // Проверка прав на чтение
            if (!await _roleChecker.CanRead(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            var equipmentExists = await _context.Equipment.AnyAsync(e => e.Id == id);
            if (!equipmentExists)
            {
                return NotFound($"Оборудование с ID {id} не найдено");
            }

            var hasRelations =
                await _context.EquipmentSoftware.AnyAsync(es => es.EquipmentId == id) ||
                await _context.ConsumableEquipment.AnyAsync(ce => ce.EquipmentId == id) ||
                await _context.EquipmentResponsibleHistory.AnyAsync(erh => erh.EquipmentId == id) ||
                await _context.EquipmentRoomHistory.AnyAsync(erh => erh.EquipmentId == id) ||
                await _context.InventoryChecks.AnyAsync(ic => ic.EquipmentId == id) ||
                await _context.NetworkSettings.AnyAsync(ns => ns.EquipmentId == id);

            return hasRelations;
        }

        /// <summary>
        /// Удалить оборудование по id
        /// </summary>
        /// <param name="id">ID оборудования</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Результат операции</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEquipment(
            [FromQuery] string? token,
            int id)
        {
            // Проверка прав на запись
            if (!await _roleChecker.CanWrite(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            var equipment = await _context.Equipment.FindAsync(id);
            if (equipment == null)
            {
                return NotFound($"Оборудование с ID {id} не найдено");
            }

            _context.Equipment.Remove(equipment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EquipmentExists(int id)
        {
            return _context.Equipment.Any(e => e.Id == id);
        }

        private string? ValidateEquipment(Equipment equipment)
        {
            // Проверка обязательных полей
            if (string.IsNullOrWhiteSpace(equipment.Name))
            {
                return "Наименование оборудования обязательно для заполнения";
            }

            if (equipment.InventoryNumber <= 0)
            {
                return "Инвентарный номер должен быть положительным числом";
            }

            // Проверка формата инвентарного номера (только цифры)
            if (!Regex.IsMatch(equipment.InventoryNumber.ToString(), @"^\d+$"))
            {
                return "Инвентарный номер должен содержать только цифры";
            }

            // Проверка стоимости (если указана)
            if (equipment.Cost.HasValue)
            {
                if (equipment.Cost.Value < 0)
                {
                    return "Стоимость не может быть отрицательной";
                }

                // Проверка что в стоимости только цифры и точка
                if (!Regex.IsMatch(equipment.Cost.Value.ToString(), @"^\d+$"))
                {
                    return "Стоимость должна содержать только цифры";
                }
            }

            return null;
        }
    }
}
