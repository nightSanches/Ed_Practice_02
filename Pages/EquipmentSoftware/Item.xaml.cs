using EquipmentAccounting.Models;
using EquipmentAccounting.Pages.Equipment;
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
    /// Логика взаимодействия для Item.xaml
    /// </summary>
    public partial class Item : UserControl
    {
        Models.EquipmentSoftware equipmentSoftware;
        Software thisSoftware;
        public Item(Models.EquipmentSoftware equipmentSoftware, int Id, Software software)
        {
            InitializeComponent();
            this.equipmentSoftware = equipmentSoftware;


            txtId.Text = Id + ".";
            txtName.Text = software.Name;
            txtVer.Text = "Версия: " + software.Version;
            txtDev.Text = "Разработчик: " + UserSession.DropdownData.Developers.FirstOrDefault(item => item.Id == software.DeveloperId)?.DisplayText;
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вы уверены что хотите удалить запись?", "Удаление", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
            {
                return;
            }
            Main.init.DeleteEquipment(equipmentSoftware.Id);
            Main.init.parent.Children.Remove(this);
        }

        private void OnLeave(object sender, MouseEventArgs e)
        {
            itemBorder.Background = (Brush)new BrushConverter().ConvertFrom("#F8F9FA");
        }

        private void OnEnter(object sender, MouseEventArgs e)
        {
            itemBorder.Background = (Brush)new BrushConverter().ConvertFrom("#e7ebee");
        }
    }
}
