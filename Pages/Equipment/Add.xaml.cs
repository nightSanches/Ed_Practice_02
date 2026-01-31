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
using EquipmentAccounting.Models;

namespace EquipmentAccounting.Pages.Equipment
{
    /// <summary>
    /// Логика взаимодействия для Add.xaml
    /// </summary>
    public partial class Add : Page
    {
        Models.Equipment equipment;
        public Add(Models.Equipment equipment = null)
        {
            InitializeComponent();
            this.equipment = equipment;

            room.Items.Clear();
            foreach (var i in UserSession.DropdownData.Rooms)
            {
                var cbx = new ComboBoxItem
                {
                    Content = i.DisplayText,
                    Tag = i.Id
                };
                room.Items.Add(cbx);
            }

            responsible.Items.Clear();
            foreach (var i in UserSession.DropdownData.Users)
            {
                var cbx = new ComboBoxItem
                {
                    Content = i.DisplayText,
                    Tag = i.Id
                };
                responsible.Items.Add(cbx);
            }

            temp_responsible.Items.Clear();
            foreach (var i in UserSession.DropdownData.Users)
            {
                var cbx = new ComboBoxItem
                {
                    Content = i.DisplayText,
                    Tag = i.Id
                };
                temp_responsible.Items.Add(cbx);
            }

            direction.Items.Clear();
            foreach (var i in UserSession.DropdownData.Directions)
            {
                var cbx = new ComboBoxItem
                {
                    Content = i.DisplayText,
                    Tag = i.Id
                };
                direction.Items.Add(cbx);
            }

            status.Items.Clear();
            foreach (var i in UserSession.DropdownData.Statuses)
            {
                var cbx = new ComboBoxItem
                {
                    Content = i.DisplayText,
                    Tag = i.Id
                };
                status.Items.Add(cbx);
            }

            model.Items.Clear();
            foreach (var i in UserSession.DropdownData.Models)
            {
                var cbx = new ComboBoxItem
                {
                    Content = i.DisplayText,
                    Tag = i.Id
                };
                model.Items.Add(cbx);
            }

            if (this.equipment != null) {
                id.Content = "Код: " + this.equipment.Id;
                name.Text = this.equipment.Name;
                inventory_number.Text = this.equipment.InventoryNumber.ToString();
                cost.Text = this.equipment.Cost.ToString();
                comment.Text = this.equipment.Comment;
                foreach(ComboBoxItem i in room.Items)
                {
                    if (i.Tag is int TagId && TagId == equipment.RoomId)
                    {
                        room.SelectedItem = i;
                        break;
                    }
                }
                foreach (ComboBoxItem i in responsible.Items)
                {
                    if (i.Tag is int TagId && TagId == equipment.ResponsibleUserId)
                    {
                        responsible.SelectedItem = i;
                        break;
                    }
                }
                foreach (ComboBoxItem i in temp_responsible.Items)
                {
                    if (i.Tag is int TagId && TagId == equipment.TempResponsibleUserId)
                    {
                        temp_responsible.SelectedItem = i;
                        break;
                    }
                }
                foreach (ComboBoxItem i in direction.Items)
                {
                    if (i.Tag is int TagId && TagId == equipment.DirectionId)
                    {
                        direction.SelectedItem = i;
                        break;
                    }
                }
                foreach (ComboBoxItem i in status.Items)
                {
                    if (i.Tag is int TagId && TagId == equipment.StatusId)
                    {
                        status.SelectedItem = i;
                        break;
                    }
                }
                foreach (ComboBoxItem i in model.Items)
                {
                    if (i.Tag is int TagId && TagId == equipment.ModelId)
                    {
                        model.SelectedItem = i;
                        break;
                    }
                }
            }
        }

        private void btnCancelClick(object sender, RoutedEventArgs e)
        {
            MainWindow.init.OpenPages(new Pages.Equipment.EquipmentPage());
        }

        private void btnAddClick(object sender, RoutedEventArgs e)
        {

        }
    }
}
