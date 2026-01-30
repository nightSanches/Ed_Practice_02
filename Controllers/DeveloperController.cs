using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using API.Classes;
using API.Models;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeveloperController : ControllerBase
    {
        private readonly DatabaseConnection _context;
        private readonly RoleChecker _roleChecker;

        public DeveloperController(DatabaseConnection context, RoleChecker roleChecker)
        {
            _context = context;
            _roleChecker = roleChecker;
        }

        /// <summary>
        /// Получить список всех разработчиков
        /// </summary>
        /// <param name="token">Токен пользователя</param>
        /// <param name="search">Поиск по наименованию</param>
        /// <param name="sortBy">Поле для сортировки (id, name)</param>
        /// <param name="sortOrder">Порядок сортировки (asc, desc)</param>
        /// <returns>Список разработчиков</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Developer>>> GetDevelopers(
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

            IQueryable<Developer> query = _context.Developers;

            // Применение поиска по наименованию
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(d => d.Name.Contains(search));
            }

            // Применение сортировки
            query = sortBy?.ToLower() switch
            {
                "name" => sortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(d => d.Name)
                    : query.OrderBy(d => d.Name),
                _ => sortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(d => d.Id)
                    : query.OrderBy(d => d.Id)
            };

            return await query.ToListAsync();
        }

        /// <summary>
        /// Получить разработчика по id
        /// </summary>
        /// <param name="id">ID разработчика</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Разработчик</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Developer>> GetDeveloper(
            [FromQuery] string? token,
            int id)
        {
            // Проверка прав на чтение
            if (!await _roleChecker.CanRead(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            var developer = await _context.Developers.FindAsync(id);

            if (developer == null)
            {
                return NotFound($"Разработчик с ID {id} не найден");
            }

            return developer;
        }

        /// <summary>
        /// Создать нового разработчика
        /// </summary>
        /// <param name="developer">Данные разработчика</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Созданный разработчик</returns>
        [HttpPost]
        public async Task<ActionResult<Developer>> CreateDeveloper(
            Developer developer,
            [FromQuery] string? token)
        {
            // Проверка прав на запись
            if (!await _roleChecker.CanWrite(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            // Валидация
            var validationResult = ValidateDeveloper(developer);
            if (validationResult != null)
            {
                return BadRequest(validationResult);
            }

            // Проверка уникальности наименования (регистронезависимо)
            if (await _context.Developers.AnyAsync(d => d.Name.ToLower() == developer.Name.ToLower()))
            {
                return BadRequest("Разработчик с таким наименованием уже существует");
            }

            _context.Developers.Add(developer);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDeveloper), new { id = developer.Id }, developer);
        }

        /// <summary>
        /// Редактировать существующего разработчика
        /// </summary>
        /// <param name="id">ID разработчика</param>
        /// <param name="developer">Обновленные данные разработчика</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Результат операции</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDeveloper(
            [FromQuery] string? token,
            int id,
            Developer developer)
        {
            // Проверка прав на запись
            if (!await _roleChecker.CanWrite(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            if (id != developer.Id)
            {
                return BadRequest("ID в пути и в теле запроса не совпадают");
            }

            // Валидация
            var validationResult = ValidateDeveloper(developer);
            if (validationResult != null)
            {
                return BadRequest(validationResult);
            }

            // Проверка уникальности наименования (регистронезависимо, исключая текущую запись)
            if (await _context.Developers.AnyAsync(d =>
                d.Name.ToLower() == developer.Name.ToLower() && d.Id != id))
            {
                return BadRequest("Разработчик с таким наименованием уже существует");
            }

            _context.Entry(developer).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DeveloperExists(id))
                {
                    return NotFound($"Разработчик с ID {id} не найден");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        /// <summary>
        /// Проверка наличия связей разработчика с другими таблицами
        /// </summary>
        /// <param name="id">ID разработчика</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>true если есть связи, false если нет</returns>
        [HttpGet("{id}/check-relations")]
        public async Task<ActionResult<bool>> CheckDeveloperRelations(
            [FromQuery] string? token,
            int id)
        {
            // Проверка прав на чтение
            if (!await _roleChecker.CanRead(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            var developerExists = await _context.Developers.AnyAsync(d => d.Id == id);
            if (!developerExists)
            {
                return NotFound($"Разработчик с ID {id} не найден");
            }

            // Проверяем только связь с таблицей Software
            var hasRelations = await _context.Software.AnyAsync(s => s.DeveloperId == id);

            return hasRelations;
        }

        /// <summary>
        /// Удалить разработчика по id
        /// </summary>
        /// <param name="id">ID разработчика</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Результат операции</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDeveloper(
            [FromQuery] string? token,
            int id)
        {
            // Проверка прав на запись
            if (!await _roleChecker.CanWrite(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            var developer = await _context.Developers.FindAsync(id);
            if (developer == null)
            {
                return NotFound($"Разработчик с ID {id} не найден");
            }

            _context.Developers.Remove(developer);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DeveloperExists(int id)
        {
            return _context.Developers.Any(d => d.Id == id);
        }

        private string? ValidateDeveloper(Developer developer)
        {
            // Проверка обязательных полей
            if (string.IsNullOrWhiteSpace(developer.Name))
            {
                return "Наименование разработчика обязательно для заполнения";
            }

            // Проверка длины наименования
            if (developer.Name.Length > 100)
            {
                return "Наименование не может превышать 100 символов";
            }

            // Проверка на корректные символы (только буквы, цифры, пробелы и некоторые спецсимволы)
            if (!Regex.IsMatch(developer.Name, @"^[a-zA-Zа-яА-Я0-9\s\.,\-]+$"))
            {
                return "Наименование содержит недопустимые символы";
            }

            return null;
        }
    }
}
