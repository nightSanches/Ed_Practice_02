using EquipmentAccounting.Services;
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

namespace EquipmentAccounting.Pages.Status
{
    /// <summary>
    /// Логика взаимодействия для Main.xaml
    /// </summary>
    public partial class Main : Page
    {
        public static Main init;
        private bool _isInitializing = true;
        public readonly StatusService _directionsService = new StatusService();
        public Main()
        {
            InitializeComponent();
            _isInitializing = false;
            init = this;
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadTable();
        }

        private async void LoadTable(string? search = null)
        {
            await LoadEquipmentAsync(search);
        }

        private async Task LoadEquipmentAsync(string? search = null)
        {
            try
            {
                var allDirections = await _directionsService.GetStatusAsync(search);
                parent.Children.Clear();
                if (allDirections != null && allDirections.Count > 0)
                {
                    int count = 0;
                    foreach (var temp in allDirections)
                    {
                        count++;
                        var equipmentItem = new Item(temp, this, count);
                        parent.Children.Add(equipmentItem);
                    }

                    Console.WriteLine("Данные успешно загружены");
                }
                else
                {
                    Console.WriteLine("Не удалось загрузить данные");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке данных: {ex.Message}");
            }
        }
        public async void DeleteEquipment(int id)
        {
            await _directionsService.DeleteStatusAsync(id);
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.init.OpenPages(new MainPage());
        }

        private void txtPass_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LoadTable(txtSearch.Text);
            }
        }

        private void btnAddClick(object sender, RoutedEventArgs e)
        {
            Window addWindow = new Add();
            addWindow.Show();
        }
    }
}
