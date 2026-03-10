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
    /// Логика взаимодействия для AddItem.xaml
    /// </summary>
    public partial class AddItem : UserControl
    {
        int Id;
        public AddItem(int id, string text, int Count)
        {
            InitializeComponent();
            Id = id;
            txtName.Text = text;
            txtId.Text = Count.ToString();
        }

        private void OnAdd(object sender, MouseButtonEventArgs e)
        {
            EquipmentSoftware.Add.init.AddSoftware(Id);
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
