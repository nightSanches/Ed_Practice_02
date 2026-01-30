using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using API.Classes;
using API.Models;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConsumableEquipmentController : ControllerBase
    {
        private readonly DatabaseConnection _context;
        private readonly RoleChecker _roleChecker;

        public ConsumableEquipmentController(DatabaseConnection context, RoleChecker roleChecker)
        {
            _context = context;
            _roleChecker = roleChecker;
        }

        /// <summary>
        /// Получить список всех прикрепленных к оборудованию расходников по equipment_id
        /// </summary>
        /// <param name="token">Токен пользователя</param>
        /// <param name="equipmentId">ID оборудования</param>
        /// <returns>Список прикрепленных расходников</returns>
        [HttpGet("equipment/{equipmentId}")]
        public async Task<ActionResult<IEnumerable<ConsumableEquipment>>> GetConsumableEquipmentByEquipmentId(
            [FromQuery] string? token,
            int equipmentId)
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

            var consumableEquipment = await _context.ConsumableEquipment
                .Where(ce => ce.EquipmentId == equipmentId)
                .OrderByDescending(h => h.AttachedAt)
                .ToListAsync();

            return consumableEquipment;
        }

        /// <summary>
        /// Создать (прикрепить) новый расходник для конкретного оборудования
        /// </summary>
        /// <param name="consumableEquipment">Данные прикрепления расходника</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Созданное прикрепление</returns>
        [HttpPost]
        public async Task<ActionResult<ConsumableEquipment>> CreateConsumableEquipment(
            ConsumableEquipment consumableEquipment,
            [FromQuery] string? token)
        {
            // Проверка прав на запись
            if (!await _roleChecker.CanWrite(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            // Валидация
            var validationResult = ValidateConsumableEquipment(consumableEquipment);
            if (validationResult != null)
            {
                return BadRequest(validationResult);
            }

            // Проверка существования расходника
            var consumableExists = await _context.Consumables.AnyAsync(c => c.Id == consumableEquipment.ConsumableId);
            if (!consumableExists)
            {
                return BadRequest($"Расходный материал с ID {consumableEquipment.ConsumableId} не найден");
            }

            // Проверка существования оборудования
            var equipmentExists = await _context.Equipment.AnyAsync(e => e.Id == consumableEquipment.EquipmentId);
            if (!equipmentExists)
            {
                return BadRequest($"Оборудование с ID {consumableEquipment.EquipmentId} не найдено");
            }

            // Проверка уникальности привязки (чтобы один расходник не был привязан дважды к одному оборудованию)
            var existingBinding = await _context.ConsumableEquipment
                .FirstOrDefaultAsync(ce => ce.ConsumableId == consumableEquipment.ConsumableId
                                        && ce.EquipmentId == consumableEquipment.EquipmentId);
            if (existingBinding != null)
            {
                return BadRequest("Этот расходный материал уже прикреплен к данному оборудованию");
            }

            // Проверка наличия достаточного количества расходников
            var consumable = await _context.Consumables.FindAsync(consumableEquipment.ConsumableId);
            if (consumable != null && consumable.Quantity < consumableEquipment.QuantityUsed)
            {
                return BadRequest($"Недостаточно расходного материала. Доступно: {consumable.Quantity}, требуется: {consumableEquipment.QuantityUsed}");
            }

            // Установка даты прикрепления
            consumableEquipment.AttachedAt = DateTime.UtcNow;

            _context.ConsumableEquipment.Add(consumableEquipment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetConsumableEquipmentByEquipmentId),
                new { equipmentId = consumableEquipment.EquipmentId },
                consumableEquipment);
        }

        /// <summary>
        /// Удалить прикрепление расходника к оборудованию
        /// </summary>
        /// <param name="id">ID прикрепления</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Результат операции</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteConsumableEquipment(
            [FromQuery] string? token,
            int id)
        {
            // Проверка прав на запись
            if (!await _roleChecker.CanWrite(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            var consumableEquipment = await _context.ConsumableEquipment.FindAsync(id);
            if (consumableEquipment == null)
            {
                return NotFound($"Прикрепление расходника к оборудованию с ID {id} не найдено");
            }

            _context.ConsumableEquipment.Remove(consumableEquipment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private string? ValidateConsumableEquipment(ConsumableEquipment consumableEquipment)
        {
            // Проверка обязательных полей
            if (consumableEquipment.ConsumableId <= 0)
            {
                return "ID расходного материала должен быть положительным числом";
            }

            if (consumableEquipment.EquipmentId <= 0)
            {
                return "ID оборудования должен быть положительным числом";
            }

            if (consumableEquipment.QuantityUsed <= 0)
            {
                return "Количество использованных единиц должно быть положительным числом";
            }

            // Проверка формата ID (только цифры)
            if (!Regex.IsMatch(consumableEquipment.ConsumableId.ToString(), @"^\d+$"))
            {
                return "ID расходного материала должен содержать только цифры";
            }

            if (!Regex.IsMatch(consumableEquipment.EquipmentId.ToString(), @"^\d+$"))
            {
                return "ID оборудования должен содержать только цифры";
            }

            if (!Regex.IsMatch(consumableEquipment.QuantityUsed.ToString(), @"^\d+$"))
            {
                return "Количество использованных единиц должно содержать только цифры";
            }

            // Проверка AttachedByUserId если указан
            if (consumableEquipment.AttachedByUserId.HasValue && consumableEquipment.AttachedByUserId.Value <= 0)
            {
                return "ID пользователя должен быть положительным числом";
            }

            return null;
        }
    }
}
