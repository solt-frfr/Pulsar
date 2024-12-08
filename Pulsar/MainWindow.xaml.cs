using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Packaging;
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
using AemulusModManager;

namespace Pulsar
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private StreamWriter sw;
        private TextBoxOutputter outputter;

        public MainWindow(bool running, bool oneClick)
        {
            InitializeComponent();
            sw = new StreamWriter(
                new FileStream(
                    $@"{Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Pulsar.log",
                    FileMode.Append,
                    FileAccess.Write,
                    FileShare.ReadWrite // Allow other processes to read/write
                ),
                Encoding.UTF8
            );

            outputter = new TextBoxOutputter(sw);
            Directory.CreateDirectory($@"{Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Mods");
            outputter.WriteEvent += consoleWriter_WriteEvent;
            outputter.WriteLineEvent += consoleWriter_WriteLineEvent;
            Console.SetOut(outputter);
        }
        private void ScrollToBottom(object sender, TextChangedEventArgs args)
        {
            ConsoleOutput.ScrollToEnd();
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
            Process.Start($@"{Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}");
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
            var downloadImage = (Image)DownloadButton.Template.FindName("DownloadImage", DownloadButton);
            if (downloadImage != null)
            {
                downloadImage.Source = new BitmapImage(new Uri("/Images/DownloadUnsel.png", UriKind.Relative));
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
            var downloadImage = (Image)DownloadButton.Template.FindName("DownloadImage", DownloadButton);
            if (downloadImage != null)
            {
                downloadImage.Source = new BitmapImage(new Uri("/Images/DownloadUnsel.png", UriKind.Relative));
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
            var downloadImage = (Image)DownloadButton.Template.FindName("DownloadImage", DownloadButton);
            if (downloadImage != null)
            {
                downloadImage.Source = new BitmapImage(new Uri("/Images/DownloadUnsel.png", UriKind.Relative));
            }
            Mods.Visibility = Visibility.Collapsed;
            SearchSort.Visibility = Visibility.Collapsed;
            ModContent.Visibility = Visibility.Collapsed;
        }

        private void Download_Click(object sender, RoutedEventArgs e)
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
                settingsImage.Source = new BitmapImage(new Uri("/Images/SettingsUnsel.png", UriKind.Relative));
            }
            var downloadImage = (Image)DownloadButton.Template.FindName("DownloadImage", DownloadButton);
            if (downloadImage != null)
            {
                downloadImage.Source = new BitmapImage(new Uri("/Images/DownloadSel.png", UriKind.Relative));
            }
            Mods.Visibility = Visibility.Collapsed;
            SearchSort.Visibility = Visibility.Collapsed;
            ModContent.Visibility = Visibility.Collapsed;
            AemulusModManager.Utilities.ParallelLogger.Log("[DEBUG] This is a log attempt. Did it work?");
            AemulusModManager.Utilities.ParallelLogger.Log("[ERROR] This is an error attempt. Did it work?");
            AemulusModManager.Utilities.ParallelLogger.Log("[INFO] This is an info attempt. Did it work?");
        }
        void consoleWriter_WriteLineEvent(object sender, ConsoleWriterEventArgs e)
        {
            Console.Write(e.Value + "\n");
        }

        void consoleWriter_WriteEvent(object sender, ConsoleWriterEventArgs e)
        {
            string text = (string)e.Value;
            this.Dispatcher.Invoke(() =>
            {
                if (text.Contains("[INFO]"))
                    ConsoleOutput.AppendText(text, "#52FF00");
                else if (text.Contains("[WARNING]"))
                    ConsoleOutput.AppendText(text, "#FFFF00");
                else if (text.Contains("[ERROR]"))
                    ConsoleOutput.AppendText(text, "#FFB0B0");
                else
                    ConsoleOutput.AppendText(text, "#F2F2F2");

            });
        }
    }
}
