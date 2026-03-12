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
using System.Xml.Linq;

namespace EquipmentAccounting.Pages.EquipmentNetwork
{
    public partial class Item : UserControl
    {
        Models.Equipment equipment;
        Main main;
        NetworkSettings networkSettings;
        public Item(NetworkSettings settings, Main main, int count, Models.Equipment equipment)
        {
            InitializeComponent();

            this.main = main;
            this.equipment = equipment;
            networkSettings = settings;

            txtId.Text = count.ToString();
            txtIp.Text = "IP: " + networkSettings.IpAddress;
            txtMask.Text = "Маска подсети: " + networkSettings.SubnetMask;
            txtGateway.Text = "Главный шлюз: " + networkSettings.DefaultGateway;
            txtDns1.Text = "DNS 1: " + networkSettings.DnsPrimary;
            txtDns2.Text = "DNS 2: " + networkSettings.DnsSecondary;
            txtMacAddress.Text = "MAC-адрес: " + networkSettings.MacAddress;
            this.equipment = equipment;
        }
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вы уверены что хотите удалить запись?", "Удаление", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
            {
                return;
            }
            main.DeleteNetwork(networkSettings.Id);
            main.parent.Children.Remove(this);
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
            MainWindow.init.OpenPages(new EquipmentNetwork.Add(equipment, networkSettings));
        }
    }
}
