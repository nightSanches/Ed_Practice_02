using EquipmentAccounting;
using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace EquipmentAccounting.Pages.Equipment
{
    public partial class EquipmentPage : Page
    {
        public readonly EquipmentService _equipmentService = new EquipmentService();
        private bool _isInitializing = true;
        public EquipmentPage()
        {
            InitializeComponent();
            _isInitializing = false;
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
                var allEquipment = await _equipmentService.GetEquipmentAsync(search, sortBy, sortOrder);
                parent.Children.Clear();
                if (allEquipment != null && allEquipment.Count > 0)
                {
                    foreach (var equipment in allEquipment)
                    {
                        var equipmentItem = new Item(equipment, this);
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
            await _equipmentService.DeleteEquipmentAsync(id);
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
                    case "Инвентарный номер": return "inventoryNumber";
                    case "Стоимость": return "cost";
                    default: return "id";
                }
            }
            return "id";
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.init.OpenPages(new Pages.MainPage());
        }

        private void txtPass_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LoadTable(txtSearch.Text, getSortBy(), getSortOrder());
            }
        }

        private void btnAddClick(object sender, RoutedEventArgs e)
        {

        }

    }
}
