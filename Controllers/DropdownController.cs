using Microsoft.AspNetCore.Mvc;
using API.Classes;
using API.Models.Dropdowns;

namespace API.Controllers
{
    /// <summary>
    /// Контроллер для получения данных для выпадающих списков
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DropdownController : ControllerBase
    {
        private readonly DatabaseConnection _context;

        /// <summary>
        /// Конструктор контроллера выпадающих списков
        /// </summary>
        /// <param name="context">Контекст базы данных</param>
        public DropdownController(DatabaseConnection context)
        {
            _context = context;
        }

        /// <summary>
        /// Получение списка типов расходных материалов
        /// </summary>
        /// <returns>Список id и name из consumable_types</returns>
        /// <response code="200">Успешно возвращен список</response>
        [HttpGet("consumable-types")]
        [ProducesResponseType(typeof(List<DropdownItem>), StatusCodes.Status200OK)]
        public IActionResult GetConsumableTypes()
        {
            try
            {
                var items = _context.ConsumableTypes
                    .Select(ct => new DropdownItem
                    {
                        Id = ct.Id,
                        DisplayText = ct.Name
                    })
                    .OrderBy(ct => ct.DisplayText)
                    .ToList();

                return Ok(items);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Ошибка при получении списка типов расходных материалов", error = ex.Message });
            }
        }

        /// <summary>
        /// Получение списка пользователей в формате "И.О. Фамилия"
        /// </summary>
        /// <returns>Список id и fullname пользователей</returns>
        /// <response code="200">Успешно возвращен список</response>
        [HttpGet("users")]
        [ProducesResponseType(typeof(List<DropdownItem>), StatusCodes.Status200OK)]
        public IActionResult GetUsers()
        {
            try
            {
                var users = _context.Users
                    .Select(u => new
                    {
                        u.Id,
                        u.FirstName,
                        u.MiddleName,
                        u.LastName
                    })
                    .ToList();

                var items = users
                    .Select(u => new DropdownItem
                    {
                        Id = u.Id,
                        DisplayText = $"{GetInitial(u.FirstName)}.{GetInitial(u.MiddleName)}. {u.LastName}"
                    })
                    .OrderBy(u => u.DisplayText)
                    .ToList();

                return Ok(items);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Ошибка при получении списка пользователей", error = ex.Message });
            }
        }

        /// <summary>
        /// Получение списка расходных материалов
        /// </summary>
        /// <returns>Список id и name из consumables</returns>
        /// <response code="200">Успешно возвращен список</response>
        [HttpGet("consumables")]
        [ProducesResponseType(typeof(List<DropdownItem>), StatusCodes.Status200OK)]
        public IActionResult GetConsumables()
        {
            try
            {
                var items = _context.Consumables
                    .Select(c => new DropdownItem
                    {
                        Id = c.Id,
                        DisplayText = c.Name
                    })
                    .OrderBy(c => c.DisplayText)
                    .ToList();

                return Ok(items);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Ошибка при получении списка расходных материалов", error = ex.Message });
            }
        }

        /// <summary>
        /// Получение списка характеристик расходных материалов
        /// </summary>
        /// <returns>Список id и name из consumable_characteristics</returns>
        /// <response code="200">Успешно возвращен список</response>
        [HttpGet("consumable-characteristics")]
        [ProducesResponseType(typeof(List<DropdownItem>), StatusCodes.Status200OK)]
        public IActionResult GetConsumableCharacteristics()
        {
            try
            {
                var items = _context.ConsumableCharacteristics
                    .Select(cc => new DropdownItem
                    {
                        Id = cc.Id,
                        DisplayText = cc.Name
                    })
                    .OrderBy(cc => cc.DisplayText)
                    .ToList();

                return Ok(items);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Ошибка при получении списка характеристик расходных материалов", error = ex.Message });
            }
        }

        /// <summary>
        /// Получение списка оборудования
        /// </summary>
        /// <returns>Список id и строка "name (inventory_number)" из equipment</returns>
        /// <response code="200">Успешно возвращен список</response>
        [HttpGet("equipment")]
        [ProducesResponseType(typeof(List<DropdownItem>), StatusCodes.Status200OK)]
        public IActionResult GetEquipment()
        {
            try
            {
                // Сначала получаем данные из базы, затем формируем строку в памяти
                var equipment = _context.Equipment
                    .Select(e => new
                    {
                        e.Id,
                        e.Name,
                        e.InventoryNumber
                    })
                    .ToList(); // Выполняем запрос и получаем данные в память

                var items = equipment
                    .Select(e => new DropdownItem
                    {
                        Id = e.Id,
                        DisplayText = $"{e.Name} ({e.InventoryNumber})"
                    })
                    .OrderBy(e => e.DisplayText)
                    .ToList();

                return Ok(items);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Ошибка при получении списка оборудования", error = ex.Message });
            }
        }

        /// <summary>
        /// Получение списка аудиторий
        /// </summary>
        /// <returns>Список id и short_name из rooms</returns>
        /// <response code="200">Успешно возвращен список</response>
        [HttpGet("rooms")]
        [ProducesResponseType(typeof(List<DropdownItem>), StatusCodes.Status200OK)]
        public IActionResult GetRooms()
        {
            try
            {
                var items = _context.Rooms
                    .Where(r => !string.IsNullOrEmpty(r.ShortName))
                    .Select(r => new DropdownItem
                    {
                        Id = r.Id,
                        DisplayText = r.ShortName
                    })
                    .OrderBy(r => r.DisplayText)
                    .ToList();

                return Ok(items);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Ошибка при получении списка аудиторий", error = ex.Message });
            }
        }

        /// <summary>
        /// Получение списка программного обеспечения
        /// </summary>
        /// <returns>Список id и name из software</returns>
        /// <response code="200">Успешно возвращен список</response>
        [HttpGet("software")]
        [ProducesResponseType(typeof(List<DropdownItem>), StatusCodes.Status200OK)]
        public IActionResult GetSoftware()
        {
            try
            {
                var items = _context.Software
                    .Select(s => new DropdownItem
                    {
                        Id = s.Id,
                        DisplayText = s.Name
                    })
                    .OrderBy(s => s.DisplayText)
                    .ToList();

                return Ok(items);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Ошибка при получении списка программного обеспечения", error = ex.Message });
            }
        }

        /// <summary>
        /// Получение списка инвентаризаций
        /// </summary>
        /// <returns>Список id и строка "name (start_date)" из inventories</returns>
        /// <response code="200">Успешно возвращен список</response>
        [HttpGet("inventories")]
        [ProducesResponseType(typeof(List<DropdownItem>), StatusCodes.Status200OK)]
        public IActionResult GetInventories()
        {
            try
            {
                var items = _context.Inventories
                    .Select(i => new DropdownItem
                    {
                        Id = i.Id,
                        DisplayText = $"{i.Name} ({i.StartDate:dd.MM.yyyy})"
                    })
                    .OrderByDescending(i => i.Id)
                    .ToList();

                return Ok(items);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Ошибка при получении списка инвентаризаций", error = ex.Message });
            }
        }

        /// <summary>
        /// Получение списка типов оборудования
        /// </summary>
        /// <returns>Список id и name из equipment_types</returns>
        /// <response code="200">Успешно возвращен список</response>
        [HttpGet("equipment-types")]
        [ProducesResponseType(typeof(List<DropdownItem>), StatusCodes.Status200OK)]
        public IActionResult GetEquipmentTypes()
        {
            try
            {
                var items = _context.EquipmentTypes
                    .Select(et => new DropdownItem
                    {
                        Id = et.Id,
                        DisplayText = et.Name
                    })
                    .OrderBy(et => et.DisplayText)
                    .ToList();

                return Ok(items);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Ошибка при получении списка типов оборудования", error = ex.Message });
            }
        }

        /// <summary>
        /// Получение списка разработчиков
        /// </summary>
        /// <returns>Список id и name из developers</returns>
        /// <response code="200">Успешно возвращен список</response>
        [HttpGet("developers")]
        [ProducesResponseType(typeof(List<DropdownItem>), StatusCodes.Status200OK)]
        public IActionResult GetDevelopers()
        {
            try
            {
                var items = _context.Developers
                    .Select(d => new DropdownItem
                    {
                        Id = d.Id,
                        DisplayText = d.Name
                    })
                    .OrderBy(d => d.DisplayText)
                    .ToList();

                return Ok(items);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Ошибка при получении списка разработчиков", error = ex.Message });
            }
        }

        /// <summary>
        /// Получение списка статусов оборудования
        /// </summary>
        /// <returns>Список id и name из statuses</returns>
        /// <response code="200">Успешно возвращен список</response>
        [HttpGet("statuses")]
        [ProducesResponseType(typeof(List<DropdownItem>), StatusCodes.Status200OK)]
        public IActionResult GetStatuses()
        {
            try
            {
                var items = _context.Statuses
                    .Select(s => new DropdownItem
                    {
                        Id = s.Id,
                        DisplayText = s.Name
                    })
                    .OrderBy(s => s.DisplayText)
                    .ToList();

                return Ok(items);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Ошибка при получении списка статусов", error = ex.Message });
            }
        }

        /// <summary>
        /// Получение списка направлений
        /// </summary>
        /// <returns>Список id и name из directions</returns>
        /// <response code="200">Успешно возвращен список</response>
        [HttpGet("directions")]
        [ProducesResponseType(typeof(List<DropdownItem>), StatusCodes.Status200OK)]
        public IActionResult GetDirections()
        {
            try
            {
                var items = _context.Directions
                    .Select(d => new DropdownItem
                    {
                        Id = d.Id,
                        DisplayText = d.Name
                    })
                    .OrderBy(d => d.DisplayText)
                    .ToList();

                return Ok(items);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Ошибка при получении списка направлений", error = ex.Message });
            }
        }

        /// <summary>
        /// Получение списка моделей оборудования
        /// </summary>
        /// <returns>Список id и name из models</returns>
        /// <response code="200">Успешно возвращен список</response>
        [HttpGet("models")]
        [ProducesResponseType(typeof(List<DropdownItem>), StatusCodes.Status200OK)]
        public IActionResult GetModels()
        {
            try
            {
                var items = _context.Models
                    .Select(m => new DropdownItem
                    {
                        Id = m.Id,
                        DisplayText = m.Name
                    })
                    .OrderBy(m => m.DisplayText)
                    .ToList();

                return Ok(items);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Ошибка при получении списка моделей", error = ex.Message });
            }
        }

        /// <summary>
        /// Получение инициала строки
        /// </summary>
        /// <param name="str">Строка</param>
        /// <returns>Первый символ строки или пустую строку если строка null или пустая</returns>
        private string GetInitial(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return string.Empty;

            return str.Substring(0, 1);
        }
    }
}
