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
using System.Xml.Linq;

namespace EquipmentAccounting.Pages.EquipmentResponsibleHistory
{
    /// <summary>
    /// Логика взаимодействия для Add.xaml
    /// </summary>
    public partial class Add : Window
    {
        private int respId;
        private int eqId;
        private Models.EquipmentResponsibleHistory direction = new Models.EquipmentResponsibleHistory();
        public readonly EquipmentResponsibleHistoryService _equipmentResponsibleHistoryService = new EquipmentResponsibleHistoryService();
        public Add(int Id, int eId)
        {
            InitializeComponent();
            respId = Id;
            eqId = eId;

            foreach (var i in UserSession.DropdownData.Users)
            {
                var cbx = new ComboBoxItem
                {
                    Content = i.DisplayText,
                    Tag = i.Id
                };
                cbxResponsible.Items.Add(cbx);
            }
            foreach (var i in UserSession.DropdownData.Users)
            {
                var cbx = new ComboBoxItem
                {
                    Content = i.DisplayText,
                    Tag = i.Id
                };
                if (i.Id == UserSession.UserId) cbx.Content += " (вы)";
                cbxAssignedBy.Items.Add(cbx);
            }

            foreach (ComboBoxItem i in cbxResponsible.Items)
            {
                if (i.Tag is int TagId && TagId == respId)
                {
                    cbxResponsible.SelectedItem = i;
                    break;
                }
            }
            foreach (ComboBoxItem i in cbxAssignedBy.Items)
            {
                if (i.Tag is int TagId && TagId == UserSession.UserId)
                {
                    cbxAssignedBy.SelectedItem = i;
                    break;
                }
            }

            dpDate.SelectedDate = DateTime.Now;
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
            var temp = new Models.EquipmentResponsibleHistory();
            temp.EquipmentId = eqId;
            if (string.IsNullOrWhiteSpace(comment.Text))
            {
                temp.Comment = null;
            }
            else temp.Comment = comment.Text;

            ComboBoxItem item = new ComboBoxItem();
            item = (ComboBoxItem)cbxResponsible.SelectedItem;
            if (item.Tag.ToString() != "empty")
                temp.ResponsibleUserId = Convert.ToInt32(item.Tag);
            else
            {
                MessageBox.Show("Ответственный обязателено для заполнения", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            item = (ComboBoxItem)cbxAssignedBy.SelectedItem;
            if (item.Tag.ToString() != "empty")
                temp.AssignedByUserId = Convert.ToInt32(item.Tag);
            else
            {
                MessageBox.Show("Прикрепил обязателено для заполнения", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if(dpDate.SelectedDate != null)
            {
                temp.AssignedAt = dpDate.SelectedDate.Value.Date + DateTime.Now.TimeOfDay;
            }
            else
            {
                temp.AssignedAt = null;
            }

            direction = temp;

            CreateDirection();
            MessageBox.Show("Оборудование закреплено", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            Main.init.LoadTable();
            this.Close();
        }

        private async void CreateDirection()
        {
            await _equipmentResponsibleHistoryService.CreateEquipmentResponsibleHistoryAsync(direction);
        }
    }
}
