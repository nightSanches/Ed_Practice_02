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
using System.Windows.Shapes;

namespace EquipmentAccounting.Pages.Model
{
    /// <summary>
    /// Логика взаимодействия для Add.xaml
    /// </summary>
    public partial class Add : Window
    {
        private Models.Model direction;
        public readonly ModelService _directionsService = new ModelService();
        public Add(Models.Model dir = null)
        {
            InitializeComponent();
            direction = dir;

            foreach (var i in UserSession.DropdownData.EquipmentTypes)
            {
                var cbx = new ComboBoxItem
                {
                    Content = i.DisplayText,
                    Tag = i.Id
                };
                room.Items.Add(cbx);
            }

            if (direction != null)
            {
                txtName.Text = direction.Name;
                foreach (ComboBoxItem i in room.Items)
                {
                    if (i.Tag is int TagId && TagId == direction.EquipmentTypeId)
                    {
                        room.SelectedItem = i;
                        break;
                    }
                }
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnAddClick(object sender, RoutedEventArgs e)
        {
            if (direction == null)
            {
                var temp = new Models.Model();

                //обязательно для заполнения
                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    MessageBox.Show("Наименование обязательно для заполнения", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else temp.Name = txtName.Text;

                ComboBoxItem item = new ComboBoxItem();
                item = (ComboBoxItem)room.SelectedItem;
                if (item.Tag.ToString() != "empty")
                    temp.EquipmentTypeId = Convert.ToInt32(item.Tag);
                else
                {
                    MessageBox.Show("Тип оборудования обязателен для заполнения", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                direction = temp;

                CreateDirection();
                MessageBox.Show("Модель оборудования добавлена", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                MainWindow.init.OpenPages(new Main());
                this.Close();
            }
            else
            {
                //обязательно для заполнения
                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    MessageBox.Show("Наименование обязательно для заполнения", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else direction.Name = txtName.Text;

                ComboBoxItem item = new ComboBoxItem();
                item = (ComboBoxItem)room.SelectedItem;
                if (item.Tag.ToString() != "empty")
                    direction.EquipmentTypeId = Convert.ToInt32(item.Tag);
                else
                {
                    MessageBox.Show("Тип оборудования обязателен для заполнения", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                UpdateDirection();
                MessageBox.Show("Модель оборудования изменена", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                MainWindow.init.OpenPages(new Main());
                this.Close();
            }
        }

        private async void CreateDirection()
        {
            await _directionsService.CreateModelAsync(direction);
        }

        private async void UpdateDirection()
        {
            await _directionsService.UpdateModelAsync(direction.Id, direction);
        }
    }
}
