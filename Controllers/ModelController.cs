using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Classes;
using API.Models;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ModelController : ControllerBase
    {
        private readonly DatabaseConnection _context;
        private readonly RoleChecker _roleChecker;

        public ModelController(DatabaseConnection context, RoleChecker roleChecker)
        {
            _context = context;
            _roleChecker = roleChecker;
        }

        /// <summary>
        /// Получить список всех моделей
        /// </summary>
        /// <param name="token">Токен пользователя</param>
        /// <param name="search">Поиск по наименованию</param>
        /// <param name="sortBy">Поле для сортировки (id, name, equipmentTypeId)</param>
        /// <param name="sortOrder">Порядок сортировки (asc, desc)</param>
        /// <returns>Список моделей</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Model>>> GetModels(
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

            IQueryable<Model> query = _context.Models
                .Include(m => m.EquipmentType);

            // Применение поиска по наименованию
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(m => m.Name.Contains(search));
            }

            // Применение сортировки
            query = sortBy?.ToLower() switch
            {
                "name" => sortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(m => m.Name)
                    : query.OrderBy(m => m.Name),
                "equipmenttypeid" => sortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(m => m.EquipmentTypeId)
                    : query.OrderBy(m => m.EquipmentTypeId),
                _ => sortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(m => m.Id)
                    : query.OrderBy(m => m.Id)
            };

            return await query.ToListAsync();
        }

        /// <summary>
        /// Получить модель по id
        /// </summary>
        /// <param name="id">ID модели</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Модель</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Model>> GetModel(
            [FromQuery] string? token,
            int id)
        {
            // Проверка прав на чтение
            if (!await _roleChecker.CanRead(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            var model = await _context.Models
                .Include(m => m.EquipmentType)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (model == null)
            {
                return NotFound($"Модель с ID {id} не найдена");
            }

            return model;
        }

        /// <summary>
        /// Создать новую модель
        /// </summary>
        /// <param name="model">Данные модели</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Созданная модель</returns>
        [HttpPost]
        public async Task<ActionResult<Model>> CreateModel(
            Model model,
            [FromQuery] string? token)
        {
            // Проверка прав на запись
            if (!await _roleChecker.CanWrite(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            // Валидация
            var validationResult = await ValidateModel(model);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.ErrorMessage);
            }

            // Проверка уникальности наименования модели
            if (await _context.Models.AnyAsync(m => m.Name == model.Name))
            {
                return BadRequest("Модель с таким наименованием уже существует");
            }

            _context.Models.Add(model);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetModel), new { id = model.Id }, model);
        }

        /// <summary>
        /// Редактировать существующую модель
        /// </summary>
        /// <param name="id">ID модели</param>
        /// <param name="model">Обновленные данные модели</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Результат операции</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateModel(
            [FromQuery] string? token,
            int id,
            Model model)
        {
            // Проверка прав на запись
            if (!await _roleChecker.CanWrite(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            if (id != model.Id)
            {
                return BadRequest("ID в пути и в теле запроса не совпадают");
            }

            // Валидация
            var validationResult = await ValidateModel(model);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.ErrorMessage);
            }

            // Проверка уникальности наименования модели (исключая текущую запись)
            if (await _context.Models.AnyAsync(m => m.Name == model.Name && m.Id != id))
            {
                return BadRequest("Модель с таким наименованием уже существует");
            }

            _context.Entry(model).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ModelExists(id))
                {
                    return NotFound($"Модель с ID {id} не найдена");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        /// <summary>
        /// Проверка наличия связей модели с другими таблицами (только таблица equipment)
        /// </summary>
        /// <param name="id">ID модели</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>true если есть связи, false если нет</returns>
        [HttpGet("{id}/check-relations")]
        public async Task<ActionResult<bool>> CheckModelRelations(
            [FromQuery] string? token,
            int id)
        {
            // Проверка прав на чтение
            if (!await _roleChecker.CanRead(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            var modelExists = await _context.Models.AnyAsync(m => m.Id == id);
            if (!modelExists)
            {
                return NotFound($"Модель с ID {id} не найдена");
            }

            // Проверяем только таблицу equipment (согласно требованиям)
            var hasRelations = await _context.Equipment.AnyAsync(e => e.ModelId == id);

            return hasRelations;
        }

        /// <summary>
        /// Удалить модель по id
        /// </summary>
        /// <param name="id">ID модели</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Результат операции</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteModel(
            [FromQuery] string? token,
            int id)
        {
            // Проверка прав на запись
            if (!await _roleChecker.CanWrite(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            var model = await _context.Models.FindAsync(id);
            if (model == null)
            {
                return NotFound($"Модель с ID {id} не найдена");
            }

            _context.Models.Remove(model);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ModelExists(int id)
        {
            return _context.Models.Any(e => e.Id == id);
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateModel(Model model)
        {
            // Проверка обязательных полей
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                return (false, "Наименование модели обязательно для заполнения");
            }

            if (model.Name.Length > 200)
            {
                return (false, "Наименование не может превышать 200 символов");
            }

            // Проверка существования типа оборудования
            var equipmentTypeExists = await _context.EquipmentTypes
                .AnyAsync(et => et.Id == model.EquipmentTypeId);

            if (!equipmentTypeExists)
            {
                return (false, "Указанный тип оборудования не существует");
            }

            return (true, null);
        }
    }
}
