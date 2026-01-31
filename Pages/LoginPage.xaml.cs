using EquipmentAccounting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using EquipmentAccounting.Models;
using EquipmentAccounting.Services;

namespace EquipmentAccounting.Pages
{
    public partial class LoginPage : Page
    {
        private readonly LoginService _loginService;

        public LoginPage()
        {
            InitializeComponent();

            _loginService = new LoginService();
        }

        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            await PerformLogin();
        }

        private async Task PerformLogin()
        {
            // Получаем пароль из PasswordBox
            var login = txtUser.Text;
            var password = txtPass.Password;

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                txtError.Text = "Введите логин и пароль";
                return;
            }

            // Блокируем кнопку на время выполнения запроса
            btnLogin.IsEnabled = false;
            txtError.Text = string.Empty;

            try
            {
                // Выполняем авторизацию через API
                var result = await _loginService.LoginAsync(login, password);

                if (result.Success)
                {
                    // Переход на главную страницу
                    NavigateToMainPage();
                }
                else
                {
                    txtError.Text = result.Error;
                    txtPass.Clear();
                }
            }
            catch (Exception ex)
            {
                txtError.Text = $"Ошибка: {ex.Message}";
            }
            finally
            {
                // Разблокируем кнопку
                btnLogin.IsEnabled = true;
            }
        }

        private void NavigateToMainPage()
        {
            MainWindow.init.OpenPages(new Pages.MainPage());
        }

        // Обработка нажатия Enter в полях ввода
        private void txtUser_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                txtPass.Focus();
            }
        }

        private async void txtPass_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await PerformLogin();
            }
        }
    }
}
