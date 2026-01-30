using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Classes;
using API.Models;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DirectionController : ControllerBase
    {
        private readonly DatabaseConnection _context;
        private readonly RoleChecker _roleChecker;

        public DirectionController(DatabaseConnection context, RoleChecker roleChecker)
        {
            _context = context;
            _roleChecker = roleChecker;
        }

        /// <summary>
        /// Получить список всех направлений
        /// </summary>
        /// <param name="token">Токен пользователя</param>
        /// <param name="search">Поиск по наименованию</param>
        /// <returns>Список направлений</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Direction>>> GetDirections(
            [FromQuery] string? token,
            [FromQuery] string? search = null)
        {
            // Проверка прав на чтение
            if (!await _roleChecker.CanRead(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            IQueryable<Direction> query = _context.Directions;

            // Применение поиска по наименованию
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(d => d.Name.Contains(search));
            }

            // Сортировка по ID (по умолчанию)
            query = query.OrderBy(d => d.Id);

            return await query.ToListAsync();
        }

        /// <summary>
        /// Получить направление по id
        /// </summary>
        /// <param name="id">ID направления</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Направление</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Direction>> GetDirection(
            [FromQuery] string? token,
            int id)
        {
            // Проверка прав на чтение
            if (!await _roleChecker.CanRead(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            var direction = await _context.Directions.FindAsync(id);

            if (direction == null)
            {
                return NotFound($"Направление с ID {id} не найдено");
            }

            return direction;
        }

        /// <summary>
        /// Создать новое направление
        /// </summary>
        /// <param name="direction">Данные направления</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Созданное направление</returns>
        [HttpPost]
        public async Task<ActionResult<Direction>> CreateDirection(
            Direction direction,
            [FromQuery] string? token)
        {
            // Проверка прав на запись
            if (!await _roleChecker.CanWrite(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            // Валидация
            var validationResult = ValidateDirection(direction);
            if (validationResult != null)
            {
                return BadRequest(validationResult);
            }

            // Проверка уникальности наименования
            if (await _context.Directions.AnyAsync(d => d.Name == direction.Name))
            {
                return BadRequest("Направление с таким наименованием уже существует");
            }

            _context.Directions.Add(direction);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDirection), new { id = direction.Id }, direction);
        }

        /// <summary>
        /// Редактировать существующее направление
        /// </summary>
        /// <param name="id">ID направления</param>
        /// <param name="direction">Обновленные данные направления</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Результат операции</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDirection(
            [FromQuery] string? token,
            int id,
            Direction direction)
        {
            // Проверка прав на запись
            if (!await _roleChecker.CanWrite(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            if (id != direction.Id)
            {
                return BadRequest("ID в пути и в теле запроса не совпадают");
            }

            // Валидация
            var validationResult = ValidateDirection(direction);
            if (validationResult != null)
            {
                return BadRequest(validationResult);
            }

            // Проверка уникальности наименования (исключая текущую запись)
            if (await _context.Directions.AnyAsync(d => d.Name == direction.Name && d.Id != id))
            {
                return BadRequest("Направление с таким наименованием уже существует");
            }

            _context.Entry(direction).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DirectionExists(id))
                {
                    return NotFound($"Направление с ID {id} не найдено");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        /// <summary>
        /// Проверка наличия связей направления с другими таблицами (только таблица equipment)
        /// </summary>
        /// <param name="id">ID направления</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>true если есть связи, false если нет</returns>
        [HttpGet("{id}/check-relations")]
        public async Task<ActionResult<bool>> CheckDirectionRelations(
            [FromQuery] string? token,
            int id)
        {
            // Проверка прав на чтение
            if (!await _roleChecker.CanRead(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            var directionExists = await _context.Directions.AnyAsync(d => d.Id == id);
            if (!directionExists)
            {
                return NotFound($"Направление с ID {id} не найдено");
            }

            // Проверяем только таблицу Equipment, так как DirectionId ссылается только на Directions
            var hasRelations = await _context.Equipment.AnyAsync(e => e.DirectionId == id);

            return hasRelations;
        }

        /// <summary>
        /// Удалить направление по id
        /// </summary>
        /// <param name="id">ID направления</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Результат операции</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDirection(
            [FromQuery] string? token,
            int id)
        {
            // Проверка прав на запись
            if (!await _roleChecker.CanWrite(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            var direction = await _context.Directions.FindAsync(id);
            if (direction == null)
            {
                return NotFound($"Направление с ID {id} не найдено");
            }

            _context.Directions.Remove(direction);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool DirectionExists(int id)
        {
            return _context.Directions.Any(e => e.Id == id);
        }

        private string? ValidateDirection(Direction direction)
        {
            // Проверка обязательных полей
            if (string.IsNullOrWhiteSpace(direction.Name))
            {
                return "Наименование направления обязательно для заполнения";
            }

            // Проверка длины наименования
            if (direction.Name.Length > 100)
            {
                return "Наименование не может превышать 100 символов";
            }

            return null;
        }
    }
}
