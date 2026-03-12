using EquipmentAccounting.Models;
using EquipmentAccounting.Pages.Equipment;
using EquipmentAccounting.Services;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
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

namespace EquipmentAccounting.Pages.EquipmentNetwork
{
    public partial class Add : Page
    {
        public readonly NetworkSettingsService _networkService = new NetworkSettingsService();
        Models.NetworkSettings networkSettings;
        Models.Equipment equipmentForBack;
        public Add(Models.Equipment equipment, Models.NetworkSettings networkSettings = null)
        {
            InitializeComponent();
            this.networkSettings = networkSettings;
            this.equipmentForBack = equipment;
            if (this.networkSettings != null)
            {
                btnSave.Content = "Изменить";
                txtTitle.Text = "Изменение сетевой настройки";
                txtIp.Text = this.networkSettings.IpAddress;
                txtMask.Text = this.networkSettings.SubnetMask;
                txtGateway.Text = this.networkSettings.DefaultGateway;
                txtDNS1.Text = this.networkSettings.DnsPrimary;
                txtDNS2.Text = this.networkSettings.DnsSecondary;
                txtMAC.Text = this.networkSettings.MacAddress;
            }
        }
        private void btnCancelClick(object sender, RoutedEventArgs e)
        {
            MainWindow.init.OpenPages(new EquipmentNetwork.Main(equipmentForBack));
        }

