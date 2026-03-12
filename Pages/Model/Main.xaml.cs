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

namespace EquipmentAccounting.Pages.Model
{
    /// <summary>
    /// Логика взаимодействия для Main.xaml
    /// </summary>
    public partial class Main : Page
    {
        public static Main init;
        private bool _isInitializing = true;
        public readonly ModelService _directionsService = new ModelService();
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

        private async void LoadTable(string? search = null, string? sortBy = "id", string? sortOrder = "asc")
        {
            await LoadEquipmentAsync(search, sortBy, sortOrder);
        }

        private async Task LoadEquipmentAsync(string? search = null, string? sortBy = "id", string? sortOrder = "asc")
        {
            try
            {
                var allEquipment = await _directionsService.GetModelAsync(search, sortBy, sortOrder);
                parent.Children.Clear();
                if (allEquipment != null && allEquipment.Count > 0)
                {
                    int count = 0;
                    foreach (var equipment in allEquipment)
                    {
                        count++;
                        var equipmentItem = new Item(equipment, this, count);
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
        private void cmbSortBy_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing) return;
            LoadTable(txtSearch.Text, getSortBy(), getSortOrder());
        }
        bool desc = false;
        private void btnSortOrder_Click(object sender, RoutedEventArgs e)
        {
            if (_isInitializing) return;
            if (desc == false)
            {
                desc = true;
                btnSortOrder.Content = "По убыванию";
            }
            else
            {
                desc = false;
                btnSortOrder.Content = "По возрастанию";
            }
            LoadTable(txtSearch.Text, getSortBy(), getSortOrder());
        }

        private string getSortOrder()
        {
            if (desc == true)
            {
                return "desc";
            }
            else
            {
                return "ask";
            }
        }
        private string getSortBy()
        {
            if (cmbSortBy.SelectedItem is ComboBoxItem selectedItem)
            {
                switch (selectedItem.Content.ToString())
                {
                    case "Id": return "id";
                    case "Наименование": return "name";
                    case "Тип оборудования": return "equipmenttypeid";
                    default: return "id";
                }
            }
            return "id";
        }
        public async void DeleteEquipment(int id)
        {
            await _directionsService.DeleteModelAsync(id);
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
