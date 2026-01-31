using API.Classes;
using API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConsumableCharacteristicValueController : ControllerBase
    {
        private readonly DatabaseConnection _context;
        private readonly RoleChecker _roleChecker;

        public ConsumableCharacteristicValueController(DatabaseConnection context, RoleChecker roleChecker)
        {
            _context = context;
            _roleChecker = roleChecker;
        }

        /// <summary>
        /// Получить список характеристик расходного материала по consumable_id
        /// </summary>
        /// <param name="consumableId">ID расходного материала</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Список характеристик</returns>
        [HttpGet("by-consumable/{consumableId}")]
        public async Task<ActionResult<IEnumerable<ConsumableCharacteristicValue>>> GetByConsumableId(
            [FromQuery] string? token,
            int consumableId)
        {
            // Проверка прав на чтение
            if (!await _roleChecker.CanRead(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            // Проверка существования расходного материала
            var consumableExists = await _context.Consumables.AnyAsync(c => c.Id == consumableId);
            if (!consumableExists)
            {
                return NotFound($"Расходный материал с ID {consumableId} не найден");
            }

            var characteristicValues = await _context.ConsumableCharacteristicValues
                .Where(ccv => ccv.ConsumableId == consumableId)
                .ToListAsync();

            return characteristicValues;
        }

        /// <summary>
        /// Создать (прикрепить) характеристику к расходному материалу
        /// </summary>
        /// <param name="characteristicValue">Данные характеристики</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Созданная характеристика</returns>
        [HttpPost]
        public async Task<ActionResult<ConsumableCharacteristicValue>> CreateConsumableCharacteristicValue(
            ConsumableCharacteristicValue characteristicValue,
            [FromQuery] string? token)
        {
            // Проверка прав на запись
            if (!await _roleChecker.CanWrite(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            // Валидация
            var validationResult = ValidateConsumableCharacteristicValue(characteristicValue);
            if (validationResult != null)
            {
                return BadRequest(validationResult);
            }

            // Проверка существования расходного материала
            var consumableExists = await _context.Consumables.AnyAsync(c => c.Id == characteristicValue.ConsumableId);
            if (!consumableExists)
            {
                return BadRequest($"Расходный материал с ID {characteristicValue.ConsumableId} не найден");
            }

            // Проверка существования характеристики
            var characteristicExists = await _context.ConsumableCharacteristics
                .AnyAsync(cc => cc.Id == characteristicValue.CharacteristicId);
            if (!characteristicExists)
            {
                return BadRequest($"Характеристика с ID {characteristicValue.CharacteristicId} не найдена");
            }

            // Проверка уникальности связи consumable_id + characteristic_id
            var exists = await _context.ConsumableCharacteristicValues
                .AnyAsync(ccv => ccv.ConsumableId == characteristicValue.ConsumableId &&
                                ccv.CharacteristicId == characteristicValue.CharacteristicId);
            if (exists)
            {
                return BadRequest("Характеристика уже прикреплена к данному расходному материалу");
            }

            _context.ConsumableCharacteristicValues.Add(characteristicValue);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetByConsumableId),
                new { consumableId = characteristicValue.ConsumableId },
                characteristicValue);
        }

        /// <summary>
        /// Удалить прикрепление характеристики к расходному материалу
        /// </summary>
        /// <param name="id">ID записи характеристики</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Результат операции</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteConsumableCharacteristicValue(
            [FromQuery] string? token,
            int id)
        {
            // Проверка прав на запись
            if (!await _roleChecker.CanWrite(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            var characteristicValue = await _context.ConsumableCharacteristicValues.FindAsync(id);
            if (characteristicValue == null)
            {
                return NotFound($"Характеристика с ID {id} не найдена");
            }

            _context.ConsumableCharacteristicValues.Remove(characteristicValue);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private string? ValidateConsumableCharacteristicValue(ConsumableCharacteristicValue characteristicValue)
        {
            // Проверка обязательных полей
            if (characteristicValue.ConsumableId <= 0)
            {
                return "ID расходного материала должен быть положительным числом";
            }

            if (characteristicValue.CharacteristicId <= 0)
            {
                return "ID характеристики должен быть положительным числом";
            }

            // Проверка длины значения характеристики
            if (!string.IsNullOrWhiteSpace(characteristicValue.Value) &&
                characteristicValue.Value.Length > 500)
            {
                return "Значение характеристики не может превышать 500 символов";
            }

            return null;
        }
    }
}
