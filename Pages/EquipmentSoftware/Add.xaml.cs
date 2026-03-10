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

namespace EquipmentAccounting.Pages.EquipmentSoftware
{
    /// <summary>
    /// Логика взаимодействия для Add.xaml
    /// </summary>
    public partial class Add : Window
    {
        private Models.Equipment equipment;
        public readonly EquipmentSoftwareService _equipmentSoftwareService = new EquipmentSoftwareService();
        public static Add init;
        int equipmentId;
        public Add(EquipmentSoftwareService equipmentSoftwareService, int equipmentId, Models.Equipment equipment)
        {
            InitializeComponent();
            _equipmentSoftwareService = equipmentSoftwareService;
            this.equipmentId = equipmentId; 
            this.equipment = equipment;
            init = this;

            int count = 0;
            foreach(var i in UserSession.DropdownData.Software)
            {
                count++;
                var equipmentSoftwareAddItem = new AddItem(i.Id, i.DisplayText, count);
                parent.Children.Add(equipmentSoftwareAddItem);
            }
        }

        public void AddSoftware(int softwareId)
        {
            CreateEquipmentSoftware(equipmentId, softwareId);
        }
        private async void CreateEquipmentSoftware(int eId, int sId)
        {
            Models.EquipmentSoftware eS = new Models.EquipmentSoftware();
            eS.EquipmentId = eId;
            eS.SoftwareId = sId;
            await _equipmentSoftwareService.CreateEquipmentSoftwareAsync(eS);
            MessageBox.Show("Программное обеспечение добавлено", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            Main.init.LoadTable();
            this.Close();
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
    }
}
