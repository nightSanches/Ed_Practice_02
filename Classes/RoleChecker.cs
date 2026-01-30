using Microsoft.EntityFrameworkCore;

namespace API.Classes
{
    public class RoleChecker
    {
        private readonly DatabaseConnection _context;

        public RoleChecker(DatabaseConnection context)
        {
            _context = context;
        }

        /// <summary>
        /// Получить роль пользователя по токену
        /// </summary>
        /// <param name="token">Токен пользователя</param>
        /// <returns>Роль пользователя или null если не найден</returns>
        public async Task<string?> GetUserRoleByToken(string? token)
        {
            if (string.IsNullOrEmpty(token))
                return null;

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Token == token);
            return user?.Role;
        }

        /// <summary>
        /// Проверить, имеет ли пользователь права на чтение
        /// </summary>
        public async Task<bool> CanRead(string? token)
        {
            var role = await GetUserRoleByToken(token);
            return role == "employee" || role == "teacher" || role == "administrator";
        }

        /// <summary>
        /// Проверить, имеет ли пользователь права на запись
        /// </summary>
        public async Task<bool> CanWrite(string? token)
        {
            var role = await GetUserRoleByToken(token);
            return role == "teacher" || role == "administrator";
        }
    }
}
