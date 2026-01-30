using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Classes;
using API.Models;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConsumableCharacteristicController : ControllerBase
    {
        private readonly DatabaseConnection _context;
        private readonly RoleChecker _roleChecker;

        public ConsumableCharacteristicController(DatabaseConnection context, RoleChecker roleChecker)
        {
            _context = context;
            _roleChecker = roleChecker;
        }

        /// <summary>
        /// Получить список всех характеристик расходников
        /// </summary>
        /// <param name="token">Токен пользователя</param>
        /// <param name="search">Поиск по наименованию</param>
        /// <param name="sortBy">Поле для сортировки (id, name, consumableTypeId)</param>
        /// <param name="sortOrder">Порядок сортировки (asc, desc)</param>
        /// <returns>Список характеристик расходников</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ConsumableCharacteristic>>> GetConsumableCharacteristics(
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

            IQueryable<ConsumableCharacteristic> query = _context.ConsumableCharacteristics;

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
        /// Получить характеристику расходников по id
        /// </summary>
        /// <param name="id">ID характеристики</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Характеристика расходников</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ConsumableCharacteristic>> GetConsumableCharacteristic(
            [FromQuery] string? token,
            int id)
        {
            // Проверка прав на чтение
            if (!await _roleChecker.CanRead(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            var consumableCharacteristic = await _context.ConsumableCharacteristics.FindAsync(id);

            if (consumableCharacteristic == null)
            {
                return NotFound($"Характеристика с ID {id} не найдена");
            }

            return consumableCharacteristic;
        }

        /// <summary>
        /// Создать новую характеристику расходников
        /// </summary>
        /// <param name="consumableCharacteristic">Данные характеристики</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Созданная характеристика</returns>
        [HttpPost]
        public async Task<ActionResult<ConsumableCharacteristic>> CreateConsumableCharacteristic(
            ConsumableCharacteristic consumableCharacteristic,
            [FromQuery] string? token)
        {
            // Проверка прав на запись
            if (!await _roleChecker.CanWrite(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            // Валидация
            var validationResult = ValidateConsumableCharacteristic(consumableCharacteristic);
            if (validationResult != null)
            {
                return BadRequest(validationResult);
            }

            // Проверка существования типа расходного материала
            if (!await _context.ConsumableTypes.AnyAsync(ct => ct.Id == consumableCharacteristic.ConsumableTypeId))
            {
                return BadRequest($"Тип расходного материала с ID {consumableCharacteristic.ConsumableTypeId} не существует");
            }

            // Проверка уникальности сочетания (consumable_type_id, name)
            if (await _context.ConsumableCharacteristics
                .AnyAsync(cc => cc.ConsumableTypeId == consumableCharacteristic.ConsumableTypeId
                              && cc.Name == consumableCharacteristic.Name))
            {
                return BadRequest("Характеристика с таким названием уже существует для данного типа расходного материала");
            }

            _context.ConsumableCharacteristics.Add(consumableCharacteristic);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetConsumableCharacteristic),
                new { id = consumableCharacteristic.Id }, consumableCharacteristic);
        }

        /// <summary>
        /// Редактировать существующую характеристику расходников
        /// </summary>
        /// <param name="id">ID характеристики</param>
        /// <param name="consumableCharacteristic">Обновленные данные характеристики</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Результат операции</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateConsumableCharacteristic(
            [FromQuery] string? token,
            int id,
            ConsumableCharacteristic consumableCharacteristic)
        {
            // Проверка прав на запись
            if (!await _roleChecker.CanWrite(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            if (id != consumableCharacteristic.Id)
            {
                return BadRequest("ID в пути и в теле запроса не совпадают");
            }

            // Валидация
            var validationResult = ValidateConsumableCharacteristic(consumableCharacteristic);
            if (validationResult != null)
            {
                return BadRequest(validationResult);
            }

            // Проверка существования типа расходного материала
            if (!await _context.ConsumableTypes.AnyAsync(ct => ct.Id == consumableCharacteristic.ConsumableTypeId))
            {
                return BadRequest($"Тип расходного материала с ID {consumableCharacteristic.ConsumableTypeId} не существует");
            }

            // Проверка уникальности сочетания (consumable_type_id, name) исключая текущую запись
            if (await _context.ConsumableCharacteristics
                .AnyAsync(cc => cc.ConsumableTypeId == consumableCharacteristic.ConsumableTypeId
                              && cc.Name == consumableCharacteristic.Name
                              && cc.Id != id))
            {
                return BadRequest("Характеристика с таким названием уже существует для данного типа расходного материала");
            }

            _context.Entry(consumableCharacteristic).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ConsumableCharacteristicExists(id))
                {
                    return NotFound($"Характеристика с ID {id} не найдена");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        /// <summary>
        /// Проверка наличия связей характеристики расходников с другими таблицами
        /// </summary>
        /// <param name="id">ID характеристики</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>true если есть связи, false если нет</returns>
        [HttpGet("{id}/check-relations")]
        public async Task<ActionResult<bool>> CheckConsumableCharacteristicRelations(
            [FromQuery] string? token,
            int id)
        {
            // Проверка прав на чтение
            if (!await _roleChecker.CanRead(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            var characteristicExists = await _context.ConsumableCharacteristics.AnyAsync(c => c.Id == id);
            if (!characteristicExists)
            {
                return NotFound($"Характеристика с ID {id} не найдена");
            }

            // Проверяем только таблицу consumable_characteristic_values
            var hasRelations = await _context.ConsumableCharacteristicValues
                .AnyAsync(ccv => ccv.CharacteristicId == id);

            return hasRelations;
        }

        /// <summary>
        /// Удалить характеристику расходников по id
        /// </summary>
        /// <param name="id">ID характеристики</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Результат операции</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteConsumableCharacteristic(
            [FromQuery] string? token,
            int id)
        {
            // Проверка прав на запись
            if (!await _roleChecker.CanWrite(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            var consumableCharacteristic = await _context.ConsumableCharacteristics.FindAsync(id);
            if (consumableCharacteristic == null)
            {
                return NotFound($"Характеристика с ID {id} не найдена");
            }

            _context.ConsumableCharacteristics.Remove(consumableCharacteristic);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ConsumableCharacteristicExists(int id)
        {
            return _context.ConsumableCharacteristics.Any(c => c.Id == id);
        }

        private string? ValidateConsumableCharacteristic(ConsumableCharacteristic consumableCharacteristic)
        {
            // Проверка обязательных полей
            if (consumableCharacteristic.ConsumableTypeId <= 0)
            {
                return "ID типа расходного материала должен быть положительным числом";
            }

            if (string.IsNullOrWhiteSpace(consumableCharacteristic.Name))
            {
                return "Наименование характеристики обязательно для заполнения";
            }

            if (consumableCharacteristic.Name.Length > 100)
            {
                return "Наименование характеристики не может превышать 100 символов";
            }

            return null;
        }
    }
}