        private async void btnAddClick(object sender, RoutedEventArgs e)
        {
            if (networkSettings == null)
            {
                var tempNetworkSettings = new Models.NetworkSettings();

                //обязательно для заполнения
                if (string.IsNullOrWhiteSpace(txtIp.Text))
                {
                    MessageBox.Show("IP обязателен для заполнения", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else
                {
                    if (!Regex.IsMatch(txtIp.Text, @"^(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])\.(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])\.(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])\.(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])$"))
                    {
                        MessageBox.Show("Неверный формат IP.\nИспользуйте формат: XXX.XXX.XXX.XXX\nXXX от 0 до 255", "Ошибка.", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    else
                    {
                        var allNetworkSettings = await Main.init._networkSettingsService.GetNetworkAsync();
                        if (allNetworkSettings != null && allNetworkSettings.Count > 0)
                        {
                            foreach (var settings in allNetworkSettings)
                            {
                                if (settings.IpAddress.ToString() == txtIp.Text)
                                {
                                    MessageBox.Show("Данный IP уже занят", "Ошибка.", MessageBoxButton.OK, MessageBoxImage.Error);
                                    return;
                                }
                            }
                        }
                        tempNetworkSettings.IpAddress = txtIp.Text;
                    }
                }

                if (!string.IsNullOrWhiteSpace(txtMask.Text))
                {
                    if (!Regex.IsMatch(txtMask.Text, @"^(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])\.(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])\.(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])\.(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])$"))
                    {
                        MessageBox.Show("Неверный формат маски подсети.\nИспользуйте формат: XXX.XXX.XXX.XXX\nXXX от 0 до 255", "Ошибка.", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    else
                        tempNetworkSettings.SubnetMask = txtMask.Text;
                }
                else tempNetworkSettings.SubnetMask = null;

                if (!string.IsNullOrWhiteSpace(txtGateway.Text))
                {
                    if (!Regex.IsMatch(txtGateway.Text, @"^(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])\.(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])\.(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])\.(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])$"))
                    {
                        MessageBox.Show("Неверный формат главного шлюза.\nИспользуйте формат: XXX.XXX.XXX.XXX\nXXX от 0 до 255", "Ошибка.", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    else
                        tempNetworkSettings.DefaultGateway = txtGateway.Text;
                }
                else tempNetworkSettings.DefaultGateway = null;

                if (!string.IsNullOrWhiteSpace(txtDNS1.Text))
                {
                    if (!Regex.IsMatch(txtDNS1.Text, @"^(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])\.(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])\.(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])\.(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])$"))
                    {
                        MessageBox.Show("Неверный формат DNS 1.\nИспользуйте формат: XXX.XXX.XXX.XXX\nXXX от 0 до 255", "Ошибка.", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    else
                        tempNetworkSettings.DnsPrimary = txtDNS1.Text;
                }
                else tempNetworkSettings.DnsPrimary = null;

                if (!string.IsNullOrWhiteSpace(txtDNS2.Text))
                {
                    if (!Regex.IsMatch(txtDNS2.Text, @"^(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])\.(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])\.(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])\.(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])$"))
                    {
                        MessageBox.Show("Неверный формат DNS 2.\nИспользуйте формат: XXX.XXX.XXX.XXX\nXXX от 0 до 255", "Ошибка.", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    else tempNetworkSettings.DnsSecondary = txtDNS2.Text;
                }
                else tempNetworkSettings.DnsSecondary = null;

                if (!string.IsNullOrWhiteSpace(txtMAC.Text))
                {
                    if (!Regex.IsMatch(txtMAC.Text, @"^([0-9A-Fa-f]{2}:){5}[0-9A-Fa-f]{2}$"))
                    {
                        MessageBox.Show("Неверный формат MAC-адреса.\nИспользуйте формат: XX:XX:XX:XX:XX:XX (шестнадцатеричные цифры, разделенные двоеточиями)", "Ошибка.", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    else tempNetworkSettings.MacAddress = txtMAC.Text;
                }
                else tempNetworkSettings.MacAddress = null;

                networkSettings = tempNetworkSettings;

                CreateEquipment();
                MessageBox.Show("Сетевые настройки оборудования сохранены", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                MainWindow.init.OpenPages(new Main(equipmentForBack));
            }
            else
            {
                if (string.IsNullOrWhiteSpace(txtIp.Text))
                {
                    MessageBox.Show("IP обязателен для заполнения", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else
                {
                    if (!Regex.IsMatch(txtIp.Text, @"^(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])\.(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])\.(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])\.(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])$"))
                    {
                        MessageBox.Show("Неверный формат IP.\nИспользуйте формат: XXX.XXX.XXX.XXX\nXXX от 0 до 255", "Ошибка.", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    else
                    {
                        var allNetworkSettings = await Main.init._networkSettingsService.GetNetworkAsync();
                        if (allNetworkSettings != null && allNetworkSettings.Count > 0)
                        {
                            foreach (var temp in allNetworkSettings)
                            {
                                if (temp.IpAddress != networkSettings.IpAddress)
                                {
                                    if (temp.IpAddress.ToString() == txtIp.Text)
                                    {
                                        MessageBox.Show("Данный IP уже занят", "Ошибка.", MessageBoxButton.OK, MessageBoxImage.Error);
                                        return;
                                    }
                                }
                            }
                        }
                        networkSettings.IpAddress = txtIp.Text;
                    }
                }

                if (!string.IsNullOrWhiteSpace(txtMask.Text))
                {
                    if (!Regex.IsMatch(txtMask.Text, @"^(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])\.(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])\.(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])\.(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])$"))
                    {
                        MessageBox.Show("Неверный формат маски подсети.\nИспользуйте формат: XXX.XXX.XXX.XXX\nXXX от 0 до 255", "Ошибка.", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    else
                        networkSettings.SubnetMask = txtMask.Text;
                }
                else networkSettings.SubnetMask = null;

                if (!string.IsNullOrWhiteSpace(txtGateway.Text))
                {
                    if (!Regex.IsMatch(txtGateway.Text, @"^(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])\.(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])\.(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])\.(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])$"))
                    {
                        MessageBox.Show("Неверный формат главного шлюза.\nИспользуйте формат: XXX.XXX.XXX.XXX\nXXX от 0 до 255", "Ошибка.", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    else
                        networkSettings.DefaultGateway = txtGateway.Text;
                }
                else networkSettings.DefaultGateway = null;

                if (!string.IsNullOrWhiteSpace(txtDNS1.Text))
                {
                    if (!Regex.IsMatch(txtDNS1.Text, @"^(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])\.(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])\.(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])\.(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])$"))
                    {
                        MessageBox.Show("Неверный формат DNS 1.\nИспользуйте формат: XXX.XXX.XXX.XXX\nXXX от 0 до 255", "Ошибка.", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    else
                        networkSettings.DnsPrimary = txtDNS1.Text;
                }
                else networkSettings.DnsPrimary = null;

                if (!string.IsNullOrWhiteSpace(txtDNS2.Text))
                {
                    if (!Regex.IsMatch(txtDNS2.Text, @"^(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])\.(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])\.(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])\.(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])$"))
                    {
                        MessageBox.Show("Неверный формат DNS 2.\nИспользуйте формат: XXX.XXX.XXX.XXX\nXXX от 0 до 255", "Ошибка.", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    else networkSettings.DnsSecondary = txtDNS2.Text;
                }
                else networkSettings.DnsSecondary = null;

                if (!string.IsNullOrWhiteSpace(txtMAC.Text))
                {
                    if (!Regex.IsMatch(txtMAC.Text, @"^([0-9A-Fa-f]{2}:){5}[0-9A-Fa-f]{2}$"))
                    {
                        MessageBox.Show("Неверный формат MAC-адреса.\nИспользуйте формат: XX:XX:XX:XX:XX:XX (шестнадцатеричные цифры, разделенные двоеточиями)", "Ошибка.", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    else networkSettings.MacAddress = txtMAC.Text;
                }
                else networkSettings.MacAddress = null;

                UpdateEquipment();
                MessageBox.Show("Сетевые настройки оборудования сохранены", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                MainWindow.init.OpenPages(new Main(equipmentForBack));
            }
        }

        private async void CreateEquipment()
        {
            await _networkService.CreateNetworkAsync(networkSettings);
        }

        private async void UpdateEquipment()
        {
            await _networkService.UpdateNetworkAsync(networkSettings.Id, networkSettings);
        }
    }
}
