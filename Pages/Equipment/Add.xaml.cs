using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using EquipmentAccounting.Models;
using EquipmentAccounting.Services;
using Microsoft.Win32;

namespace EquipmentAccounting.Pages.Equipment
{
    /// <summary>
    /// Логика взаимодействия для Add.xaml
    /// </summary>
    /// 
    public partial class Add : Page
    {
        public readonly EquipmentService _equipmentService = new EquipmentService();
        Models.Equipment equipment;
        public Add(Models.Equipment equipment = null)
        {
            InitializeComponent();
            this.equipment = equipment;

            foreach (var i in UserSession.DropdownData.Rooms)
            {
                var cbx = new ComboBoxItem
                {
                    Content = i.DisplayText,
                    Tag = i.Id
                };
                room.Items.Add(cbx);
            }

            foreach (var i in UserSession.DropdownData.Users)
            {
                var cbx = new ComboBoxItem
                {
                    Content = i.DisplayText,
                    Tag = i.Id
                };
                responsible.Items.Add(cbx);
            }

            foreach (var i in UserSession.DropdownData.Users)
            {
                var cbx = new ComboBoxItem
                {
                    Content = i.DisplayText,
                    Tag = i.Id
                };
                temp_responsible.Items.Add(cbx);
            }

            foreach (var i in UserSession.DropdownData.Directions)
            {
                var cbx = new ComboBoxItem
                {
                    Content = i.DisplayText,
                    Tag = i.Id
                };
                direction.Items.Add(cbx);
            }

            foreach (var i in UserSession.DropdownData.Statuses)
            {
                var cbx = new ComboBoxItem
                {
                    Content = i.DisplayText,
                    Tag = i.Id
                };
                status.Items.Add(cbx);
            }

            foreach (var i in UserSession.DropdownData.Models)
            {
                var cbx = new ComboBoxItem
                {
                    Content = i.DisplayText,
                    Tag = i.Id
                };
                model.Items.Add(cbx);
            }
            id.Content = "";
            if (this.equipment != null) {
                btnSoftware.Visibility = Visibility.Visible;
                btnSave.Content = "Изменить";
                txtTitle.Text = "Изменение оборудования";
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

                if(equipment.Photo != null)
                {
                    LoadPhotoFromBase64();
                }
            }
        }
        private void btnCancelClick(object sender, RoutedEventArgs e)
        {
            MainWindow.init.OpenPages(new Pages.Equipment.EquipmentPage());
        }

