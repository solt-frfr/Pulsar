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
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using AemulusModManager;
using AemulusModManager.Utilities;

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
            ModsWindow(true);
            SettingsWindow.Visibility = Visibility.Collapsed;
            AssignWindow.Visibility = Visibility.Collapsed;
            DownloadWindow.Visibility = Visibility.Collapsed;
            outputter = new TextBoxOutputter(sw);
            Directory.CreateDirectory($@"{Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Mods");
            outputter.WriteEvent += consoleWriter_WriteEvent;
            outputter.WriteLineEvent += consoleWriter_WriteLineEvent;
            Console.SetOut(outputter);
            Refresh();
        }
        private string[] CountFolders(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                string[] directories = Directory.GetDirectories(folderPath);
                int folderCount = directories.Length;
                ParallelLogger.Log($"[INFO] Number of mod folders: {folderCount}");
                return directories;
            }
            else
            {
                ParallelLogger.Log("[ERROR] The specified folder does not exist.");
                return null;
            }
        }
        private void Refresh()
        {
            ModDataGrid.Items.Clear();
            string path = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Mods";
            string[] griditems = CountFolders(path);
            foreach (string modpath in griditems)
            {
                ParallelLogger.Log($"[INFO] Found folder {modpath}");
                Meta mod = new Meta();
                string filepath = modpath + $@"\meta.json";
                if (File.Exists(filepath))
                {
                    var jsonoptions = new JsonSerializerOptions
                    {
                        WriteIndented = true
                    };
                    string jsonString = File.ReadAllText(filepath);
                    mod = JsonSerializer.Deserialize<Meta>(jsonString, jsonoptions);
                    if (path + $@"\{mod.ID}" == modpath)
                        ModDataGrid.Items.Add(mod);
                    else
                        ParallelLogger.Log($"[ERROR] Folder and ID mismatch. Mod ''{mod.Name}'' will not be used.");
                }
            }
        }
        private void ScrollToBottom(object sender, TextChangedEventArgs args)
        {
            ConsoleOutput.ScrollToEnd();
        }
        private void New_OnClick(object sender, RoutedEventArgs e)
        {
            MakePack newpack = new MakePack(new Meta());
            newpack.ShowDialog();
        }

        private void Edit_OnClick(object sender, RoutedEventArgs e)
        {
            Meta row = (Meta)ModDataGrid.SelectedItem;
            MakePack edit = new MakePack(row);
            edit.ShowDialog();
        }

        private void Folder_OnClick(object sender, RoutedEventArgs e)
        {
            foreach (var item in ModDataGrid.SelectedItems)
            {
                Meta row = (Meta)item;
                if (row != null)
                {
                    if (Directory.Exists($@"{Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Mods\{row.ID}"))
                    {
                        try
                        {
                            ProcessStartInfo StartInformation = new ProcessStartInfo();
                            StartInformation.FileName = $@"{Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Mods\{row.ID}";
                            StartInformation.UseShellExecute = true;
                            Process process = Process.Start(StartInformation);
                            ParallelLogger.Log($@"[INFO] Opened \Mods\{row.ID}.");
                        }
                        catch (Exception ex)
                        {
                            ParallelLogger.Log($@"[ERROR] Couldn't open \Mods\{row.ID} ({ex.Message})");
                        }
                    }
                }
            }
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
            try
            {
                if (string.IsNullOrWhiteSpace(row.Description))
                    DescBox.Text = "Quasar never worked for me, so I made my own. You're seeing this because this mod has no description, or no mod is selected.\n\nDon't see a mod? The ID and folder names must match.";
                else
                    DescBox.Text = row.Description;
            }
            catch
            {
                DescBox.Text = "Quasar never worked for me, so I made my own. You're seeing this because this mod has no description, or no mod is selected.\n\nDon't see a mod? The ID and folder names must match.";
            }
            try
            {
                if (File.Exists($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Mods\{row.ID}\preview.png"))
                {
                    Preview.Source = new BitmapImage(new Uri($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Mods\{row.ID}\Preview.png", UriKind.Absolute));
                }
                else
                {
                    Preview.Source = new BitmapImage(new Uri("/Images/Preview.png", UriKind.Relative));
                }
            }
            catch
            {
                Preview.Source = new BitmapImage(new Uri("/Images/Preview.png", UriKind.Relative));
            }
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
            ModsWindow(true);
            SettingsWindow.Visibility = Visibility.Collapsed;
            AssignWindow.Visibility = Visibility.Collapsed;
            DownloadWindow.Visibility = Visibility.Collapsed;
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
            ModsWindow(false);
            SettingsWindow.Visibility = Visibility.Collapsed;
            AssignWindow.Visibility = Visibility.Visible;
            DownloadWindow.Visibility = Visibility.Collapsed;
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
            ModsWindow(false);
            SettingsWindow.Visibility = Visibility.Visible;
            AssignWindow.Visibility = Visibility.Collapsed;
            DownloadWindow.Visibility = Visibility.Collapsed;
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
            ModsWindow(false);
            SettingsWindow.Visibility = Visibility.Collapsed;
            AssignWindow.Visibility = Visibility.Collapsed;
            DownloadWindow.Visibility = Visibility.Visible;
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

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        private void Deploy_Click(object sender, RoutedEventArgs e)
        {
            ParallelLogger.Log("[INFO] If this was properly set up, it would deploy your mods.");
        }

        private void ModsWindow(bool sender)
        {
            if (sender == false)
            {
                Mods.Visibility = Visibility.Collapsed;
                SearchSort.Visibility = Visibility.Collapsed;
                ModContent.Visibility = Visibility.Collapsed;
            }
            else
            {
                Mods.Visibility = Visibility.Visible;
                SearchSort.Visibility = Visibility.Visible;
                ModContent.Visibility = Visibility.Visible;
            }
        }
    }
}
