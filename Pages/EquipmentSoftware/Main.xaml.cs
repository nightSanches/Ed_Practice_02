using EquipmentAccounting.Models;
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

namespace EquipmentAccounting.Pages.EquipmentSoftware
{
    /// <summary>
    /// Логика взаимодействия для Main.xaml
    /// </summary>
    public partial class Main : Page
    {
        public static Main init;
        private Models.Equipment equipment;
        public readonly EquipmentSoftwareService _equipmentSoftwareService = new EquipmentSoftwareService();
        public Main(Models.Equipment equipment)
        {
            InitializeComponent();
            this.equipment = equipment;
            txtTitle.Text = "ПРОГРАММНОЕ ОБЕСПЕЧЕНИЕ - " + this.equipment.Name;
            init = this;
        }

        private void btnCancelClick(object sender, RoutedEventArgs e)
        {
            MainWindow.init.OpenPages(new Pages.Equipment.Add(equipment));
        }

        private void btnAddClick(object sender, RoutedEventArgs e)
        {
            Window addWindow = new Add(_equipmentSoftwareService, equipment.Id, equipment);
            addWindow.Show();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadTable();
        }

        public async void LoadTable()
        {
            await LoadEquipmentAsync();
        }

        private async Task LoadEquipmentAsync()
        {
            try
            {
                var allEquipmentSoftware = await _equipmentSoftwareService.GetEquipmentSoftwareAsync(equipment.Id);
                parent.Children.Clear();
                if (allEquipmentSoftware != null && allEquipmentSoftware.Count > 0)
                {
                    int count = 0;
                    foreach (var equipmentSoftware in allEquipmentSoftware)
                    {
                        count++;
                        var equipmentSoftwareItem = new Item(equipmentSoftware, count);
                        parent.Children.Add(equipmentSoftwareItem);
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
            await _equipmentSoftwareService.DeleteEquipmentSoftwareAsync(id);
            LoadTable();
        }
    }
}
