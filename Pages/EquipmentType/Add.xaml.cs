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

namespace EquipmentAccounting.Pages.EquipmentType
{
    /// <summary>
    /// Логика взаимодействия для Add.xaml
    /// </summary>
    public partial class Add : Window
    {
        private Models.EquipmentType direction;
        public readonly EquipmentTypeService _directionsService = new EquipmentTypeService();
        public Add(Models.EquipmentType dir = null)
        {
            InitializeComponent();
            direction = dir;
            if (direction != null) txtName.Text = direction.Name;
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
                var temp = new Models.EquipmentType();

                //обязательно для заполнения
                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    MessageBox.Show("Наименование обязательно для заполнения", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else temp.Name = txtName.Text;

                direction = temp;

                CreateDirection();
                MessageBox.Show("Направление добавлено", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                MainWindow.init.OpenPages(new EquipmentType.Main());
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

                UpdateDirection();
                MessageBox.Show("Наименование изменено", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                MainWindow.init.OpenPages(new EquipmentType.Main());
                this.Close();
            }
        }

        private async void CreateDirection()
        {
            await _directionsService.CreateEquipmentTypeAsync(direction);
        }

        private async void UpdateDirection()
        {
            await _directionsService.UpdateEquipmentTypeAsync(direction.Id, direction);
        }
    }
}
