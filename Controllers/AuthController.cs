using Microsoft.AspNetCore.Mvc;
using API.Classes;
using API.Models.Authentification;
using API.Models;

namespace API.Controllers
{
    /// <summary>
    /// Контроллер для управления аутентификацией пользователей
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly DatabaseConnection _context;

        /// <summary>
        /// Конструктор контроллера авторизации
        /// </summary>
        /// <param name="context">Контекст базы данных</param>
        public AuthController(DatabaseConnection context)
        {
            _context = context;
        }

        /// <summary>
        /// Авторизация пользователя по логину и паролю
        /// </summary>
        /// <param name="request">Данные для входа</param>
        /// <returns>Токен доступа при успешной авторизации</returns>
        /// <remarks>
        /// Пример запроса:
        /// POST /api/auth/login
        /// {
        ///     "username": "admin",
        ///     "password": "admin"
        /// }
        /// </remarks>
        /// <response code="200">Успешная авторизация, возвращает токен</response>
        /// <response code="401">Неверные учетные данные</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            try
            {
                //валидация входных данных
                if (string.IsNullOrWhiteSpace(request.Username) ||
                    string.IsNullOrWhiteSpace(request.Password))
                {
                    return Unauthorized(new { message = "Логин и пароль обязательны для заполнения" });
                }

                //поиск пользователя по логину
                var user = _context.Users
                    .FirstOrDefault(u => u.Username == request.Username);

                //проверка существования пользователя и корректности пароля
                if (user == null || user.Password != request.Password)
                {
                    return Unauthorized(new { message = "Неверный логин или пароль" });
                }

                //генерация нового токена
                var newToken = GenerateRandomToken(32);

                //обновление токена в базе данных
                user.Token = newToken;
                _context.SaveChanges();

                //формирование полного имени пользователя
                string fullName = $"{user.LastName} {user.FirstName}";
                if (!string.IsNullOrWhiteSpace(user.MiddleName))
                {
                    fullName += $" {user.MiddleName}";
                }

                //возврат ответа с токеном
                var response = new LoginResponse
                {
                    Token = newToken,
                    Role = user.Role,
                    FullName = fullName
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "Внутренняя ошибка сервера при авторизации" });
            }
        }

        private string GenerateRandomToken(int length)
        {
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            var random = new Random();
            var chars = new char[length];

            for (int i = 0; i < length; i++)
            {
                chars[i] = validChars[random.Next(validChars.Length)];
            }

            for (int i = 0; i < length; i++)
            {
                int swapIndex = random.Next(length);
                (chars[i], chars[swapIndex]) = (chars[swapIndex], chars[i]);
            }

            return new string(chars);
        }

        internal static bool ValidateToken(string token, DatabaseConnection context)
        {
            if (string.IsNullOrWhiteSpace(token))
                return false;

            // Проверка существования пользователя с таким токеном
            var user = context.Users.FirstOrDefault(u => u.Token == token);
            return user != null;
        }
    }
}
