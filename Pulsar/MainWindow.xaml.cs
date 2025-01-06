using System;
using SevenZip;
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
using System.Windows.Shapes;
using AemulusModManager;
using AemulusModManager.Utilities;
using Microsoft.Win32;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Quasar_Rewrite.Properties;
using SevenZipExtractor;
using System.Windows.Shell;
using System.Runtime;

namespace Pulsar
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        private StreamWriter sw;
        private TextBoxOutputter outputter;
        private List<string> enabledmods = new List<string>();

        public MainWindow(bool running, bool oneClick)
        {
            InitializeComponent();
            sw = new StreamWriter(
                new FileStream(
                    $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Pulsar.log",
                    FileMode.Append,
                    FileAccess.Write,
                    FileShare.ReadWrite
                ),
                Encoding.UTF8
            );
            ModsWindow(true);
            SettingsWindow.Visibility = Visibility.Collapsed;
            AssignWindow.Visibility = Visibility.Collapsed;
            DownloadWindow.Visibility = Visibility.Collapsed;
            outputter = new TextBoxOutputter(sw);
            Directory.CreateDirectory($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Mods");
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
                return directories;
            }
            else
            {
                return null;
            }
        }

        public string CreateLinkImage(string link)
        {
            if (link.Contains("gamebanana.com"))
            {
                return "Images/Gamebanana.png";
            }
            else if (link.Contains("github.com"))
            {
                return "Images/Github.png";
            }
            else if (!string.IsNullOrWhiteSpace(link))
            {
                return "Images/Web.png";
            }
            else
            {
                return null;
            }
        }
        public void Refresh()
        {
            try
            {
                try
                {
                    enabledmods.Clear();
                }
                catch { }
                enabledmods = QuickJson(false, enabledmods, "enabledmods.json");
            }
            catch { }
            ModDataGrid.Items.Clear();
            Sort.Items.Clear();
            Sort.Items.Add("---");
            Sort2.Items.Clear();
            Sort2.Items.Add("---");
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
                    if ((path + $@"\{mod.ID}" == modpath) && !ModDataGrid.Items.Contains(mod))
                    {
                        if (enabledmods.Contains(mod.ID))
                            mod.IsChecked = true;
                        else
                            mod.IsChecked = false;
                        mod.LinkImage = CreateLinkImage(mod.Link);
                        ModDataGrid.Items.Add(mod);
                        if (!Sort.Items.Contains(mod.Type))
                            Sort.Items.Add(mod.Type);
                        if (!Sort2.Items.Contains(mod.Category))
                            Sort2.Items.Add(mod.Category);
                    }
                    else
                        ParallelLogger.Log($"[ERROR] Folder and ID mismatch. Mod ''{mod.Name}'' will not be used.");
                }
                else
                    ParallelLogger.Log($"[ERROR] Mod ''{mod.Name}'' does not have an ID, and will not be used.");
            }
            Settings settings = new Settings();
            string settingspath = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\settings.json";
            if (File.Exists(settingspath))
            {
                var jsonoptions = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                string jsonString = File.ReadAllText(settingspath);
                settings = JsonSerializer.Deserialize<Settings>(jsonString, jsonoptions);
                PathBox.Text = settings.DeployPath;
            }
            ParallelLogger.Log($"[INFO] Refreshed mods.");
            ParallelLogger.Log($"[INFO] Refreshed settings.");
            Search.Text = "Search...";
            Sort.SelectedIndex = 0;
            Sort2.SelectedIndex = 0;
            ModDataGrid.Items.Refresh();
        }
        static void CopyDirectory(string sourceDir, string destinationDir, bool overwrite = true)
        {
            if (!Directory.Exists(sourceDir))
                throw new DirectoryNotFoundException($"Source directory not found: {sourceDir}");
            Directory.CreateDirectory(destinationDir);
            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string fileName = System.IO.Path.GetFileName(file);
                string destFilePath = System.IO.Path.Combine(destinationDir, fileName);
                File.Copy(file, destFilePath, overwrite);
            }
            foreach (string subDir in Directory.GetDirectories(sourceDir))
            {
                string subDirName = System.IO.Path.GetFileName(subDir);
                string destSubDirPath = System.IO.Path.Combine(destinationDir, subDirName);
                CopyDirectory(subDir, destSubDirPath, overwrite);
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
            Refresh();
        }

        private void Edit_OnClick(object sender, RoutedEventArgs e)
        {
            Meta row = (Meta)ModDataGrid.SelectedItem;
            MakePack edit = new MakePack(row);
            try
            {
                edit.ShowDialog();
            }
            catch { }
            Refresh();
        }

        private void Folder_OnClick(object sender, RoutedEventArgs e)
        {
            foreach (var item in ModDataGrid.SelectedItems)
            {
                Meta row = (Meta)item;
                if (row != null)
                {
                    if (Directory.Exists($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Mods\{row.ID}"))
                    {
                        try
                        {
                            ProcessStartInfo StartInformation = new ProcessStartInfo();
                            StartInformation.FileName = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Mods\{row.ID}";
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
            foreach (var item in ModDataGrid.SelectedItems)
            {
                Meta row = (Meta)item;
                if (row != null)
                {
                    if (Directory.Exists($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Mods\{row.ID}"))
                    {
                        try
                        {
                            CopyDirectory($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Mods\{row.ID}", $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Temp\{row.ID}\{row.Name}", true);
                            SevenZipCompressor.SetLibraryPath($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\x86\7z.dll");
                            SevenZipCompressor zcompressor = new SevenZipCompressor
                            {
                                ArchiveFormat = OutArchiveFormat.SevenZip,
                                CompressionLevel = CompressionLevel.High
                            };
                            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog
                            {
                                Title = "Save Mod Archive",
                                Filter = "Pulsar Archive (*.7z)|*.7z",
                                DefaultExt = ".7z",
                                FileName = $@"{row.ID}.7z"
                            };

                            if (saveFileDialog.ShowDialog() == true)
                            {
                                string filePath = saveFileDialog.FileName;
                                zcompressor.CompressDirectory($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Temp\{row.ID}", filePath);
                                ParallelLogger.Log($@"[INFO] Archive for {row.Name} created successfully.");
                            }
                            else
                            {
                                Console.WriteLine("Save file operation canceled.");
                            }
                            Directory.Delete($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Temp\{row.ID}\{row.Name}", true);
                        }
                        catch (Exception ex)
                        {
                            ParallelLogger.Log($@"[ERROR] Couldn't open \Mods\{row.ID} ({ex.Message})");
                        }
                    }
                }
            }
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
                    DescBox.Text = "Quasar never worked for me, so I made my own. You're seeing this because this mod has no description, or no mod is selected.\n\nDon't see a mod? The ID and folder names must match.\n\nConfused about the buttons at the bottom? Hover over them for more info.";
                else
                    DescBox.Text = row.Description;
            }
            catch
            {
                DescBox.Text = "Quasar never worked for me, so I made my own. You're seeing this because this mod has no description, or no mod is selected.\n\nDon't see a mod? The ID and folder names must match.\n\nConfused about the buttons at the bottom? Hover over them for more info.";
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
            string settingspath = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\settings.json";
            string jsonString = File.ReadAllText(settingspath);
            var jsonoptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            Settings settings = JsonSerializer.Deserialize<Settings>(jsonString, jsonoptions);
            foreach (string ID in enabledmods)
            {
                string sourceDir = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Mods\{ID}";
                string destinationDir = settings.DeployPath + $@"\" + ID;
                if (!Directory.Exists(sourceDir))
                    throw new DirectoryNotFoundException($"Source directory not found: {sourceDir}");
                Directory.CreateDirectory(destinationDir);
                foreach (string file in Directory.GetFiles(sourceDir))
                {
                    string fileName = System.IO.Path.GetFileName(file);
                    string destFilePath = System.IO.Path.Combine(destinationDir, fileName);
                    File.Copy(file, destFilePath, true);
                    ParallelLogger.Log($@"[INFO] Copied {file} to {destFilePath}");
                }
                foreach (string subDir in Directory.GetDirectories(sourceDir))
                {
                    string subDirName = System.IO.Path.GetFileName(subDir);
                    string destSubDirPath = System.IO.Path.Combine(destinationDir, subDirName);
                    CopyDirectory(subDir, destSubDirPath, true);
                }
            }
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

        private void Path_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog modspath = new System.Windows.Forms.FolderBrowserDialog();
            if (modspath.ShowDialog() != null)
            {
                if (!string.IsNullOrWhiteSpace(modspath.SelectedPath))
                    PathBox.Text = modspath.SelectedPath;
            }
        }

        private void PathBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ParallelLogger.Log($"[INFO] Updated settings.");
            Settings settings = new Settings();
            settings.DeployPath = PathBox.Text;
            string filepath = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\settings.json";
            var jsonoptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            string jsonString = JsonSerializer.Serialize(settings, jsonoptions);
            File.WriteAllText(filepath, jsonString);
            settings = JsonSerializer.Deserialize<Settings>(jsonString, jsonoptions);
        }

        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Mods"))
            {
                ProcessStartInfo StartInformation = new ProcessStartInfo();
                StartInformation.FileName = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Mods";
                StartInformation.UseShellExecute = true;
                Process process = Process.Start(StartInformation);
                ParallelLogger.Log($@"[INFO] Opened \Mods.");
            }
        }

        private void Search_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.TextBox search)
            {
                e.Handled = true;
                search.Focus();
                search.SelectAll();
            }
        }

        private void Search_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is System.Windows.Controls.TextBox search)
            {
                e.Handled = true;
                search.Focus();
                search.SelectAll();
            }
        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                ModDataGrid.Items.Clear();
                string path = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Mods";
                string[] griditems = CountFolders(path);
                foreach (string modpath in griditems)
                {
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
                        if ((path + $@"\{mod.ID}" == modpath) && !ModDataGrid.Items.Contains(mod)
                            && (mod.ID.Contains(Search.Text) || mod.Name.Contains(Search.Text) || Search.Text.Contains("Search...") || string.IsNullOrWhiteSpace(Search.Text))
                            && (Sort.SelectedItem.Equals(mod.Type) || Sort.SelectedItem.Equals("---"))
                            && (Sort2.SelectedItem.Equals(mod.Category) || Sort2.SelectedItem.Equals("---")))
                        {
                            if (enabledmods.Contains(mod.ID))
                                mod.IsChecked = true;
                            else
                                mod.IsChecked = false;
                            mod.LinkImage = CreateLinkImage(mod.Link);
                            ModDataGrid.Items.Add(mod);
                        }
                    }
                }
                ModDataGrid.Items.Refresh();
            }
            catch { }
        }

        private void Sort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Search_TextChanged(sender, null);
        }

        private void Sort2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Search_TextChanged(sender, null);
        }

        private void InstallArchive_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SevenZip.SevenZipExtractor.SetLibraryPath($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\x86\7z.dll");
                System.Windows.Forms.OpenFileDialog openarchive = new System.Windows.Forms.OpenFileDialog();
                openarchive.Filter = "Mod Archive (*.*)|*.*";
                openarchive.Title = "Select Mod Archive";
                string archivePath = null;
                string filename = null;
                string filetype = null;
                string firstDirectoryPath = null;
                string extpath = null;
                string newpath = null;
                if (openarchive.ShowDialog() != null)
                {
                    archivePath = openarchive.FileName;
                    filename = System.IO.Path.GetFileNameWithoutExtension(archivePath);
                    filetype = System.IO.Path.GetExtension(archivePath);
                }
                ParallelLogger.Log($@"[DEBUG] File format {filetype}");
                string outputFolder = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Mods";
                if (filetype == ".rar")
                {
                    using (ArchiveFile archiveFile = new ArchiveFile(archivePath))
                    {
                        foreach (var fileData in archiveFile.Entries)
                        {
                            if (fileData.IsFolder)
                            {
                                var parts = fileData.FileName.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
                                if (parts.Length == 1)
                                {
                                    firstDirectoryPath = fileData.FileName.TrimEnd('/', '\\');
                                    ParallelLogger.Log($@"[DEBUG] Found first directory {firstDirectoryPath}");
                                    break;
                                }
                            }
                        }
                        extpath = outputFolder + $@"\" + firstDirectoryPath.ToLower();
                        newpath = outputFolder + $@"\" + filename.ToLower();
                        Directory.CreateDirectory(extpath);
                        archiveFile.Extract(outputFolder, true);
                        string temppath = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Temp";
                    }
                    ParallelLogger.Log($"[INFO] Extracted ''{firstDirectoryPath}''.");
                }
                else
                {
                    using (var extractor = new SevenZip.SevenZipExtractor(archivePath))
                    {
                        foreach (var fileData in extractor.ArchiveFileData)
                        {
                            if (fileData.IsDirectory)
                            {
                                var parts = fileData.FileName.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
                                if (parts.Length == 1)
                                {
                                    firstDirectoryPath = fileData.FileName.TrimEnd('/', '\\');
                                    ParallelLogger.Log($@"[DEBUG] Found first directory {firstDirectoryPath}");
                                    break;
                                }
                            }
                        }
                        extpath = outputFolder + $@"\" + firstDirectoryPath.ToLower();
                        newpath = outputFolder + $@"\" + filename.ToLower();
                        Directory.CreateDirectory(extpath);
                        extractor.ExtractArchive(outputFolder);
                        string temppath = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Temp";
                        if (Directory.Exists(extpath))
                        {
                            Directory.Move(extpath, temppath);
                            Directory.Move(temppath, newpath);
                            ParallelLogger.Log($@"[DEBUG] Renamed '{extpath}' to '{newpath}'");
                        }
                    }
                }
                if (!File.Exists(newpath + $@"\meta.json"))
                {
                    Meta extmod = new Meta();
                    extmod.Name = firstDirectoryPath;
                    extmod.ID = filename.ToLower();
                    if (File.Exists(newpath + $@"\preview.png"))
                        extmod.ArchiveImage = true;
                    MakePack finish = new MakePack(extmod);
                    finish.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                ParallelLogger.Log($"[ERROR] Error during extraction: {ex.Message}");
            }
            Refresh();
        }

        private void OpenLink_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button button && button.CommandParameter is string url)
            {
                try
                {
                    System.Diagnostics.Process.Start(new ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    ParallelLogger.Log($"[ERROR] Failed to open link: {ex.Message}");
                }
            }
        }
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.CheckBox checkBox)
            {
                var row = checkBox.DataContext as Meta;
                if (row != null)
                {
                    if (!enabledmods.Contains(row.ID))
                        enabledmods.Add(row.ID);
                    QuickJson(true, enabledmods, "enabledmods.json");
                    enabledmods = QuickJson(false, enabledmods, "enabledmods.json");
                }
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.CheckBox checkBox)
            {
                var row = checkBox.DataContext as Meta;
                if (row != null)
                {
                    if (enabledmods.Contains(row.ID))
                        enabledmods.Remove(row.ID);
                    QuickJson(true, enabledmods, "enabledmods.json");
                    enabledmods = QuickJson(false, enabledmods, "enabledmods.json");
                }
            }
        }
        private List<string> QuickJson(bool write, List<string> what, string filename)
        {
            string root = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\";
            if (write)
            {
                var jsonoptions = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                string jsonString = JsonSerializer.Serialize(what, jsonoptions);
                File.WriteAllText(root + filename, jsonString);
                return null;
            }
            else
            {
                var jsonoptions = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                string jsonString = File.ReadAllText(root + filename);
                what = JsonSerializer.Deserialize<List<string>>(jsonString, jsonoptions);
                return what;
            }
        }
    }
}
