using EquipmentAccounting.Models;
using EquipmentAccounting.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EquipmentAccounting.Pages.Directions
{
    /// <summary>
    /// Логика взаимодействия для Add.xaml
    /// </summary>
    public partial class Add : Window
    {
        private Models.Direction direction;
        public readonly DirectionsService _directionsService = new DirectionsService();
        public Add(Models.Direction dir = null)
        {
            InitializeComponent();
            direction = dir;
            if(direction!=null)
                txtName.Text = direction.Name;
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
                var temp = new Models.Direction();

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
                MainWindow.init.OpenPages(new Directions.Main());
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
                MessageBox.Show("Направление изменено", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                MainWindow.init.OpenPages(new Directions.Main());
                this.Close();
            }
        }

        private async void CreateDirection()
        {
            await _directionsService.CreateDirectionAsync(direction);
        }

        private async void UpdateDirection()
        {
            await _directionsService.UpdateDirectionAsync(direction.Id, direction);
        }
    }
}
