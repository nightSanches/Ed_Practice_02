namespace API.Models.Authentification
{
    /// <summary>
    /// Модель запроса для авторизации пользователя
    /// </summary>
    public class LoginRequest
    {
        public string Username { get; set; }

        public string Password { get; set; }
    }
}
