using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Classes;
using API.Models;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryController : ControllerBase
    {
        private readonly DatabaseConnection _context;
        private readonly RoleChecker _roleChecker;

        public InventoryController(DatabaseConnection context, RoleChecker roleChecker)
        {
            _context = context;
            _roleChecker = roleChecker;
        }

        /// <summary>
        /// Получить список всех инвентаризаций
        /// </summary>
        /// <param name="token">Токен пользователя</param>
        /// <param name="search">Поиск по наименованию</param>
        /// <param name="sortBy">Поле для сортировки (id, name, startDate, endDate)</param>
        /// <param name="sortOrder">Порядок сортировки (asc, desc)</param>
        /// <returns>Список инвентаризаций</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Inventory>>> GetInventories(
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

            IQueryable<Inventory> query = _context.Inventories;

            // Применение поиска по наименованию
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(i => i.Name.Contains(search));
            }

            // Применение сортировки
            query = sortBy?.ToLower() switch
            {
                "name" => sortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(i => i.Name)
                    : query.OrderBy(i => i.Name),
                "startdate" => sortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(i => i.StartDate)
                    : query.OrderBy(i => i.StartDate),
                "enddate" => sortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(i => i.EndDate)
                    : query.OrderBy(i => i.EndDate),
                _ => sortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(i => i.Id)
                    : query.OrderBy(i => i.Id)
            };

            return await query.ToListAsync();
        }

        /// <summary>
        /// Получить инвентаризацию по id
        /// </summary>
        /// <param name="id">ID инвентаризации</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Инвентаризация</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Inventory>> GetInventory(
            [FromQuery] string? token,
            int id)
        {
            // Проверка прав на чтение
            if (!await _roleChecker.CanRead(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            var inventory = await _context.Inventories.FindAsync(id);

            if (inventory == null)
            {
                return NotFound($"Инвентаризация с ID {id} не найдена");
            }

            return inventory;
        }

        /// <summary>
        /// Создать новую инвентаризацию
        /// </summary>
        /// <param name="inventory">Данные инвентаризации</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Созданная инвентаризация</returns>
        [HttpPost]
        public async Task<ActionResult<Inventory>> CreateInventory(
            Inventory inventory,
            [FromQuery] string? token)
        {
            // Проверка прав на запись
            if (!await _roleChecker.CanWrite(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            // Валидация
            var validationResult = ValidateInventory(inventory);
            if (validationResult != null)
            {
                return BadRequest(validationResult);
            }

            _context.Inventories.Add(inventory);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetInventory), new { id = inventory.Id }, inventory);
        }

        /// <summary>
        /// Редактировать существующую инвентаризацию
        /// </summary>
        /// <param name="id">ID инвентаризации</param>
        /// <param name="inventory">Обновленные данные инвентаризации</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Результат операции</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateInventory(
            [FromQuery] string? token,
            int id,
            Inventory inventory)
        {
            // Проверка прав на запись
            if (!await _roleChecker.CanWrite(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            if (id != inventory.Id)
            {
                return BadRequest("ID в пути и в теле запроса не совпадают");
            }

            // Валидация
            var validationResult = ValidateInventory(inventory);
            if (validationResult != null)
            {
                return BadRequest(validationResult);
            }

            _context.Entry(inventory).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InventoryExists(id))
                {
                    return NotFound($"Инвентаризация с ID {id} не найдена");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        /// <summary>
        /// Проверка наличия связей инвентаризации с другими таблицами
        /// </summary>
        /// <param name="id">ID инвентаризации</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>true если есть связи, false если нет</returns>
        [HttpGet("{id}/check-relations")]
        public async Task<ActionResult<bool>> CheckInventoryRelations(
            [FromQuery] string? token,
            int id)
        {
            // Проверка прав на чтение
            if (!await _roleChecker.CanRead(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            var inventoryExists = await _context.Inventories.AnyAsync(i => i.Id == id);
            if (!inventoryExists)
            {
                return NotFound($"Инвентаризация с ID {id} не найдена");
            }

            var hasRelations = await _context.InventoryChecks.AnyAsync(ic => ic.InventoryId == id);

            return hasRelations;
        }

        /// <summary>
        /// Удалить инвентаризацию по id
        /// </summary>
        /// <param name="id">ID инвентаризации</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Результат операции</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInventory(
            [FromQuery] string? token,
            int id)
        {
            // Проверка прав на запись
            if (!await _roleChecker.CanWrite(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            var inventory = await _context.Inventories.FindAsync(id);
            if (inventory == null)
            {
                return NotFound($"Инвентаризация с ID {id} не найдена");
            }

            _context.Inventories.Remove(inventory);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool InventoryExists(int id)
        {
            return _context.Inventories.Any(i => i.Id == id);
        }

        private string? ValidateInventory(Inventory inventory)
        {
            // Проверка обязательных полей
            if (string.IsNullOrWhiteSpace(inventory.Name))
            {
                return "Наименование инвентаризации обязательно для заполнения";
            }

            // Проверка дат
            if (inventory.StartDate > inventory.EndDate)
            {
                return "Дата начала не может быть позже даты окончания";
            }

            // Проверка длины названия
            if (inventory.Name.Length > 200)
            {
                return "Наименование не может превышать 200 символов";
            }

            return null;
        }
    }
}
