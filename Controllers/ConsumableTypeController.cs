using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Classes;
using API.Models;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConsumableTypeController : ControllerBase
    {
        private readonly DatabaseConnection _context;
        private readonly RoleChecker _roleChecker;

        public ConsumableTypeController(DatabaseConnection context, RoleChecker roleChecker)
        {
            _context = context;
            _roleChecker = roleChecker;
        }

        /// <summary>
        /// Получить список всех типов расходников
        /// </summary>
        /// <param name="token">Токен пользователя</param>
        /// <param name="search">Поиск по наименованию</param>
        /// <returns>Список типов расходников</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ConsumableType>>> GetConsumableTypes(
            [FromQuery] string? token,
            [FromQuery] string? search = null)
        {
            // Проверка прав на чтение
            if (!await _roleChecker.CanRead(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            IQueryable<ConsumableType> query = _context.ConsumableTypes;

            // Применение поиска по наименованию
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(ct => ct.Name.Contains(search));
            }

            // Сортировка по id по умолчанию
            query = query.OrderBy(ct => ct.Id);

            return await query.ToListAsync();
        }

        /// <summary>
        /// Получить тип расходников по id
        /// </summary>
        /// <param name="id">ID типа расходников</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Тип расходников</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ConsumableType>> GetConsumableType(
            [FromQuery] string? token,
            int id)
        {
            // Проверка прав на чтение
            if (!await _roleChecker.CanRead(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            var consumableType = await _context.ConsumableTypes.FindAsync(id);

            if (consumableType == null)
            {
                return NotFound($"Тип расходников с ID {id} не найден");
            }

            return consumableType;
        }

        /// <summary>
        /// Создать новый тип расходников
        /// </summary>
        /// <param name="consumableType">Данные типа расходников</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Созданный тип расходников</returns>
        [HttpPost]
        public async Task<ActionResult<ConsumableType>> CreateConsumableType(
            ConsumableType consumableType,
            [FromQuery] string? token)
        {
            // Проверка прав на запись
            if (!await _roleChecker.CanWrite(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            // Валидация
            var validationResult = ValidateConsumableType(consumableType);
            if (validationResult != null)
            {
                return BadRequest(validationResult);
            }

            // Проверка уникальности наименования
            if (await _context.ConsumableTypes.AnyAsync(ct => ct.Name == consumableType.Name))
            {
                return BadRequest("Наименование типа расходников должно быть уникальным");
            }

            _context.ConsumableTypes.Add(consumableType);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetConsumableType), new { id = consumableType.Id }, consumableType);
        }

        /// <summary>
        /// Редактировать существующий тип расходников
        /// </summary>
        /// <param name="id">ID типа расходников</param>
        /// <param name="consumableType">Обновленные данные типа расходников</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Результат операции</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateConsumableType(
            [FromQuery] string? token,
            int id,
            ConsumableType consumableType)
        {
            // Проверка прав на запись
            if (!await _roleChecker.CanWrite(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            if (id != consumableType.Id)
            {
                return BadRequest("ID в пути и в теле запроса не совпадают");
            }

            // Валидация
            var validationResult = ValidateConsumableType(consumableType);
            if (validationResult != null)
            {
                return BadRequest(validationResult);
            }

            // Проверка уникальности наименования (исключая текущую запись)
            if (await _context.ConsumableTypes.AnyAsync(ct => ct.Name == consumableType.Name && ct.Id != id))
            {
                return BadRequest("Наименование типа расходников должно быть уникальным");
            }

            _context.Entry(consumableType).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ConsumableTypeExists(id))
                {
                    return NotFound($"Тип расходников с ID {id} не найден");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        /// <summary>
        /// Проверка наличия связей типа оборудования с другими таблицами
        /// </summary>
        /// <param name="id">ID типа расходников</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>true если есть связи, false если нет</returns>
        [HttpGet("{id}/check-relations")]
        public async Task<ActionResult<bool>> CheckConsumableTypeRelations(
            [FromQuery] string? token,
            int id)
        {
            // Проверка прав на чтение
            if (!await _roleChecker.CanRead(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            var consumableTypeExists = await _context.ConsumableTypes.AnyAsync(ct => ct.Id == id);
            if (!consumableTypeExists)
            {
                return NotFound($"Тип расходников с ID {id} не найден");
            }

            var hasRelations =
                await _context.Consumables.AnyAsync(c => c.ConsumableTypeId == id) ||
                await _context.ConsumableCharacteristics.AnyAsync(cc => cc.ConsumableTypeId == id);

            return hasRelations;
        }

        /// <summary>
        /// Удалить тип расходников по id
        /// </summary>
        /// <param name="id">ID типа расходников</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Результат операции</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteConsumableType(
            [FromQuery] string? token,
            int id)
        {
            // Проверка прав на запись
            if (!await _roleChecker.CanWrite(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            var consumableType = await _context.ConsumableTypes.FindAsync(id);
            if (consumableType == null)
            {
                return NotFound($"Тип расходников с ID {id} не найден");
            }

            _context.ConsumableTypes.Remove(consumableType);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ConsumableTypeExists(int id)
        {
            return _context.ConsumableTypes.Any(ct => ct.Id == id);
        }

        private string? ValidateConsumableType(ConsumableType consumableType)
        {
            // Проверка обязательных полей
            if (string.IsNullOrWhiteSpace(consumableType.Name))
            {
                return "Наименование типа расходников обязательно для заполнения";
            }

            // Проверка длины наименования
            if (consumableType.Name.Length > 100)
            {
                return "Наименование не может превышать 100 символов";
            }

            return null;
        }
    }
}
