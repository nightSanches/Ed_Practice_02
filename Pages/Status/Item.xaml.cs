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
    /// Логика взаимодействия для Item.xaml
    /// </summary>
    public partial class Item : UserControl
    {
        Main main;
        Models.Status direction;
        public Item(Models.Status dir, Main main, int count)
        {
            InitializeComponent();
            direction = dir;
            this.main = main;

            txtId.Text = count.ToString();
            txtName.Text = direction.Name.ToString();
        }

        private void OnEnter(object sender, MouseEventArgs e)
        {
            itemBorder.Background = (Brush)new BrushConverter().ConvertFrom("#e7ebee");
        }

        private void OnLeave(object sender, MouseEventArgs e)
        {
            itemBorder.Background = (Brush)new BrushConverter().ConvertFrom("#F8F9FA");
        }

        private void OnEdit(object sender, MouseButtonEventArgs e)
        {
            Window addWindow = new Add(direction);
            addWindow.Show();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вы уверены что хотите удалить запись?", "Удаление", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
            {
                return;
            }
            main.DeleteEquipment(direction.Id);
            main.parent.Children.Remove(this);
        }
    }
}
