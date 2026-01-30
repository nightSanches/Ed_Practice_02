using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Classes;
using API.Models;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EquipmentSoftwareController : ControllerBase
    {
        private readonly DatabaseConnection _context;
        private readonly RoleChecker _roleChecker;

        public EquipmentSoftwareController(DatabaseConnection context, RoleChecker roleChecker)
        {
            _context = context;
            _roleChecker = roleChecker;
        }

        /// <summary>
        /// Получить список всех прикрепленных к оборудованию программ по equipment_id
        /// </summary>
        /// <param name="token">Токен пользователя</param>
        /// <param name="equipmentId">ID оборудования</param>
        /// <returns>Список прикрепленного программного обеспечения</returns>
        [HttpGet("by-equipment/{equipmentId}")]
        public async Task<ActionResult<IEnumerable<EquipmentSoftware>>> GetEquipmentSoftwareByEquipmentId(
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

            var equipmentSoftware = await _context.EquipmentSoftware
                .Where(es => es.EquipmentId == equipmentId)
                .ToListAsync();

            return equipmentSoftware;
        }

        /// <summary>
        /// Создать (прикрепить) новую программу для конкретного оборудования
        /// </summary>
        /// <param name="equipmentSoftware">Данные для прикрепления ПО к оборудованию</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Созданная запись о прикреплении ПО</returns>
        [HttpPost]
        public async Task<ActionResult<EquipmentSoftware>> CreateEquipmentSoftware(
            EquipmentSoftware equipmentSoftware,
            [FromQuery] string? token)
        {
            // Проверка прав на запись
            if (!await _roleChecker.CanWrite(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            // Валидация
            var validationResult = ValidateEquipmentSoftware(equipmentSoftware);
            if (validationResult != null)
            {
                return BadRequest(validationResult);
            }

            // Проверка существования оборудования
            var equipmentExists = await _context.Equipment.AnyAsync(e => e.Id == equipmentSoftware.EquipmentId);
            if (!equipmentExists)
            {
                return BadRequest($"Оборудование с ID {equipmentSoftware.EquipmentId} не существует");
            }

            // Проверка существования программного обеспечения
            var softwareExists = await _context.Software.AnyAsync(s => s.Id == equipmentSoftware.SoftwareId);
            if (!softwareExists)
            {
                return BadRequest($"Программное обеспечение с ID {equipmentSoftware.SoftwareId} не существует");
            }

            // Проверка уникальности связи (уникальный составной ключ)
            var exists = await _context.EquipmentSoftware
                .AnyAsync(es => es.EquipmentId == equipmentSoftware.EquipmentId
                             && es.SoftwareId == equipmentSoftware.SoftwareId);
            if (exists)
            {
                return BadRequest("Данное программное обеспечение уже прикреплено к этому оборудованию");
            }

            _context.EquipmentSoftware.Add(equipmentSoftware);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(CreateEquipmentSoftware), new { id = equipmentSoftware.Id }, equipmentSoftware);
        }

        /// <summary>
        /// Удалить прикрепление программного обеспечения к оборудованию
        /// </summary>
        /// <param name="id">ID записи о прикреплении ПО</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Результат операции</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEquipmentSoftware(
            [FromQuery] string? token,
            int id)
        {
            // Проверка прав на запись
            if (!await _roleChecker.CanWrite(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            var equipmentSoftware = await _context.EquipmentSoftware.FindAsync(id);
            if (equipmentSoftware == null)
            {
                return NotFound($"Запись о прикреплении ПО с ID {id} не найдена");
            }

            _context.EquipmentSoftware.Remove(equipmentSoftware);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private string? ValidateEquipmentSoftware(EquipmentSoftware equipmentSoftware)
        {
            // Проверка обязательных полей
            if (equipmentSoftware.EquipmentId <= 0)
            {
                return "ID оборудования должен быть положительным числом";
            }

            if (equipmentSoftware.SoftwareId <= 0)
            {
                return "ID программного обеспечения должен быть положительным числом";
            }

            return null;
        }
    }
}
