using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Classes;
using API.Models;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EquipmentResponsibleHistoryController : ControllerBase
    {
        private readonly DatabaseConnection _context;
        private readonly RoleChecker _roleChecker;

        public EquipmentResponsibleHistoryController(DatabaseConnection context, RoleChecker roleChecker)
        {
            _context = context;
            _roleChecker = roleChecker;
        }

        /// <summary>
        /// Получить список всех ответственных за оборудование по equipment_id
        /// </summary>
        /// <param name="equipmentId">ID оборудования</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Список истории ответственных</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EquipmentResponsibleHistory>>> GetEquipmentResponsibleHistory(
            [FromQuery] int equipmentId,
            [FromQuery] string? token)
        {
            // Проверка прав на чтение
            if (!await _roleChecker.CanRead(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            // Проверка существования оборудования
            var equipmentExists = await _context.Equipment.AnyAsync(e => e.Id == equipmentId);
            if (!equipmentExists)
            {
                return NotFound($"Оборудование с ID {equipmentId} не найдено");
            }

            var history = await _context.EquipmentResponsibleHistory
                .Where(erh => erh.EquipmentId == equipmentId)
                .OrderByDescending(h => h.AssignedAt)
                .ToListAsync();

            return history;
        }

        /// <summary>
        /// Создать (прикрепить) нового ответственного для конкретного оборудования
        /// </summary>
        /// <param name="history">Данные истории ответственного</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Созданная запись истории</returns>
        [HttpPost]
        public async Task<ActionResult<EquipmentResponsibleHistory>> CreateEquipmentResponsibleHistory(
            EquipmentResponsibleHistory history,
            [FromQuery] string? token)
        {
            // Проверка прав на запись
            if (!await _roleChecker.CanWrite(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            // Валидация
            var validationResult = ValidateEquipmentResponsibleHistory(history);
            if (validationResult != null)
            {
                return BadRequest(validationResult);
            }

            // Проверка существования оборудования
            var equipmentExists = await _context.Equipment.AnyAsync(e => e.Id == history.EquipmentId);
            if (!equipmentExists)
            {
                return NotFound($"Оборудование с ID {history.EquipmentId} не найдено");
            }

            // Проверка существования ответственного пользователя
            var userExists = await _context.Users.AnyAsync(u => u.Id == history.ResponsibleUserId);
            if (!userExists)
            {
                return NotFound($"Пользователь с ID {history.ResponsibleUserId} не найден");
            }

            // Проверка существования назначающего пользователя (если указан)
            if (history.AssignedByUserId.HasValue)
            {
                var assignedByUserExists = await _context.Users.AnyAsync(u => u.Id == history.AssignedByUserId);
                if (!assignedByUserExists)
                {
                    return NotFound($"Пользователь, назначивший ответственного, с ID {history.AssignedByUserId} не найден");
                }
            }

            // Установка даты назначения, если не указана
            if (!history.AssignedAt.HasValue)
            {
                history.AssignedAt = DateTime.Now;
            }

            _context.EquipmentResponsibleHistory.Add(history);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEquipmentResponsibleHistory),
                new { equipmentId = history.EquipmentId, token = token },
                history);
        }

        /// <summary>
        /// Удалить прикрепление ответственного к оборудованию
        /// </summary>
        /// <param name="id">ID записи истории</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Результат операции</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEquipmentResponsibleHistory(
            [FromQuery] string? token,
            int id)
        {
            // Проверка прав на запись
            if (!await _roleChecker.CanWrite(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            var history = await _context.EquipmentResponsibleHistory.FindAsync(id);
            if (history == null)
            {
                return NotFound($"Запись истории с ID {id} не найдена");
            }

            _context.EquipmentResponsibleHistory.Remove(history);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private string? ValidateEquipmentResponsibleHistory(EquipmentResponsibleHistory history)
        {
            // Проверка обязательных полей
            if (history.EquipmentId <= 0)
            {
                return "ID оборудования должно быть положительным числом";
            }

            if (history.ResponsibleUserId <= 0)
            {
                return "ID ответственного пользователя должно быть положительным числом";
            }

            // Проверка ID назначающего пользователя (если указан)
            if (history.AssignedByUserId.HasValue && history.AssignedByUserId.Value <= 0)
            {
                return "ID назначающего пользователя должно быть положительным числом";
            }

            // Проверка длины комментария
            if (!string.IsNullOrEmpty(history.Comment) && history.Comment.Length > 500)
            {
                return "Комментарий не может превышать 500 символов";
            }

            return null;
        }
    }
}
