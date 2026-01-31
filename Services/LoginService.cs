using System.Net.Http;
using System.Text.Json;
using EquipmentAccounting.Models.AuthModels;
using EquipmentAccounting.Models;

namespace EquipmentAccounting.Services
{
    public class LoginService : ApiClient
    {
        public async Task<(bool Success, AuthResponse? Response, string Error)> LoginAsync(string username, string password)
        {
            try
            {
                var authRequest = new AuthRequest
                {
                    Username = username,
                    Password = password
                };

                var response = await SendRequestAsync(HttpMethod.Post, "auth/login", authRequest);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var authResponse = JsonSerializer.Deserialize<AuthResponse>(json);

                    if (authResponse != null)
                    {
                        UserSession.Token = authResponse.Token;
                        UserSession.Role = authResponse.Role;
                        UserSession.FullName = authResponse.FullName;

                        return (true, authResponse, string.Empty);
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return (false, null, "Неверные учетные данные");
                }

                return (false, null, "Ошибка при авторизации");
            }
            catch (HttpRequestException ex)
            {
                return (false, null, $"Ошибка подключения: {ex.Message}");
            }
            catch (Exception ex)
            {
                return (false, null, $"Ошибка: {ex.Message}");
            }
        }
    }
}
