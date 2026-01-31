using EquipmentAccounting.Models;
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
using System.Xml.Linq;
using EquipmentAccounting.Models;
using EquipmentAccounting.Services;

namespace EquipmentAccounting.Pages
{
    /// <summary>
    /// Логика взаимодействия для MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        private readonly DropdownsService _dropdownsService = new DropdownsService();
        public MainPage()
        {
            InitializeComponent();

            InitializeUserInfo();
            LoadingProgressBar.Visibility = Visibility.Collapsed;
            SetAllButtonsEnabled(false);
        }
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Начинаем загрузку данных при загрузке страницы
            await LoadDropdownDataAsync();
        }
        private void InitializeUserInfo()
        {
            tbName.Text = UserSession.FullName;

            if (UserSession.Role == "administrator")
            {
                tbRole.Text = "Администратор";
            }
            else if (UserSession.Role == "teacher")
            {
                tbRole.Text = "Преподаватель";
            }
            else if (UserSession.Role == "employee")
            {
                tbRole.Text = "Сотрудник";
            }
        }
        private async Task LoadDropdownDataAsync()
        {
            try
            {
                ShowProgressBar(true);

                var dropdownData = await _dropdownsService.LoadAllDropdownDataAsync();

                if (dropdownData != null)
                {
                    UserSession.DropdownData = dropdownData;

                    SetAllButtonsEnabled(true);

                    Console.WriteLine("Данные выпадающих списков успешно загружены");
                }
                else
                {
                    Console.WriteLine("Не удалось загрузить данные выпадающих списков");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке данных: {ex.Message}");
            }
            finally
            {
                ShowProgressBar(false);
            }
        }
        private void ShowProgressBar(bool show)
        {
            Dispatcher.Invoke(() =>
            {
                if (show)
                {
                    LoadingProgressBar.Visibility = Visibility.Visible;
                    LoadingProgressBar.IsIndeterminate = true;
                }
                else
                {
                    LoadingProgressBar.Visibility = Visibility.Collapsed;
                    LoadingProgressBar.IsIndeterminate = false;
                }
            });
        }

        private void SetAllButtonsEnabled(bool enabled)
        {
            Dispatcher.Invoke(() =>
            {
                var style = enabled ?
                    (Style)FindResource("MenuButtonStyle") :
                    (Style)FindResource("DisabledMenuButtonStyle");

                var navigationButtons = new[]
                {
                    btnEquipment, btnDirections, btnEquipmentModels, btnEquipmentTypes,
                    btnStatuses, btnNetworkSettings, btnSoftware, btnDevelopers,
                    btnConsumables, btnConsumableSpecs, btnConsumableTypes,
                    btnInventory, btnClassrooms, btnUsers
                };

                foreach (var button in navigationButtons)
                {
                    if (button != null)
                    {
                        button.IsEnabled = enabled;
                        button.Style = style;
                    }
                }
            });
        }

        private void btnEquipment_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.init.OpenPages(new Pages.Equipment.EquipmentPage());
        }

        private void btnDirections_Click(object sender, RoutedEventArgs e)
        {
            // Навигация на страницу направлений
        }

        private void btnEquipmentModels_Click(object sender, RoutedEventArgs e)
        {
            // Навигация на страницу моделей оборудования
        }

        private void btnEquipmentTypes_Click(object sender, RoutedEventArgs e)
        {
            // Навигация на страницу типов оборудования
        }

        private void btnStatuses_Click(object sender, RoutedEventArgs e)
        {
            // Навигация на страницу статусов
        }

        private void btnNetworkSettings_Click(object sender, RoutedEventArgs e)
        {
            // Навигация на страницу сетевых настроек
        }

        // Обработчики для блока Программное обеспечение
        private void btnSoftware_Click(object sender, RoutedEventArgs e)
        {
            // Навигация на страницу ПО
        }

        private void btnDevelopers_Click(object sender, RoutedEventArgs e)
        {
            // Навигация на страницу разработчиков
        }

        // Обработчики для блока Расходные материалы
        private void btnConsumables_Click(object sender, RoutedEventArgs e)
        {
            // Навигация на страницу расходных материалов
        }

        private void btnConsumableSpecs_Click(object sender, RoutedEventArgs e)
        {
            // Навигация на страницу характеристик расходников
        }

        private void btnConsumableTypes_Click(object sender, RoutedEventArgs e)
        {
            // Навигация на страницу типов расходников
        }

        // Обработчики для блока Другое
        private void btnInventory_Click(object sender, RoutedEventArgs e)
        {
            // Навигация на страницу инвентаризации
        }

        private void btnClassrooms_Click(object sender, RoutedEventArgs e)
        {
            // Навигация на страницу аудиторий
        }

        private void btnUsers_Click(object sender, RoutedEventArgs e)
        {
            // Навигация на страницу пользователей
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            UserSession.Clear();
            MainWindow.init.OpenPages(new Pages.LoginPage());
        }
    }
}
