using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Classes;
using API.Models;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EquipmentTypeController : ControllerBase
    {
        private readonly DatabaseConnection _context;
        private readonly RoleChecker _roleChecker;

        public EquipmentTypeController(DatabaseConnection context, RoleChecker roleChecker)
        {
            _context = context;
            _roleChecker = roleChecker;
        }

        /// <summary>
        /// Получить список всех типов оборудования
        /// </summary>
        /// <param name="token">Токен пользователя</param>
        /// <param name="search">Поиск по наименованию</param>
        /// <returns>Список типов оборудования</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EquipmentType>>> GetEquipmentTypes(
            [FromQuery] string? token,
            [FromQuery] string? search = null)
        {
            // Проверка прав на чтение
            if (!await _roleChecker.CanRead(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            IQueryable<EquipmentType> query = _context.EquipmentTypes;

            // Применение поиска по наименованию
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(e => e.Name.Contains(search));
            }

            // Сортировка по ID по умолчанию
            query = query.OrderBy(e => e.Id);

            return await query.ToListAsync();
        }

        /// <summary>
        /// Получить тип оборудования по id
        /// </summary>
        /// <param name="id">ID типа оборудования</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Тип оборудования</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<EquipmentType>> GetEquipmentType(
            [FromQuery] string? token,
            int id)
        {
            // Проверка прав на чтение
            if (!await _roleChecker.CanRead(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            var equipmentType = await _context.EquipmentTypes.FindAsync(id);

            if (equipmentType == null)
            {
                return NotFound($"Тип оборудования с ID {id} не найден");
            }

            return equipmentType;
        }

        /// <summary>
        /// Создать новый тип оборудования
        /// </summary>
        /// <param name="equipmentType">Данные типа оборудования</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Созданный тип оборудования</returns>
        [HttpPost]
        public async Task<ActionResult<EquipmentType>> CreateEquipmentType(
            EquipmentType equipmentType,
            [FromQuery] string? token)
        {
            // Проверка прав на запись
            if (!await _roleChecker.CanWrite(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            // Валидация
            var validationResult = ValidateEquipmentType(equipmentType);
            if (validationResult != null)
            {
                return BadRequest(validationResult);
            }

            // Проверка уникальности наименования
            if (await _context.EquipmentTypes.AnyAsync(e => e.Name == equipmentType.Name))
            {
                return BadRequest("Наименование типа оборудования должно быть уникальным");
            }

            _context.EquipmentTypes.Add(equipmentType);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEquipmentType), new { id = equipmentType.Id }, equipmentType);
        }

        /// <summary>
        /// Редактировать существующий тип оборудования
        /// </summary>
        /// <param name="id">ID типа оборудования</param>
        /// <param name="equipmentType">Обновленные данные типа оборудования</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Результат операции</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEquipmentType(
            [FromQuery] string? token,
            int id,
            EquipmentType equipmentType)
        {
            // Проверка прав на запись
            if (!await _roleChecker.CanWrite(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            if (id != equipmentType.Id)
            {
                return BadRequest("ID в пути и в теле запроса не совпадают");
            }

            // Валидация
            var validationResult = ValidateEquipmentType(equipmentType);
            if (validationResult != null)
            {
                return BadRequest(validationResult);
            }

            // Проверка уникальности наименования (исключая текущую запись)
            if (await _context.EquipmentTypes.AnyAsync(e => e.Name == equipmentType.Name && e.Id != id))
            {
                return BadRequest("Наименование типа оборудования должно быть уникальным");
            }

            _context.Entry(equipmentType).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EquipmentTypeExists(id))
                {
                    return NotFound($"Тип оборудования с ID {id} не найден");
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
        /// <param name="id">ID типа оборудования</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>true если есть связи, false если нет</returns>
        [HttpGet("{id}/check-relations")]
        public async Task<ActionResult<bool>> CheckEquipmentTypeRelations(
            [FromQuery] string? token,
            int id)
        {
            // Проверка прав на чтение
            if (!await _roleChecker.CanRead(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            var equipmentTypeExists = await _context.EquipmentTypes.AnyAsync(e => e.Id == id);
            if (!equipmentTypeExists)
            {
                return NotFound($"Тип оборудования с ID {id} не найден");
            }

            // Проверка связей только с таблицей Models
            var hasRelations = await _context.Models.AnyAsync(m => m.EquipmentTypeId == id);

            return hasRelations;
        }

        /// <summary>
        /// Удалить тип оборудования по id
        /// </summary>
        /// <param name="id">ID типа оборудования</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Результат операции</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEquipmentType(
            [FromQuery] string? token,
            int id)
        {
            // Проверка прав на запись
            if (!await _roleChecker.CanWrite(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            var equipmentType = await _context.EquipmentTypes.FindAsync(id);
            if (equipmentType == null)
            {
                return NotFound($"Тип оборудования с ID {id} не найден");
            }

            _context.EquipmentTypes.Remove(equipmentType);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EquipmentTypeExists(int id)
        {
            return _context.EquipmentTypes.Any(e => e.Id == id);
        }

        private string? ValidateEquipmentType(EquipmentType equipmentType)
        {
            // Проверка обязательных полей
            if (string.IsNullOrWhiteSpace(equipmentType.Name))
            {
                return "Наименование типа оборудования обязательно для заполнения";
            }

            // Проверка длины наименования
            if (equipmentType.Name.Length > 100)
            {
                return "Наименование не может превышать 100 символов";
            }

            return null;
        }
    }
}
