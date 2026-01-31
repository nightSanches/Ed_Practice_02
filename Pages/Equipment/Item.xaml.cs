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
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using EquipmentAccounting.Models;

namespace EquipmentAccounting.Pages.Equipment
{
    /// <summary>
    /// Логика взаимодействия для Item.xaml
    /// </summary>
    public partial class Item : UserControl
    {
        EquipmentPage main;
        Models.Equipment equipment;
        public Item(Models.Equipment equipment, EquipmentPage main)
        {
            InitializeComponent();
            this.main = main;
            this.equipment = equipment;

            txtName.Text = "(" + equipment.Id + ") " + equipment.Name;
            txtModel.Text = UserSession.DropdownData.Models.FirstOrDefault(item => item.Id == equipment.ModelId).DisplayText;
            txtStatus.Text = UserSession.DropdownData.Statuses.FirstOrDefault(item => item.Id == equipment.StatusId).DisplayText;
            txtComment.Text = equipment.Comment;
            txtDirection.Text = UserSession.DropdownData.Directions.FirstOrDefault(item => item.Id == equipment.DirectionId).DisplayText;
            txtInventoryNumber.Text = equipment.InventoryNumber.ToString();
            txtRoom.Text = UserSession.DropdownData.Rooms.FirstOrDefault(item => item.Id == equipment.RoomId).DisplayText;
            txtCost.Text = equipment.Cost.ToString();

            LoadPhotoFromBase64();
        }
        private void LoadPhotoFromBase64()
        {
            try
            {
                if (!string.IsNullOrEmpty(equipment.Photo))
                {
                    byte[] imageBytes = Convert.FromBase64String(equipment.Photo);

                    using (MemoryStream memoryStream = new MemoryStream(imageBytes))
                    {
                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.StreamSource = memoryStream;
                        bitmapImage.EndInit();
                        bitmapImage.Freeze();

                        imgPhoto.Source = bitmapImage;
                    }
                }
                else
                {
                    imgPhoto.Source = new BitmapImage(new Uri("/Images/default-equipment.png", UriKind.Relative));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки изображения: {ex.Message}");
                imgPhoto.Source = new BitmapImage(new Uri("/Images/default-equipment.png", UriKind.Relative));
            }
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
        }

        private void OnLeave(object sender, MouseEventArgs e)
        {
            itemBorder.Background = (Brush)new BrushConverter().ConvertFrom("#F8F9FA");
        }
    }
}
