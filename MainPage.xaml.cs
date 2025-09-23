using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Phone.Controls;

namespace VirtuWP
{
    public partial class MainPage : PhoneApplicationPage
    {
        private Dictionary<string, VirtualMachine> vms = new Dictionary<string, VirtualMachine>();
        private VirtualMachine activeVm = null;
        private string selectedOs = "";

        public MainPage()
        {
            InitializeComponent();
            LoadExistingVms();
        }

        // -------------------- LOAD VMS --------------------
        private void LoadExistingVms()
        {
            VmList.Items.Clear();
            using (IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication())
            {
                foreach (var f in iso.GetFileNames("*.wpvm"))
                {
                    string vmName = f.Replace(".wpvm", "");
                    try
                    {
                        VirtualMachine vm = VirtualMachine.LoadVm(vmName);
                        vms[vmName] = vm;
                        VmList.Items.Add(new { Name = vmName });
                    }
                    catch
                    {
                        MessageBox.Show($"VM \"{vmName}\" is corrupted.");
                        // Do NOT delete
                    }
                }
            }
        }

        // -------------------- NEW VM --------------------
        private void BtnNewVm_Click(object sender, RoutedEventArgs e) => NewVmMenu.Visibility = Visibility.Visible;
        private void BtnCancelNewVm_Click(object sender, RoutedEventArgs e)
        {
            NewVmMenu.Visibility = Visibility.Collapsed;
            TxtVmName.Text = "";
            selectedOs = "";
        }
        private void BtnLinux_Click(object sender, RoutedEventArgs e) => selectedOs = "linux";
        private void BtnKolibriOS_Click(object sender, RoutedEventArgs e) => selectedOs = "kolibrios";

        private void CreateVm()
        {
            string name = TxtVmName.Text.Trim();
            if (string.IsNullOrEmpty(name)) { MessageBox.Show("Enter a VM name"); return; }
            if (vms.ContainsKey(name)) { MessageBox.Show($"A VM with the name \"{name}\" already exists."); return; }
            if (string.IsNullOrEmpty(selectedOs)) { MessageBox.Show("Select an OS"); return; }

            using (IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication())
            {
                // Total VM storage
                long totalSize = 0;
                foreach (var file in iso.GetFileNames("*.wpvm"))
                {
                    using (var fs = iso.OpenFile(file, FileMode.Open)) totalSize += fs.Length;
                }
                if (totalSize >= 50 * 1024 * 1024)
                {
                    MessageBox.Show("Cannot create new VM. Total VM storage limit (50MB) reached.");
                    return;
                }
            }

            VirtualMachine vm = new VirtualMachine(name, selectedOs);
            vm.SaveVm();

            // Check size limit per VM
            using (IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication())
            {
                string fileName = name.ToLower().Replace(" ", "") + ".wpvm";
                using (var fs = iso.OpenFile(fileName, FileMode.Open))
                {
                    if (fs.Length >= 25 * 1024 * 1024)
                    {
                        MessageBox.Show($"Cannot create VM \"{name}\". VM size is 25MB or larger.");
                        return;
                    }
                }
            }

            vms[name] = vm;
            VmList.Items.Add(new { Name = name });

            NewVmMenu.Visibility = Visibility.Collapsed;
            TxtVmName.Text = "";
            selectedOs = "";
        }

        // -------------------- OPEN VM --------------------
        private void OpenVm(VirtualMachine vm)
        {
            string fileName = vm.Name.ToLower().Replace(" ", "") + ".wpvm";
            using (IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!iso.FileExists(fileName)) return;

                using (var fs = iso.OpenFile(fileName, FileMode.Open))
                {
                    if (fs.Length >= 25 * 1024 * 1024)
                    {
                        MessageBox.Show($"Cannot run VM \"{vm.Name}\". VM size is 25MB or larger.");
                        return;
                    }

                    long totalSize = 0;
                    foreach (var file in iso.GetFileNames("*.wpvm"))
                    {
                        using (var f = iso.OpenFile(file, FileMode.Open))
                            totalSize += f.Length;
                    }
                    if (totalSize >= 50 * 1024 * 1024)
                    {
                        MessageBox.Show("Cannot run VM. Total VM storage limit (50MB) reached.");
                        return;
                    }
                }
            }

            activeVm = vm;
            VmList.Visibility = Visibility.Collapsed;
            BtnNewVm.Visibility = Visibility.Collapsed;
            BtnExitVm.Visibility = Visibility.Visible;
            ShellOutput.Visibility = Visibility.Visible;
            ShellOutputScroll.Visibility = Visibility.Visible;
            ShellInput.Visibility = Visibility.Visible;
            ShellInput.Focus();
            ShellOutput.Text = vm.GetPrompt() + " ";
        }

        private void BtnExitVm_Click(object sender, RoutedEventArgs e)
        {
            activeVm = null;
            VmList.Visibility = Visibility.Visible;
            BtnNewVm.Visibility = Visibility.Visible;
            BtnExitVm.Visibility = Visibility.Collapsed;
            ShellOutput.Visibility = Visibility.Collapsed;
            ShellOutputScroll.Visibility = Visibility.Collapsed;
            ShellInput.Visibility = Visibility.Collapsed;
            ShellInput.Text = "";
        }

        private void ShellInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && activeVm != null)
            {
                string command = ShellInput.Text;
                ShellInput.Text = "";
                string output = activeVm.RunCommand(command);
                ShellOutput.Text += output + "\n" + activeVm.GetPrompt() + " ";
                ShellOutputScroll.ScrollToVerticalOffset(ShellOutputScroll.ExtentHeight);
            }
        }

        private void LayoutRoot_Tap(object sender, GestureEventArgs e)
        {
            if (ShellInput.Visibility == Visibility.Visible)
                ShellInput.Focus();
        }

        private void VmItem_Tap(object sender, GestureEventArgs e)
        {
            StackPanel panel = sender as StackPanel;
            if (panel == null) return;

            TextBlock tb = null;
            foreach (var child in panel.Children)
                if (child is TextBlock) tb = child as TextBlock;

            if (tb == null) return;

            string vmName = tb.Text;
            if (vms.ContainsKey(vmName))
            {
                OpenVm(vms[vmName]);
            }
            else
            {
                MessageBox.Show($"VM \"{vmName}\" not found or corrupted.");
            }
        }
    }
}
