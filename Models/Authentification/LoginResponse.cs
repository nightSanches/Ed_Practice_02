namespace API.Models.Authentification
{
    /// <summary>
    /// Модель ответа на успешную авторизацию
    /// </summary>
    public class LoginResponse
    {
        /// <summary>
        /// Токен доступа
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Роль пользователя
        /// </summary>
        public string Role { get; set; }

        /// <summary>
        /// ФИО пользователя
        /// </summary>
        public string FullName { get; set; }
    }
}
