using EquipmentAccounting.Pages.Equipment;
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

namespace EquipmentAccounting.Pages.EquipmentNetwork
{
    /// <summary>
    /// Логика взаимодействия для Main.xaml
    /// </summary>
    public partial class Main : Page
    {
        public static Main init;
        private Models.Equipment equipment;
        public readonly NetworkSettingsService _networkSettingsService = new NetworkSettingsService();
        public Main(Models.Equipment equipment)
        {
            InitializeComponent();
            this.equipment = equipment;
            txtTitle.Text = "Сетевые настройки - " + this.equipment.Name;
            init = this;
        }
        private void btnCancelClick(object sender, RoutedEventArgs e)
        {
            MainWindow.init.OpenPages(new Equipment.Add(equipment));
        }

        private void btnAddClick(object sender, RoutedEventArgs e)
        {
            MainWindow.init.OpenPages(new EquipmentNetwork.Add(equipment));
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadTable();
        }

        private async void LoadTable(string? search = null, string? sortBy = "id", string? sortOrder = "asc")
        {
            await LoadNetworkAsync(search, sortBy, sortOrder);
        }

        private async Task LoadNetworkAsync(string? search = null, string? sortBy = "id", string? sortOrder = "asc")
        {
            try
            {
                var allEquipment = await _networkSettingsService.GetNetworkAsync(search, sortBy, sortOrder);
                parent.Children.Clear();
                if (allEquipment != null && allEquipment.Count > 0)
                {
                    int count = 0;
                    foreach (var network in allEquipment)
                    {
                        if(network.EquipmentId == equipment.Id)
                        {
                            count++;
                            var equipmentItem = new Item(network, this, count, equipment);
                            parent.Children.Add(equipmentItem);
                        }
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
        public async void DeleteNetwork(int id)
        {
            await _networkSettingsService.DeleteNetworkAsync(id);
        }

    }
}
