using API.Classes;
using API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConsumableResponsibleHistoryController : ControllerBase
    {
        private readonly DatabaseConnection _context;
        private readonly RoleChecker _roleChecker;

        public ConsumableResponsibleHistoryController(DatabaseConnection context, RoleChecker roleChecker)
        {
            _context = context;
            _roleChecker = roleChecker;
        }

        /// <summary>
        /// Получить историю ответственных за расходник по consumable_id
        /// </summary>
        /// <param name="consumableId">ID расходного материала</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Список записей истории ответственных</returns>
        [HttpGet("by-consumable/{consumableId}")]
        public async Task<ActionResult<IEnumerable<ConsumableResponsibleHistory>>> GetHistoryByConsumable(
            [FromQuery] string? token,
            int consumableId)
        {
            // Проверка прав на чтение
            if (!await _roleChecker.CanRead(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            var history = await _context.ConsumableResponsibleHistory
                .Where(h => h.ConsumableId == consumableId)
                .OrderByDescending(h => h.AssignedAt)
                .ToListAsync();

            return history;
        }

        /// <summary>
        /// Создать новую запись о назначении ответственного за расходник
        /// </summary>
        /// <param name="history">Данные записи истории</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Созданная запись истории</returns>
        [HttpPost]
        public async Task<ActionResult<ConsumableResponsibleHistory>> CreateResponsibleHistory(
            ConsumableResponsibleHistory history,
            [FromQuery] string? token)
        {
            // Проверка прав на запись
            if (!await _roleChecker.CanWrite(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            // Валидация
            var validationResult = ValidateConsumableResponsibleHistory(history);
            if (validationResult != null)
            {
                return BadRequest(validationResult);
            }

            // Проверка существования расходного материала
            var consumableExists = await _context.Consumables.AnyAsync(c => c.Id == history.ConsumableId);
            if (!consumableExists)
            {
                return BadRequest($"Расходный материал с ID {history.ConsumableId} не найден");
            }

            // Проверка существования пользователя-ответственного
            var responsibleExists = await _context.Users.AnyAsync(u => u.Id == history.ResponsibleUserId);
            if (!responsibleExists)
            {
                return BadRequest($"Пользователь с ID {history.ResponsibleUserId} не найден");
            }

            // Проверка существования пользователя, который назначает (если указан)
            if (history.AssignedByUserId.HasValue)
            {
                var assignedByExists = await _context.Users.AnyAsync(u => u.Id == history.AssignedByUserId.Value);
                if (!assignedByExists)
                {
                    return BadRequest($"Пользователь, назначающий ответственного, с ID {history.AssignedByUserId.Value} не найден");
                }
            }

            // Установка даты назначения, если не указана
            if (!history.AssignedAt.HasValue)
            {
                history.AssignedAt = DateTime.Now;
            }

            _context.ConsumableResponsibleHistory.Add(history);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetHistoryByConsumable),
                new { consumableId = history.ConsumableId },
                history);
        }

        /// <summary>
        /// Удалить запись о назначении ответственного
        /// </summary>
        /// <param name="id">ID записи истории</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Результат операции</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteResponsibleHistory(
            [FromQuery] string? token,
            int id)
        {
            // Проверка прав на запись
            if (!await _roleChecker.CanWrite(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            var history = await _context.ConsumableResponsibleHistory.FindAsync(id);
            if (history == null)
            {
                return NotFound($"Запись истории с ID {id} не найдена");
            }

            _context.ConsumableResponsibleHistory.Remove(history);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Проверка валидации данных записи истории
        /// </summary>
        /// <param name="history">Запись истории для проверки</param>
        /// <returns>Сообщение об ошибке или null если ошибок нет</returns>
        private string? ValidateConsumableResponsibleHistory(ConsumableResponsibleHistory history)
        {
            // Проверка обязательных полей
            if (history.ConsumableId <= 0)
            {
                return "ID расходного материала должен быть положительным числом";
            }

            if (history.ResponsibleUserId <= 0)
            {
                return "ID ответственного пользователя должен быть положительным числом";
            }

            // Проверка AssignedByUserId если указан
            if (history.AssignedByUserId.HasValue && history.AssignedByUserId.Value <= 0)
            {
                return "ID пользователя, назначившего ответственного, должен быть положительным числом";
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
