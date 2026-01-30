using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using API.Classes;
using API.Models;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConsumableController : ControllerBase
    {
        private readonly DatabaseConnection _context;
        private readonly RoleChecker _roleChecker;

        public ConsumableController(DatabaseConnection context, RoleChecker roleChecker)
        {
            _context = context;
            _roleChecker = roleChecker;
        }

        /// <summary>
        /// Получить список всех расходных материалов
        /// </summary>
        /// <param name="token">Токен пользователя</param>
        /// <param name="search">Поиск по наименованию</param>
        /// <param name="sortBy">Поле для сортировки (id, name, arrivalDate, quantity, consumableTypeId)</param>
        /// <param name="sortOrder">Порядок сортировки (asc, desc)</param>
        /// <returns>Список расходных материалов</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Consumable>>> GetConsumables(
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

            IQueryable<Consumable> query = _context.Consumables;

            // Применение поиска по наименованию
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c => c.Name.Contains(search));
            }

            // Применение сортировки
            query = sortBy?.ToLower() switch
            {
                "name" => sortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(c => c.Name)
                    : query.OrderBy(c => c.Name),
                "arrivaldate" => sortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(c => c.ArrivalDate)
                    : query.OrderBy(c => c.ArrivalDate),
                "quantity" => sortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(c => c.Quantity)
                    : query.OrderBy(c => c.Quantity),
                "consumabletypeid" => sortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(c => c.ConsumableTypeId)
                    : query.OrderBy(c => c.ConsumableTypeId),
                _ => sortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(c => c.Id)
                    : query.OrderBy(c => c.Id)
            };

            return await query.ToListAsync();
        }

        /// <summary>
        /// Получить расходный материал по id
        /// </summary>
        /// <param name="id">ID расходного материала</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Расходный материал</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Consumable>> GetConsumable(
            int id,
            [FromQuery] string? token)
        {
            // Проверка прав на чтение
            if (!await _roleChecker.CanRead(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            var consumable = await _context.Consumables.FindAsync(id);

            if (consumable == null)
            {
                return NotFound($"Расходный материал с ID {id} не найден");
            }

            return consumable;
        }

        /// <summary>
        /// Создать новый расходный материал
        /// </summary>
        /// <param name="consumable">Данные расходного материала</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Созданный расходный материал</returns>
        [HttpPost]
        public async Task<ActionResult<Consumable>> CreateConsumable(
            Consumable consumable,
            [FromQuery] string? token)
        {
            // Проверка прав на запись
            if (!await _roleChecker.CanWrite(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            // Валидация
            var validationResult = ValidateConsumable(consumable);
            if (validationResult != null)
            {
                return BadRequest(validationResult);
            }

            _context.Consumables.Add(consumable);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetConsumable), new { id = consumable.Id }, consumable);
        }

        /// <summary>
        /// Редактировать существующий расходный материал
        /// </summary>
        /// <param name="id">ID расходного материала</param>
        /// <param name="consumable">Обновленные данные расходного материала</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Результат операции</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateConsumable(
            int id,
            Consumable consumable,
            [FromQuery] string? token)
        {
            // Проверка прав на запись
            if (!await _roleChecker.CanWrite(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            if (id != consumable.Id)
            {
                return BadRequest("ID в пути и в теле запроса не совпадают");
            }

            // Валидация
            var validationResult = ValidateConsumable(consumable);
            if (validationResult != null)
            {
                return BadRequest(validationResult);
            }

            _context.Entry(consumable).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ConsumableExists(id))
                {
                    return NotFound($"Расходный материал с ID {id} не найден");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        /// <summary>
        /// Проверка наличия связей расходного материала с другими таблицами
        /// </summary>
        /// <param name="id">ID расходного материала</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>true если есть связи, false если нет</returns>
        [HttpGet("{id}/check-relations")]
        public async Task<ActionResult<bool>> CheckConsumableRelations(
            int id,
            [FromQuery] string? token)
        {
            // Проверка прав на чтение
            if (!await _roleChecker.CanRead(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            var consumableExists = await _context.Consumables.AnyAsync(c => c.Id == id);
            if (!consumableExists)
            {
                return NotFound($"Расходный материал с ID {id} не найден");
            }

            var hasRelations =
                await _context.ConsumableCharacteristicValues.AnyAsync(ccv => ccv.ConsumableId == id) ||
                await _context.ConsumableEquipment.AnyAsync(ce => ce.ConsumableId == id) ||
                await _context.ConsumableResponsibleHistory.AnyAsync(crh => crh.ConsumableId == id);

            return hasRelations;
        }

        /// <summary>
        /// Удалить расходный материал по id
        /// </summary>
        /// <param name="id">ID расходного материала</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Результат операции</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteConsumable(
            int id,
            [FromQuery] string? token)
        {
            // Проверка прав на запись
            if (!await _roleChecker.CanWrite(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            var consumable = await _context.Consumables.FindAsync(id);
            if (consumable == null)
            {
                return NotFound($"Расходный материал с ID {id} не найден");
            }

            _context.Consumables.Remove(consumable);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ConsumableExists(int id)
        {
            return _context.Consumables.Any(e => e.Id == id);
        }

        private string? ValidateConsumable(Consumable consumable)
        {
            // Проверка обязательных полей
            if (string.IsNullOrWhiteSpace(consumable.Name))
            {
                return "Наименование расходного материала обязательно для заполнения";
            }

            // Проверка даты поступления
            if (consumable.ArrivalDate == default)
            {
                return "Дата поступления обязательна для заполнения";
            }

            // Проверка формата даты поступления (ДД.ММ.ГГГГ)
            if (!Regex.IsMatch(consumable.ArrivalDate.ToString("dd.MM.yyyy"), @"^(0[1-9]|[12][0-9]|3[01])\.(0[1-9]|1[0-2])\.\d{4}$"))
            {
                return "Дата поступления должна быть в формате ДД.ММ.ГГГГ";
            }

            // Проверка количества
            if (consumable.Quantity < 0)
            {
                return "Количество не может быть отрицательным";
            }

            // Проверка что количество содержит только цифры
            if (!Regex.IsMatch(consumable.Quantity.ToString(), @"^\d+$"))
            {
                return "Количество должно содержать только цифры";
            }

            // Проверка типа расходного материала
            if (consumable.ConsumableTypeId <= 0)
            {
                return "Необходимо выбрать тип расходного материала";
            }

            return null;
        }
    }
}
