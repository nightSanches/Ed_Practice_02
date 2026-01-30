using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Classes;
using API.Models;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomController : ControllerBase
    {
        private readonly DatabaseConnection _context;
        private readonly RoleChecker _roleChecker;

        public RoomController(DatabaseConnection context, RoleChecker roleChecker)
        {
            _context = context;
            _roleChecker = roleChecker;
        }

        /// <summary>
        /// Получить список всех аудиторий
        /// </summary>
        /// <param name="token">Токен пользователя</param>
        /// <param name="search">Поиск по наименованию</param>
        /// <param name="sortBy">Поле для сортировки (id, name, shortName)</param>
        /// <param name="sortOrder">Порядок сортировки (asc, desc)</param>
        /// <returns>Список аудиторий</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Room>>> GetRooms(
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

            IQueryable<Room> query = _context.Rooms;

            // Применение поиска по наименованию
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(r => r.Name.Contains(search));
            }

            // Применение сортировки
            query = sortBy?.ToLower() switch
            {
                "name" => sortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(r => r.Name)
                    : query.OrderBy(r => r.Name),
                "shortname" => sortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(r => r.ShortName)
                    : query.OrderBy(r => r.ShortName),
                _ => sortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(r => r.Id)
                    : query.OrderBy(r => r.Id)
            };

            return await query.ToListAsync();
        }

        /// <summary>
        /// Получить аудиторию по id
        /// </summary>
        /// <param name="id">ID аудитории</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Аудитория</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Room>> GetRoom(
            int id,
            [FromQuery] string? token)
        {
            // Проверка прав на чтение
            if (!await _roleChecker.CanRead(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            var room = await _context.Rooms.FindAsync(id);

            if (room == null)
            {
                return NotFound($"Аудитория с ID {id} не найдена");
            }

            return room;
        }

        /// <summary>
        /// Создать новую аудиторию
        /// </summary>
        /// <param name="room">Данные аудитории</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Созданная аудитория</returns>
        [HttpPost]
        public async Task<ActionResult<Room>> CreateRoom(
            Room room,
            [FromQuery] string? token)
        {
            // Проверка прав на запись
            if (!await _roleChecker.CanWrite(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            // Валидация
            var validationResult = ValidateRoom(room);
            if (validationResult != null)
            {
                return BadRequest(validationResult);
            }

            // Проверка уникальности наименования (опционально, если требуется уникальность)
            if (await _context.Rooms.AnyAsync(r => r.Name == room.Name))
            {
                return BadRequest("Аудитория с таким наименованием уже существует");
            }

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRoom), new { id = room.Id }, room);
        }

        /// <summary>
        /// Редактировать существующую аудиторию
        /// </summary>
        /// <param name="id">ID аудитории</param>
        /// <param name="room">Обновленные данные аудитории</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Результат операции</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRoom(
            int id,
            Room room,
            [FromQuery] string? token)
        {
            // Проверка прав на запись
            if (!await _roleChecker.CanWrite(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            if (id != room.Id)
            {
                return BadRequest("ID в пути и в теле запроса не совпадают");
            }

            // Валидация
            var validationResult = ValidateRoom(room);
            if (validationResult != null)
            {
                return BadRequest(validationResult);
            }

            // Проверка уникальности наименования (исключая текущую запись)
            if (await _context.Rooms.AnyAsync(r => r.Name == room.Name && r.Id != id))
            {
                return BadRequest("Аудитория с таким наименованием уже существует");
            }

            _context.Entry(room).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RoomExists(id))
                {
                    return NotFound($"Аудитория с ID {id} не найдена");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        /// <summary>
        /// Проверка наличия связей аудитории с другими таблицами
        /// </summary>
        /// <param name="id">ID аудитории</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>true если есть связи, false если нет</returns>
        [HttpGet("{id}/check-relations")]
        public async Task<ActionResult<bool>> CheckRoomRelations(
            int id,
            [FromQuery] string? token)
        {
            // Проверка прав на чтение
            if (!await _roleChecker.CanRead(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            var roomExists = await _context.Rooms.AnyAsync(r => r.Id == id);
            if (!roomExists)
            {
                return NotFound($"Аудитория с ID {id} не найдена");
            }

            var hasRelations =
                await _context.Equipment.AnyAsync(e => e.RoomId == id) ||
                await _context.EquipmentRoomHistory.AnyAsync(erh => erh.RoomId == id);

            return hasRelations;
        }

        /// <summary>
        /// Удалить аудиторию по id
        /// </summary>
        /// <param name="id">ID аудитории</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Результат операции</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoom(
            int id,
            [FromQuery] string? token)
        {
            // Проверка прав на запись
            if (!await _roleChecker.CanWrite(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
            {
                return NotFound($"Аудитория с ID {id} не найдена");
            }

            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool RoomExists(int id)
        {
            return _context.Rooms.Any(e => e.Id == id);
        }

        private async Task<bool> CheckRoomRelationsInternal(int roomId)
        {
            return await _context.Equipment.AnyAsync(e => e.RoomId == roomId) ||
                   await _context.EquipmentRoomHistory.AnyAsync(erh => erh.RoomId == roomId);
        }

        private string? ValidateRoom(Room room)
        {
            // Проверка обязательных полей
            if (string.IsNullOrWhiteSpace(room.Name))
            {
                return "Наименование аудитории обязательно для заполнения";
            }

            // Проверка длины полей
            if (room.Name.Length > 100)
            {
                return "Наименование не может превышать 100 символов";
            }

            if (!string.IsNullOrWhiteSpace(room.ShortName) && room.ShortName.Length > 20)
            {
                return "Сокращенное наименование не может превышать 20 символов";
            }

            return null;
        }
    }
}
