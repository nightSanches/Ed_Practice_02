using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Classes;
using API.Models;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatusController : ControllerBase
    {
        private readonly DatabaseConnection _context;
        private readonly RoleChecker _roleChecker;

        public StatusController(DatabaseConnection context, RoleChecker roleChecker)
        {
            _context = context;
            _roleChecker = roleChecker;
        }

        /// <summary>
        /// Получить список всех статусов
        /// </summary>
        /// <param name="token">Токен пользователя</param>
        /// <param name="search">Поиск по наименованию</param>
        /// <returns>Список статусов</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Status>>> GetStatuses(
            [FromQuery] string? token,
            [FromQuery] string? search = null)
        {
            // Проверка прав на чтение
            if (!await _roleChecker.CanRead(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            IQueryable<Status> query = _context.Statuses;

            // Применение поиска по наименованию
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(s => s.Name.Contains(search));
            }

            return await query.OrderBy(s => s.Name).ToListAsync();
        }

        /// <summary>
        /// Получить статус по id
        /// </summary>
        /// <param name="id">ID статуса</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Статус</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Status>> GetStatus(
            [FromQuery] string? token,
            int id)
        {
            // Проверка прав на чтение
            if (!await _roleChecker.CanRead(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            var status = await _context.Statuses.FindAsync(id);

            if (status == null)
            {
                return NotFound($"Статус с ID {id} не найден");
            }

            return status;
        }

        /// <summary>
        /// Создать новый статус
        /// </summary>
        /// <param name="status">Данные статуса</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Созданный статус</returns>
        [HttpPost]
        public async Task<ActionResult<Status>> CreateStatus(
            Status status,
            [FromQuery] string? token)
        {
            // Проверка прав на запись
            if (!await _roleChecker.CanWrite(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            // Валидация
            var validationResult = ValidateStatus(status);
            if (validationResult != null)
            {
                return BadRequest(validationResult);
            }

            // Проверка уникальности наименования статуса
            if (await _context.Statuses.AnyAsync(s => s.Name == status.Name))
            {
                return BadRequest("Статус с таким наименованием уже существует");
            }

            _context.Statuses.Add(status);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetStatus), new { id = status.Id }, status);
        }

        /// <summary>
        /// Редактировать существующий статус
        /// </summary>
        /// <param name="id">ID статуса</param>
        /// <param name="status">Обновленные данные статуса</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Результат операции</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStatus(
            [FromQuery] string? token,
            int id,
            Status status)
        {
            // Проверка прав на запись
            if (!await _roleChecker.CanWrite(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            if (id != status.Id)
            {
                return BadRequest("ID в пути и в теле запроса не совпадают");
            }

            // Валидация
            var validationResult = ValidateStatus(status);
            if (validationResult != null)
            {
                return BadRequest(validationResult);
            }

            // Проверка уникальности наименования статуса (исключая текущую запись)
            if (await _context.Statuses.AnyAsync(s => s.Name == status.Name && s.Id != id))
            {
                return BadRequest("Статус с таким наименованием уже существует");
            }

            _context.Entry(status).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StatusExists(id))
                {
                    return NotFound($"Статус с ID {id} не найден");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        /// <summary>
        /// Проверка наличия связей статуса с другими таблицами
        /// </summary>
        /// <param name="id">ID статуса</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>true если есть связи (используется в оборудовании), false если нет</returns>
        [HttpGet("{id}/check-relations")]
        public async Task<ActionResult<bool>> CheckStatusRelations(
            [FromQuery] string? token,
            int id)
        {
            // Проверка прав на чтение
            if (!await _roleChecker.CanRead(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            var statusExists = await _context.Statuses.AnyAsync(s => s.Id == id);
            if (!statusExists)
            {
                return NotFound($"Статус с ID {id} не найден");
            }

            // Проверяем только таблицу Equipment, так как в схеме базы данных
            // статус используется только в таблице оборудования (внешний ключ status_id)
            var hasRelations = await _context.Equipment.AnyAsync(e => e.StatusId == id);

            return hasRelations;
        }

        /// <summary>
        /// Удалить статус по id
        /// </summary>
        /// <param name="id">ID статуса</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Результат операции</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStatus(
            [FromQuery] string? token,
            int id)
        {
            // Проверка прав на запись
            if (!await _roleChecker.CanWrite(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            var status = await _context.Statuses.FindAsync(id);
            if (status == null)
            {
                return NotFound($"Статус с ID {id} не найден");
            }

            _context.Statuses.Remove(status);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool StatusExists(int id)
        {
            return _context.Statuses.Any(s => s.Id == id);
        }

        private string? ValidateStatus(Status status)
        {
            // Проверка обязательных полей
            if (string.IsNullOrWhiteSpace(status.Name))
            {
                return "Наименование статуса обязательно для заполнения";
            }

            // Проверка длины наименования
            if (status.Name.Length > 100)
            {
                return "Наименование статуса не может превышать 100 символов";
            }

            return null;
        }
    }
}
