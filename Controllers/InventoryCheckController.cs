using API.Classes;
using API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryCheckController : ControllerBase
    {
        private readonly DatabaseConnection _context;
        private readonly RoleChecker _roleChecker;

        public InventoryCheckController(DatabaseConnection context, RoleChecker roleChecker)
        {
            _context = context;
            _roleChecker = roleChecker;
        }

        /// <summary>
        /// Получить список проинвентаризированного оборудования по inventory_id
        /// </summary>
        /// <param name="token">Токен пользователя</param>
        /// <param name="inventoryId">ID инвентаризации</param>
        /// <returns>Список проверок инвентаризации</returns>
        [HttpGet("by-inventory/{inventoryId}")]
        public async Task<ActionResult<IEnumerable<InventoryCheck>>> GetInventoryChecksByInventory(
            [FromQuery] string? token,
            int inventoryId)
        {
            // Проверка прав на чтение
            if (!await _roleChecker.CanRead(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            var inventoryExists = await _context.Inventories.AnyAsync(i => i.Id == inventoryId);
            if (!inventoryExists)
            {
                return NotFound($"Инвентаризация с ID {inventoryId} не найдена");
            }

            var checks = await _context.InventoryChecks
                .Where(ic => ic.InventoryId == inventoryId)
                .OrderBy(ic => ic.Id)
                .ToListAsync();

            return checks;
        }

        /// <summary>
        /// Создать (прикрепить) проинвентаризированное оборудование к инвентаризации
        /// </summary>
        /// <param name="inventoryCheck">Данные проверки инвентаризации</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Созданная проверка инвентаризации</returns>
        [HttpPost]
        public async Task<ActionResult<InventoryCheck>> CreateInventoryCheck(
            InventoryCheck inventoryCheck,
            [FromQuery] string? token)
        {
            // Проверка прав на запись
            if (!await _roleChecker.CanRead(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            // Валидация
            var validationResult = ValidateInventoryCheck(inventoryCheck);
            if (validationResult != null)
            {
                return BadRequest(validationResult);
            }

            // Проверка существования инвентаризации
            var inventoryExists = await _context.Inventories.AnyAsync(i => i.Id == inventoryCheck.InventoryId);
            if (!inventoryExists)
            {
                return BadRequest($"Инвентаризация с ID {inventoryCheck.InventoryId} не найдена");
            }

            // Проверка существования оборудования
            var equipmentExists = await _context.Equipment.AnyAsync(e => e.Id == inventoryCheck.EquipmentId);
            if (!equipmentExists)
            {
                return BadRequest($"Оборудование с ID {inventoryCheck.EquipmentId} не найдено");
            }

            // Проверка уникальности пары inventory_id + equipment_id
            var checkExists = await _context.InventoryChecks
                .AnyAsync(ic => ic.InventoryId == inventoryCheck.InventoryId &&
                               ic.EquipmentId == inventoryCheck.EquipmentId);
            if (checkExists)
            {
                return BadRequest("Данное оборудование уже прикреплено к этой инвентаризации");
            }

            // Проверка существования пользователя (если указан)
            if (inventoryCheck.CheckedByUserId.HasValue)
            {
                var userExists = await _context.Users.AnyAsync(u => u.Id == inventoryCheck.CheckedByUserId.Value);
                if (!userExists)
                {
                    return BadRequest($"Пользователь с ID {inventoryCheck.CheckedByUserId} не найден");
                }
            }

            // Установка текущей даты, если не указана
            if (!inventoryCheck.CheckedAt.HasValue)
            {
                inventoryCheck.CheckedAt = DateTime.UtcNow;
            }

            _context.InventoryChecks.Add(inventoryCheck);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetInventoryChecksByInventory),
                new { inventoryId = inventoryCheck.InventoryId }, inventoryCheck);
        }

        /// <summary>
        /// Удалить прикрепление оборудования к инвентаризации (удаление по id)
        /// </summary>
        /// <param name="id">ID проверки инвентаризации</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Результат операции</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInventoryCheck(
            [FromQuery] string? token,
            int id)
        {
            // Проверка прав на запись
            if (!await _roleChecker.CanRead(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            var inventoryCheck = await _context.InventoryChecks.FindAsync(id);
            if (inventoryCheck == null)
            {
                return NotFound($"Проверка инвентаризации с ID {id} не найдена");
            }

            _context.InventoryChecks.Remove(inventoryCheck);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private string? ValidateInventoryCheck(InventoryCheck inventoryCheck)
        {
            // Проверка обязательных полей
            if (inventoryCheck.InventoryId <= 0)
            {
                return "ID инвентаризации должен быть положительным числом";
            }

            if (inventoryCheck.EquipmentId <= 0)
            {
                return "ID оборудования должен быть положительным числом";
            }

            // Проверка комментария (если указан)
            if (!string.IsNullOrWhiteSpace(inventoryCheck.Comment) && inventoryCheck.Comment.Length > 500)
            {
                return "Комментарий не может превышать 500 символов";
            }

            return null;
        }
    }
}
