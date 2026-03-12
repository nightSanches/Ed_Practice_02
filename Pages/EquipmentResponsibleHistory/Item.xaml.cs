using EquipmentAccounting.Models;
using EquipmentAccounting.Pages.Equipment;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace EquipmentAccounting.Pages.EquipmentResponsibleHistory
{
    /// <summary>
    /// Логика взаимодействия для Item.xaml
    /// </summary>
    public partial class Item : UserControl
    {
        Main main;
        Models.EquipmentResponsibleHistory equipment;
        public Item(Models.EquipmentResponsibleHistory equipment, Main main, int count)
        {
            InitializeComponent();
            this.equipment = equipment;
            this.main = main;

            txtId.Text = count.ToString();
            txtName.Text = UserSession.DropdownData.Users.FirstOrDefault(item => item.Id == equipment.ResponsibleUserId)?.DisplayText;
            txtCom.Text = equipment.Comment;
            txtAssignedBy.Text = UserSession.DropdownData.Users.FirstOrDefault(item => item.Id == equipment.AssignedByUserId)?.DisplayText;
            txtAssignedTime.Text = equipment.AssignedAt.ToString();
        }
        
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вы уверены что хотите удалить запись?", "Удаление", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
            {
                return;
            }
            main.DeleteEquipment(equipment.Id);
            main.parent.Children.Remove(this);
        }

        private void OnEnter(object sender, MouseEventArgs e)
        {
            itemBorder.Background = (Brush)new BrushConverter().ConvertFrom("#e7ebee");
            brdrCom.Background = (Brush)new BrushConverter().ConvertFrom("#F8F9FA");
        }

        private void OnLeave(object sender, MouseEventArgs e)
        {
            itemBorder.Background = (Brush)new BrushConverter().ConvertFrom("#F8F9FA");
            brdrCom.Background = (Brush)new BrushConverter().ConvertFrom("#f2f2f2");
        }

        private void btnDocument_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
