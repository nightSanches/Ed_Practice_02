using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using API.Classes;
using API.Models;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SoftwareController : ControllerBase
    {
        private readonly DatabaseConnection _context;
        private readonly RoleChecker _roleChecker;

        public SoftwareController(DatabaseConnection context, RoleChecker roleChecker)
        {
            _context = context;
            _roleChecker = roleChecker;
        }

        /// <summary>
        /// Получить список всех программ
        /// </summary>
        /// <param name="token">Токен пользователя</param>
        /// <param name="search">Поиск по наименованию</param>
        /// <param name="sortBy">Поле для сортировки (id, name, developerId, version)</param>
        /// <param name="sortOrder">Порядок сортировки (asc, desc)</param>
        /// <returns>Список программного обеспечения</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Software>>> GetSoftware(
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

            IQueryable<Software> query = _context.Software;

            // Применение поиска по наименованию
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(s => s.Name.Contains(search));
            }

            // Применение сортировки
            query = sortBy?.ToLower() switch
            {
                "name" => sortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(s => s.Name)
                    : query.OrderBy(s => s.Name),
                "developerid" => sortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(s => s.DeveloperId)
                    : query.OrderBy(s => s.DeveloperId),
                "version" => sortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(s => s.Version)
                    : query.OrderBy(s => s.Version),
                _ => sortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(s => s.Id)
                    : query.OrderBy(s => s.Id)
            };

            return await query.ToListAsync();
        }

        /// <summary>
        /// Получить программу по id (для заполнения полей данными в frontend приложении)
        /// </summary>
        /// <param name="id">ID программы</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Программное обеспечение</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Software>> GetSoftware(
            [FromQuery] string? token,
            int id)
        {
            // Проверка прав на чтение
            if (!await _roleChecker.CanRead(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            var software = await _context.Software
                .Include(s => s.Developer)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (software == null)
            {
                return NotFound($"Программное обеспечение с ID {id} не найдено");
            }

            return software;
        }

        /// <summary>
        /// Создать новую программу
        /// </summary>
        /// <param name="software">Данные программного обеспечения</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Созданное программное обеспечение</returns>
        [HttpPost]
        public async Task<ActionResult<Software>> CreateSoftware(
            Software software,
            [FromQuery] string? token)
        {
            // Проверка прав на запись
            if (!await _roleChecker.CanWrite(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            // Валидация
            var validationResult = ValidateSoftware(software);
            if (validationResult != null)
            {
                return BadRequest(validationResult);
            }

            // Проверка существования разработчика
            var developerExists = await _context.Developers.AnyAsync(d => d.Id == software.DeveloperId);
            if (!developerExists)
            {
                return BadRequest($"Разработчик с ID {software.DeveloperId} не найден");
            }

            _context.Software.Add(software);
            await _context.SaveChangesAsync();

            // Загружаем связанные данные для возврата
            await _context.Entry(software)
                .Reference(s => s.Developer)
                .LoadAsync();

            return CreatedAtAction(nameof(GetSoftware), new { id = software.Id }, software);
        }

        /// <summary>
        /// Редактировать существующую программу
        /// </summary>
        /// <param name="id">ID программы</param>
        /// <param name="software">Обновленные данные программы</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Результат операции</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSoftware(
            [FromQuery] string? token,
            int id,
            Software software)
        {
            // Проверка прав на запись
            if (!await _roleChecker.CanWrite(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            if (id != software.Id)
            {
                return BadRequest("ID в пути и в теле запроса не совпадают");
            }

            // Валидация
            var validationResult = ValidateSoftware(software);
            if (validationResult != null)
            {
                return BadRequest(validationResult);
            }

            // Проверка существования разработчика
            var developerExists = await _context.Developers.AnyAsync(d => d.Id == software.DeveloperId);
            if (!developerExists)
            {
                return BadRequest($"Разработчик с ID {software.DeveloperId} не найден");
            }

            _context.Entry(software).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SoftwareExists(id))
                {
                    return NotFound($"Программное обеспечение с ID {id} не найдено");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        /// <summary>
        /// Проверка наличия связей программы с другими таблицами (только таблица equipment_software)
        /// </summary>
        /// <param name="id">ID программы</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>true если есть связи, false если нет</returns>
        [HttpGet("{id}/check-relations")]
        public async Task<ActionResult<bool>> CheckSoftwareRelations(
            [FromQuery] string? token,
            int id)
        {
            // Проверка прав на чтение
            if (!await _roleChecker.CanRead(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            var softwareExists = await _context.Software.AnyAsync(s => s.Id == id);
            if (!softwareExists)
            {
                return NotFound($"Программное обеспечение с ID {id} не найдено");
            }

            // Проверяем только таблицу equipment_software
            var hasRelations = await _context.EquipmentSoftware.AnyAsync(es => es.SoftwareId == id);

            return hasRelations;
        }

        /// <summary>
        /// Удаление программы (удаление по id)
        /// </summary>
        /// <param name="id">ID программы</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Результат операции</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSoftware(
            [FromQuery] string? token,
            int id)
        {
            // Проверка прав на запись
            if (!await _roleChecker.CanWrite(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            var software = await _context.Software.FindAsync(id);
            if (software == null)
            {
                return NotFound($"Программное обеспечение с ID {id} не найдено");
            }

            _context.Software.Remove(software);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Проверка существования программы
        /// </summary>
        private bool SoftwareExists(int id)
        {
            return _context.Software.Any(e => e.Id == id);
        }

        /// <summary>
        /// Валидация данных программного обеспечения
        /// </summary>
        private string? ValidateSoftware(Software software)
        {
            // Проверка обязательных полей
            if (string.IsNullOrWhiteSpace(software.Name))
            {
                return "Наименование программного обеспечения обязательно для заполнения";
            }

            if (software.DeveloperId <= 0)
            {
                return "ID разработчика должен быть положительным числом";
            }

            // Проверка формата версии (если указана)
            if (!string.IsNullOrWhiteSpace(software.Version))
            {
                if (!Regex.IsMatch(software.Version, @"^[a-zA-Z0-9.\-\s]+$"))
                {
                    return "Версия может содержать только буквы, цифры, точки, дефисы и пробелы";
                }
            }

            return null;
        }
    }
}
