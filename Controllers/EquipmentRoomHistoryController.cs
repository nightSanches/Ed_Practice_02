using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Classes;
using API.Models;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EquipmentRoomHistoryController : ControllerBase
    {
        private readonly DatabaseConnection _context;
        private readonly RoleChecker _roleChecker;

        public EquipmentRoomHistoryController(DatabaseConnection context, RoleChecker roleChecker)
        {
            _context = context;
            _roleChecker = roleChecker;
        }

        /// <summary>
        /// Получить историю перемещений оборудования по equipment_id
        /// </summary>
        /// <param name="token">Токен пользователя</param>
        /// <param name="equipmentId">ID оборудования</param>
        /// <returns>Список истории перемещений</returns>
        [HttpGet("by-equipment/{equipmentId}")]
        public async Task<ActionResult<IEnumerable<EquipmentRoomHistory>>> GetByEquipmentId(
            [FromQuery] string? token,
            int equipmentId)
        {
            // Проверка прав на чтение
            if (!await _roleChecker.CanRead(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            var history = await _context.EquipmentRoomHistory
                .Where(h => h.EquipmentId == equipmentId)
                .OrderByDescending(h => h.MovedAt)
                .ToListAsync();

            if (history == null || !history.Any())
            {
                return NotFound($"История перемещений для оборудования с ID {equipmentId} не найдена");
            }

            return history;
        }

        /// <summary>
        /// Создать новую запись о перемещении оборудования
        /// </summary>
        /// <param name="history">Данные перемещения</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Созданная запись истории</returns>
        [HttpPost]
        public async Task<ActionResult<EquipmentRoomHistory>> CreateEquipmentRoomHistory(
            EquipmentRoomHistory history,
            [FromQuery] string? token)
        {
            // Проверка прав на запись
            if (!await _roleChecker.CanWrite(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            // Валидация
            var validationResult = ValidateEquipmentRoomHistory(history);
            if (validationResult != null)
            {
                return BadRequest(validationResult);
            }

            // Проверка существования оборудования
            if (!await _context.Equipment.AnyAsync(e => e.Id == history.EquipmentId))
            {
                return BadRequest($"Оборудование с ID {history.EquipmentId} не существует");
            }

            // Проверка существования аудитории
            if (!await _context.Rooms.AnyAsync(r => r.Id == history.RoomId))
            {
                return BadRequest($"Аудитория с ID {history.RoomId} не существует");
            }

            // Проверка существования пользователя, если указан
            if (history.MovedByUserId.HasValue &&
                !await _context.Users.AnyAsync(u => u.Id == history.MovedByUserId.Value))
            {
                return BadRequest($"Пользователь с ID {history.MovedByUserId} не существует");
            }

            // Установка текущей даты, если не указано
            if (!history.MovedAt.HasValue)
            {
                history.MovedAt = DateTime.UtcNow;
            }

            _context.EquipmentRoomHistory.Add(history);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetByEquipmentId),
                new { equipmentId = history.EquipmentId },
                history);
        }

        /// <summary>
        /// Удалить запись истории перемещения
        /// </summary>
        /// <param name="id">ID записи истории</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Результат операции</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEquipmentRoomHistory(
            [FromQuery] string? token,
            int id)
        {
            // Проверка прав на запись
            if (!await _roleChecker.CanWrite(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            var history = await _context.EquipmentRoomHistory.FindAsync(id);
            if (history == null)
            {
                return NotFound($"Запись истории перемещения с ID {id} не найдена");
            }

            _context.EquipmentRoomHistory.Remove(history);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private string? ValidateEquipmentRoomHistory(EquipmentRoomHistory history)
        {
            // Проверка обязательных полей
            if (history.EquipmentId <= 0)
            {
                return "ID оборудования должен быть положительным числом";
            }

            if (history.RoomId <= 0)
            {
                return "ID аудитории должен быть положительным числом";
            }

            // Проверка MovedByUserId, если указан
            if (history.MovedByUserId.HasValue && history.MovedByUserId.Value <= 0)
            {
                return "ID пользователя должен быть положительным числом";
            }

            // Проверка длины комментария
            if (!string.IsNullOrWhiteSpace(history.Comment) && history.Comment.Length > 1000)
            {
                return "Комментарий не может превышать 1000 символов";
            }

            return null;
        }
    }
}
