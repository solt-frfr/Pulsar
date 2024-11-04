using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
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

namespace Pulsar
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var jsonoptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            ConsoleBox.Text = File.ReadAllText($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\log.json");
        }
        
        private void New_OnClick(object sender, RoutedEventArgs e)
        {
            MakePack newpack = new MakePack();
            newpack.ShowDialog();
        }

        private void Edit_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Folder_OnClick(object sender, RoutedEventArgs e)
        {
            Process.Start($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}");
        }

        private void Assign_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Zip_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Delete_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void currentrow(object sender, SelectionChangedEventArgs e)
        {
            Meta row = (Meta)ModDataGrid.SelectedItem;
        }

        private void Mods_Click(object sender, RoutedEventArgs e)
        {
            var modsImage = (Image)ModsButton.Template.FindName("ModsImage", ModsButton);
            if (modsImage != null)
            {
                modsImage.Source = new BitmapImage(new Uri("/Images/ModsSel.png", UriKind.Relative));
            }
            var assignImage = (Image)AssignButton.Template.FindName("AssignImage", AssignButton);
            if (assignImage != null)
            {
                assignImage.Source = new BitmapImage(new Uri("/Images/AssignUnsel.png", UriKind.Relative));
            }
            var settingsImage = (Image)SettingsButton.Template.FindName("SettingsImage", SettingsButton);
            if (settingsImage != null)
            {
                settingsImage.Source = new BitmapImage(new Uri("/Images/SettingsUnsel.png", UriKind.Relative));
            }
            Mods.Visibility = Visibility.Visible;
            SearchSort.Visibility = Visibility.Visible;
            ModContent.Visibility = Visibility.Visible;
        }
        private void Assign_Click(object sender, RoutedEventArgs e)
        {
            var modsImage = (Image)ModsButton.Template.FindName("ModsImage", ModsButton);
            if (modsImage != null)
            {
                modsImage.Source = new BitmapImage(new Uri("/Images/ModsUnsel.png", UriKind.Relative));
            }
            var assignImage = (Image)AssignButton.Template.FindName("AssignImage", AssignButton);
            if (assignImage != null)
            {
                assignImage.Source = new BitmapImage(new Uri("/Images/AssignSel.png", UriKind.Relative));
            }
            var settingsImage = (Image)SettingsButton.Template.FindName("SettingsImage", SettingsButton);
            if (settingsImage != null)
            {
                settingsImage.Source = new BitmapImage(new Uri("/Images/SettingsUnsel.png", UriKind.Relative));
            }
            Mods.Visibility = Visibility.Collapsed;
            SearchSort.Visibility = Visibility.Collapsed;
            ModContent.Visibility = Visibility.Collapsed;
        }
        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            var modsImage = (Image)ModsButton.Template.FindName("ModsImage", ModsButton);
            if (modsImage != null)
            {
                modsImage.Source = new BitmapImage(new Uri("/Images/ModsUnsel.png", UriKind.Relative));
            }
            var assignImage = (Image)AssignButton.Template.FindName("AssignImage", AssignButton);
            if (assignImage != null)
            {
                assignImage.Source = new BitmapImage(new Uri("/Images/AssignUnsel.png", UriKind.Relative));
            }
            var settingsImage = (Image)SettingsButton.Template.FindName("SettingsImage", SettingsButton);
            if (settingsImage != null)
            {
                settingsImage.Source = new BitmapImage(new Uri("/Images/SettingsSel.png", UriKind.Relative));
            }
            Mods.Visibility = Visibility.Collapsed;
            SearchSort.Visibility = Visibility.Collapsed;
            ModContent.Visibility = Visibility.Collapsed;
        }
    }
}
