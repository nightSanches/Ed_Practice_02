using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using API.Classes;
using API.Models;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly DatabaseConnection _context;
        private readonly RoleChecker _roleChecker;

        public UsersController(DatabaseConnection context, RoleChecker roleChecker)
        {
            _context = context;
            _roleChecker = roleChecker;
        }

        /// <summary>
        /// Получить список всех пользователей
        /// </summary>
        /// <param name="token">Токен пользователя</param>
        /// <param name="search">Поиск по фамилии, имени, логину или email</param>
        /// <param name="sortBy">Поле для сортировки (id, username, lastName, firstName, role)</param>
        /// <param name="sortOrder">Порядок сортировки (asc, desc)</param>
        /// <param name="roleFilter">Фильтр по роли</param>
        /// <returns>Список пользователей</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers(
            [FromQuery] string? token,
            [FromQuery] string? search = null,
            [FromQuery] string? sortBy = "id",
            [FromQuery] string? sortOrder = "asc",
            [FromQuery] string? roleFilter = null)
        {
            // Проверка прав на чтение
            if (!await _roleChecker.CanRead(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            IQueryable<User> query = _context.Users;

            // Применение поиска
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(u =>
                    u.LastName.Contains(search) ||
                    u.FirstName.Contains(search) ||
                    u.Username.Contains(search) ||
                    (u.Email != null && u.Email.Contains(search)));
            }

            // Применение фильтра по роли
            if (!string.IsNullOrWhiteSpace(roleFilter))
            {
                query = query.Where(u => u.Role == roleFilter);
            }

            // Применение сортировки
            query = sortBy?.ToLower() switch
            {
                "username" => sortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(u => u.Username)
                    : query.OrderBy(u => u.Username),
                "lastname" => sortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(u => u.LastName)
                    : query.OrderBy(u => u.LastName),
                "firstname" => sortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(u => u.FirstName)
                    : query.OrderBy(u => u.FirstName),
                "role" => sortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(u => u.Role)
                    : query.OrderBy(u => u.Role),
                _ => sortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(u => u.Id)
                    : query.OrderBy(u => u.Id)
            };

            var users = await query.Select(u => new User
            {
                Id = u.Id,
                Username = u.Username,
                Password = u.Password,
                Role = u.Role,
                Token = u.Token,
                Email = u.Email,
                LastName = u.LastName,
                FirstName = u.FirstName,
                MiddleName = u.MiddleName,
                Phone = u.Phone,
                Address = u.Address
            }).ToListAsync();

            return users;
        }

        /// <summary>
        /// Получить пользователя по id
        /// </summary>
        /// <param name="id">ID пользователя</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Пользователь</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(
            [FromQuery] string? token,
            int id)
        {
            // Проверка прав на чтение
            if (!await _roleChecker.CanRead(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound($"Пользователь с ID {id} не найден");
            }

            return user;
        }

        /// <summary>
        /// Создать нового пользователя
        /// </summary>
        /// <param name="user">Данные пользователя</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Созданный пользователь</returns>
        [HttpPost]
        public async Task<ActionResult<User>> CreateUser(
            User user,
            [FromQuery] string? token)
        {
            // Проверка прав на запись
            if (!await _roleChecker.CanWrite(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            // Валидация модели
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Дополнительная валидация
            var validationResult = ValidateUser(user);
            if (validationResult != null)
            {
                return BadRequest(validationResult);
            }

            // Проверка уникальности логина
            if (await _context.Users.AnyAsync(u => u.Username == user.Username))
            {
                return BadRequest("Пользователь с таким логином уже существует");
            }

            // Проверка уникальности email (если указан)
            if (!string.IsNullOrWhiteSpace(user.Email) &&
                await _context.Users.AnyAsync(u => u.Email == user.Email))
            {
                return BadRequest("Пользователь с таким email уже существует");
            }

            user.Token = Guid.NewGuid().ToString();

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        /// <summary>
        /// Редактировать существующего пользователя
        /// </summary>
        /// <param name="id">ID пользователя</param>
        /// <param name="user">Обновленные данные пользователя</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Результат операции</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(
            [FromQuery] string? token,
            int id,
            User user)
        {
            // Проверка прав на запись
            if (!await _roleChecker.CanWrite(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            // Валидация модели
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != user.Id)
            {
                return BadRequest("ID в пути и в теле запроса не совпадают");
            }

            // Дополнительная валидация
            var validationResult = ValidateUser(user);
            if (validationResult != null)
            {
                return BadRequest(validationResult);
            }

            // Проверка существования пользователя
            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
            {
                return NotFound($"Пользователь с ID {id} не найден");
            }

            // Проверка уникальности логина (исключая текущего пользователя)
            if (await _context.Users.AnyAsync(u => u.Username == user.Username && u.Id != id))
            {
                return BadRequest("Пользователь с таким логином уже существует");
            }

            // Проверка уникальности email (если указан)
            if (!string.IsNullOrWhiteSpace(user.Email) &&
                await _context.Users.AnyAsync(u => u.Email == user.Email && u.Id != id))
            {
                return BadRequest("Пользователь с таким email уже существует");
            }

            // Сохраняем токен, если он не меняется
            if (string.IsNullOrWhiteSpace(user.Token))
            {
                user.Token = existingUser.Token;
            }

            // Сохраняем пароль, если он не меняется
            if (string.IsNullOrWhiteSpace(user.Password))
            {
                user.Password = existingUser.Password;
            }

            _context.Entry(existingUser).CurrentValues.SetValues(user);
            _context.Entry(existingUser).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound($"Пользователь с ID {id} не найден");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        /// <summary>
        /// Проверка наличия связей пользователя с другими таблицами
        /// </summary>
        /// <param name="id">ID пользователя</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>true если есть связи, false если нет</returns>
        [HttpGet("{id}/check-relations")]
        public async Task<ActionResult<bool>> CheckUserRelations(
            [FromQuery] string? token,
            int id)
        {
            // Проверка прав на чтение
            if (!await _roleChecker.CanRead(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            var userExists = await _context.Users.AnyAsync(u => u.Id == id);
            if (!userExists)
            {
                return NotFound($"Пользователь с ID {id} не найден");
            }

            // Проверка всех возможных связей
            var hasRelations =
                await _context.Rooms.AnyAsync(r => r.ResponsibleUserId == id || r.TempResponsibleUserId == id) ||
                await _context.InventoryChecks.AnyAsync(ic => ic.CheckedByUserId == id) ||
                await _context.Inventories.AnyAsync(i => i.CreatedByUserId == id) ||
                await _context.EquipmentRoomHistory.AnyAsync(erh => erh.MovedByUserId == id) ||
                await _context.EquipmentResponsibleHistory.AnyAsync(erh =>
                    erh.ResponsibleUserId == id || erh.AssignedByUserId == id) ||
                await _context.Equipment.AnyAsync(e =>
                    e.ResponsibleUserId == id || e.TempResponsibleUserId == id) ||
                await _context.ConsumableResponsibleHistory.AnyAsync(crh =>
                    crh.ResponsibleUserId == id || crh.AssignedByUserId == id) ||
                await _context.ConsumableEquipment.AnyAsync(ce => ce.AttachedByUserId == id) ||
                await _context.Consumables.AnyAsync(c =>
                    c.ResponsibleUserId == id || c.TempResponsibleUserId == id);

            return hasRelations;
        }

        /// <summary>
        /// Удалить пользователя по id
        /// </summary>
        /// <param name="id">ID пользователя</param>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Результат операции</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(
            [FromQuery] string? token,
            int id)
        {
            // Проверка прав на запись
            if (!await _roleChecker.CanWrite(token))
            {
                return Unauthorized("Недостаточно прав для выполнения операции");
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound($"Пользователь с ID {id} не найден");
            }

            // Проверка наличия связей перед удалением
            var hasRelations = await CheckUserRelationsInternal(id);
            if (hasRelations)
            {
                return BadRequest("Невозможно удалить пользователя, так как он связан с другими записями в системе");
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }

        private async Task<bool> CheckUserRelationsInternal(int userId)
        {
            return
                await _context.Rooms.AnyAsync(r => r.ResponsibleUserId == userId || r.TempResponsibleUserId == userId) ||
                await _context.InventoryChecks.AnyAsync(ic => ic.CheckedByUserId == userId) ||
                await _context.Inventories.AnyAsync(i => i.CreatedByUserId == userId) ||
                await _context.EquipmentRoomHistory.AnyAsync(erh => erh.MovedByUserId == userId) ||
                await _context.EquipmentResponsibleHistory.AnyAsync(erh =>
                    erh.ResponsibleUserId == userId || erh.AssignedByUserId == userId) ||
                await _context.Equipment.AnyAsync(e =>
                    e.ResponsibleUserId == userId || e.TempResponsibleUserId == userId) ||
                await _context.ConsumableResponsibleHistory.AnyAsync(crh =>
                    crh.ResponsibleUserId == userId || crh.AssignedByUserId == userId) ||
                await _context.ConsumableEquipment.AnyAsync(ce => ce.AttachedByUserId == userId) ||
                await _context.Consumables.AnyAsync(c =>
                    c.ResponsibleUserId == userId || c.TempResponsibleUserId == userId);
        }


        // ЛИШНИЙ КОД
        //private string? ValidateUser(User user)
        //{
        //    // Проверка обязательных полей
        //    if (string.IsNullOrWhiteSpace(user.Username))
        //    {
        //        return "Логин обязателен для заполнения";
        //    }

        //    if (string.IsNullOrWhiteSpace(user.Password))
        //    {
        //        return "Пароль обязателен для заполнения";
        //    }

        //    if (string.IsNullOrWhiteSpace(user.LastName))
        //    {
        //        return "Фамилия обязательна для заполнения";
        //    }

        //    if (string.IsNullOrWhiteSpace(user.FirstName))
        //    {
        //        return "Имя обязательно для заполнения";
        //    }

        //    // Проверка формата логина
        //    if (!Regex.IsMatch(user.Username, @"^[a-zA-Z0-9_]+$"))
        //    {
        //        return "Логин может содержать только буквы, цифры и символ подчеркивания";
        //    }

        //    // Проверка роли
        //    if (!Regex.IsMatch(user.Role, @"^(employee|teacher|administrator)$"))
        //    {
        //        return "Роль должна быть: employee, teacher или administrator";
        //    }

        //    // Проверка email если указан
        //    if (!string.IsNullOrWhiteSpace(user.Email))
        //    {
        //        try
        //        {
        //            var email = new System.Net.Mail.MailAddress(user.Email);
        //            if (email.Address != user.Email)
        //            {
        //                return "Некорректный формат email";
        //            }
        //        }
        //        catch
        //        {
        //            return "Некорректный формат email";
        //        }
        //    }

        //    // Проверка телефона если указан
        //    if (!string.IsNullOrWhiteSpace(user.Phone))
        //    {
        //        if (!Regex.IsMatch(user.Phone, @"^[\d\s\-\+\(\)]+$"))
        //        {
        //            return "Телефон может содержать только цифры, пробелы, тире, плюс и скобки";
        //        }
        //    }

        //    return null;
        //}
    }
}