        private async void btnAddClick(object sender, RoutedEventArgs e)
        {
            //Стоимость должна содержать только цифры прим. 1000,50
            if (equipment == null)
            {
                var tempEquipment = new Models.Equipment();

                //обязательно для заполнения
                if(string.IsNullOrWhiteSpace(name.Text))
                {
                    MessageBox.Show("Наименование обязательно для заполнения", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else if(name.Text.Length > 200)
                {
                    MessageBox.Show("Наименование максимум 200 символов", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else
                    tempEquipment.Name = name.Text;

                if (string.IsNullOrWhiteSpace(inventory_number.Text))
                {
                    MessageBox.Show("Инвентарный номер обязателен для заполнения", "Ошибка.", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else if(!Regex.IsMatch(inventory_number.Text, @"^\d+$"))
                {
                    MessageBox.Show("Инвентарный номер должен содержать только цифры", "Ошибка.", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else if (Convert.ToInt32(inventory_number.Text) <= 0)
                {
                    MessageBox.Show("Инвентарный номер должен быть больше нуля", "Ошибка.", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else 
                {
                    var allEquipment = await EquipmentPage.init._equipmentService.GetEquipmentAsync();
                    if (allEquipment != null && allEquipment.Count > 0)
                    {
                        foreach (var equipment in allEquipment)
                        {
                            if(equipment.InventoryNumber.ToString() == inventory_number.Text)
                            {
                                MessageBox.Show("Инвентарный номер должен быть уникальным", "Ошибка.", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                        }
                    }
                    tempEquipment.InventoryNumber = Convert.ToInt32(inventory_number.Text);
                }

                if(tempImageBase64 != null)
                    tempEquipment.Photo = tempImageBase64;

                if (!string.IsNullOrWhiteSpace(cost.Text))
                {
                    if (!Regex.IsMatch(cost.Text, @"^\d+(\,\d+)?$"))
                    {
                        MessageBox.Show("Неправильный формат стоимости (прим. 1000 или 1000,15)", "Ошибка.", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    else if (Convert.ToDecimal(cost.Text) < 0)
                    {
                        MessageBox.Show("Стоимость не может быть отрицательной", "Ошибка.", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    tempEquipment.Cost = Convert.ToDecimal(cost.Text);
                }
                else tempEquipment.Cost = null;

                if (!string.IsNullOrWhiteSpace(comment.Text))
                    tempEquipment.Comment = comment.Text;
                else tempEquipment.Comment = null;

                ComboBoxItem item = new ComboBoxItem();
                item = (ComboBoxItem)room.SelectedItem;
                if (item.Tag.ToString() != "empty")
                    tempEquipment.RoomId = Convert.ToInt32(item.Tag);
                else tempEquipment.RoomId = null;

                item = (ComboBoxItem)responsible.SelectedItem;
                if (item.Tag.ToString() != "empty")
                    tempEquipment.ResponsibleUserId = Convert.ToInt32(item.Tag);
                else tempEquipment.ResponsibleUserId = null;

                item = (ComboBoxItem)temp_responsible.SelectedItem;
                if (item.Tag.ToString() != "empty")
                    tempEquipment.TempResponsibleUserId = Convert.ToInt32(item.Tag);
                else tempEquipment.TempResponsibleUserId = null;

                item = (ComboBoxItem)direction.SelectedItem;
                if (item.Tag.ToString() != "empty")
                    tempEquipment.DirectionId = Convert.ToInt32(item.Tag);
                else tempEquipment.DirectionId = null;

                item = (ComboBoxItem)status.SelectedItem;
                if (item.Tag.ToString() != "empty")
                    tempEquipment.StatusId = Convert.ToInt32(item.Tag);
                else tempEquipment.StatusId = null;

                item = (ComboBoxItem)model.SelectedItem;
                if (item.Tag.ToString() != "empty")
                    tempEquipment.ModelId = Convert.ToInt32(item.Tag);
                else tempEquipment.ModelId = null;

                equipment = tempEquipment;

                CreateEquipment();
                MessageBox.Show("Оборудование сохранено", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                MainWindow.init.OpenPages(new Pages.Equipment.EquipmentPage());
            }
            else
            {
                //обязательно для заполнения
                if (string.IsNullOrWhiteSpace(name.Text))
                {
                    MessageBox.Show("Наименование обязательно для заполнения", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else if (name.Text.Length > 200)
                {
                    MessageBox.Show("Наименование максимум 200 символов", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else
                    equipment.Name = name.Text;

                if (string.IsNullOrWhiteSpace(inventory_number.Text))
                {
                    MessageBox.Show("Инвентарный номер обязателен для заполнения", "Ошибка.", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else if (!Regex.IsMatch(inventory_number.Text, @"^\d+$"))
                {
                    MessageBox.Show("Инвентарный номер должен содержать только цифры", "Ошибка.", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else if (Convert.ToInt32(inventory_number.Text) <= 0)
                {
                    MessageBox.Show("Инвентарный номер должен быть больше нуля", "Ошибка.", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else
                {
                    var allEquipment = await EquipmentPage.init._equipmentService.GetEquipmentAsync();
                    if (allEquipment != null && allEquipment.Count > 0)
                    {
                        foreach (var equipment in allEquipment)
                        {
                            if (equipment.InventoryNumber.ToString() == inventory_number.Text)
                            {
                                if(equipment.InventoryNumber != equipment.InventoryNumber)
                                {
                                    MessageBox.Show("Инвентарный номер должен быть уникальным", "Ошибка.", MessageBoxButton.OK, MessageBoxImage.Error);
                                    return;
                                }
                            }
                        }
                    }
                    equipment.InventoryNumber = Convert.ToInt32(inventory_number.Text);
                }
                if (tempImageBase64 != null)
                    equipment.Photo = tempImageBase64;

                if (!string.IsNullOrWhiteSpace(cost.Text))
                {
                    if (!Regex.IsMatch(cost.Text, @"^\d+(\,\d+)?$"))
                    {
                        MessageBox.Show("Неправильный формат стоимости (прим. 1000 или 1000,15)", "Ошибка.", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    else if (Convert.ToDecimal(cost.Text) < 0)
                    {
                        MessageBox.Show("Стоимость не может быть отрицательной", "Ошибка.", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    equipment.Cost = Convert.ToDecimal(cost.Text);
                }
                else equipment.Cost = null;

                if (!string.IsNullOrWhiteSpace(comment.Text))
                    equipment.Comment = comment.Text;
                else equipment.Comment = null;

                ComboBoxItem item = new ComboBoxItem();
                item = (ComboBoxItem)room.SelectedItem;
                if (item.Tag.ToString() != "empty")
                    equipment.RoomId = Convert.ToInt32(item.Tag);
                else equipment.RoomId = null;

                item = (ComboBoxItem)responsible.SelectedItem;
                if (item.Tag.ToString() != "empty")
                    equipment.ResponsibleUserId = Convert.ToInt32(item.Tag);
                else equipment.ResponsibleUserId = null;

                item = (ComboBoxItem)temp_responsible.SelectedItem;
                if (item.Tag.ToString() != "empty")
                    equipment.TempResponsibleUserId = Convert.ToInt32(item.Tag);
                else equipment.TempResponsibleUserId = null;

                item = (ComboBoxItem)direction.SelectedItem;
                if (item.Tag.ToString() != "empty")
                    equipment.DirectionId = Convert.ToInt32(item.Tag);
                else equipment.DirectionId = null;

                item = (ComboBoxItem)status.SelectedItem;
                if (item.Tag.ToString() != "empty")
                    equipment.StatusId = Convert.ToInt32(item.Tag);
                else equipment.StatusId = null;

                item = (ComboBoxItem)model.SelectedItem;
                if (item.Tag.ToString() != "empty")
                    equipment.ModelId = Convert.ToInt32(item.Tag);
                else equipment.ModelId = null;

                UpdateEquipment();
                MessageBox.Show("Оборудование обновлено", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                MainWindow.init.OpenPages(new Pages.Equipment.EquipmentPage());
            }
        }

        private async void CreateEquipment()
        {
            await _equipmentService.CreateEquipmentAsync(equipment);
        }

        private async void UpdateEquipment()
        {
            await _equipmentService.UpdateEquipmentAsync(equipment.Id, equipment);
        }
        private string tempImageBase64 = null;
        private void btnLoadImage(object sender, RoutedEventArgs e)
        {
            tempImageBase64 = GetImageBase64FromDialog();
            LoadPhotoFromBase64();
        }

        public static string GetImageBase64FromDialog()
        {
            try
            {
                var openFileDialog = new OpenFileDialog
                {
                    Title = "Выберите изображение(Максимум 16 Мб)",
                    Filter = "Изображения (*.jpg; *.jpeg; *.png)|*.jpg;*.jpeg;*.png|Все файлы (*.*)|*.*",
                    Multiselect = false
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    string path = openFileDialog.FileName;
                    if(!File.Exists(path))
                    {
                        throw new FileNotFoundException("Файл не найден", path);
                    }

                    var fileInfo = new FileInfo(path);

                    if(fileInfo.Length > (16*1024*1024))
                    {
                        throw new InvalidOperationException($"Размер файла превышает максимально допустимый ({16*1024*1024} Мб)");
                    }

                    byte[] imageBytes = File.ReadAllBytes(path);
                    string base64String =  Convert.ToBase64String(imageBytes);
                    return base64String;
                }
                return null;
            }
            catch(Exception ex)
            {
                throw;
            }
        }
        private void LoadPhotoFromBase64()
        {
            try
            {
                if (!string.IsNullOrEmpty(tempImageBase64))
                {
                    byte[] imageBytes = Convert.FromBase64String(tempImageBase64);

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
                else if(!string.IsNullOrEmpty(equipment.Photo))
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
        private void ImageDeleteClick(object sender, MouseButtonEventArgs e)
        {
            if(equipment != null)
            {
                if (equipment.Photo != null)
                {
                    MessageBoxResult msgBoxResult = MessageBox.Show("Удалить изображение?", "Удаление изображения", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (msgBoxResult == MessageBoxResult.Yes)
                    {
                        equipment.Photo = null;
                        imgPhoto.Source = new BitmapImage(new Uri("/Images/default-equipment.png", UriKind.Relative));
                    }
                }
            }
            else if(imgPhoto.Source.ToString() != "pack://application:,,,/Images/default-equipment.png")
            {
                MessageBoxResult msgBoxResult = MessageBox.Show("Удалить изображение?", "Удаление изображения", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (msgBoxResult == MessageBoxResult.Yes)
                {
                    imgPhoto.Source = new BitmapImage(new Uri("/Images/default-equipment.png", UriKind.Relative));
                }
            }
        }

        private void btnOpenSoftware(object sender, RoutedEventArgs e)
        {
            MainWindow.init.OpenPages(new Pages.EquipmentSoftware.Main(this.equipment));
        }
    }
}
