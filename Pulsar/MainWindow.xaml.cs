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
using System.Security.Policy;
using Pulsar.Utilities;
using Pulsar.Utitlities;
using System.Runtime.Remoting.Messaging;
using static System.Net.WebRequestMethods;
using System.Net.Http;
using static System.Windows.Forms.LinkLabel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

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
        private List<System.Windows.Controls.ComboBox> assignboxes = new List<System.Windows.Controls.ComboBox>();
        private bool isInitialized = false;

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
            assignboxes.Add(ChangeBox1);
            assignboxes.Add(ChangeBox2);
            assignboxes.Add(ChangeBox3);
            assignboxes.Add(ChangeBox4);
            assignboxes.Add(ChangeBox5);
            assignboxes.Add(ChangeBox6);
            assignboxes.Add(ChangeBox7);
            assignboxes.Add(ChangeBox8);
            AssignHideAll(true);
            Refresh();
            isInitialized = true;
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
            try
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
            catch { return null; }
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
            Sort3.Items.Clear();
            Sort3.Items.Add("---");
            BlacklistBox.Items.Clear();
            string path = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Mods";
            string[] griditems = CountFolders(path);
            Settings settings = new Settings();
            string settingspath = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\settings.json";
            List<string> blacklist = new List<string>();
            if (System.IO.File.Exists(settingspath))
            {
                var jsonoptions = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                string jsonString = System.IO.File.ReadAllText(settingspath);
                settings = JsonSerializer.Deserialize<Settings>(jsonString, jsonoptions);
                PathBox.Text = settings.DeployPath;
                blacklist = settings.Blacklist;
                DefPrevBox.SelectedIndex = settings.DefaultImage;
                Preview.Source = new BitmapImage(new Uri($"/Images/Preview{DefPrevBox.SelectedIndex}.png", UriKind.Relative));
                BlacklistOn.IsChecked = settings.EnableBlacklist;
            }
            foreach (string modpath in griditems)
            {
                Meta mod = new Meta();
                string filepath = modpath + $@"\meta.json";
                if (!System.IO.File.Exists(filepath))
                {
                    string genid = modpath.Replace(path, "");
                    mod.Name = mod.ID = genid = genid.TrimStart('\\');
                    ParallelLogger.Log($@"[INFO] {genid} doesn't have a meta.json. Generating one.");
                    var jsonoptions = new JsonSerializerOptions
                    {
                        WriteIndented = true
                    };
                    string jsonString = JsonSerializer.Serialize(mod, jsonoptions);
                    System.IO.File.WriteAllText(filepath, jsonString);
                }
                if (System.IO.File.Exists(modpath + $@"\rescan"))
                {
                    System.IO.File.Delete(modpath + $@"\rescan");
                    System.IO.File.Delete(modpath + $@"\files.json");
                    System.IO.File.Delete(modpath + $@"\deploy.json");
                    PathManagement.GatherFiles(modpath);
                    System.IO.File.Copy(modpath + $@"\files.json", modpath + $@"\deploy.json");
                }
                if (!System.IO.File.Exists(modpath + $@"\files.json"))
                {
                    PathManagement.GatherFiles(modpath);
                }
                if (!System.IO.File.Exists(modpath + $@"\deploy.json"))
                {
                    System.IO.File.Copy(modpath + $@"\files.json", modpath + $@"\deploy.json");
                }
                if (System.IO.File.Exists(modpath + $@"\config.json"))
                {
                    if (!System.IO.File.Exists(modpath + $@"\userconfig.json"))
                    {
                        System.IO.File.Copy(modpath + $@"\config.json", modpath + $@"\userconfig.json");
                    }
                }
                if (System.IO.File.Exists(filepath))
                {
                    var jsonoptions = new JsonSerializerOptions
                    {
                        WriteIndented = true
                    };
                    string jsonString = System.IO.File.ReadAllText(filepath);
                    mod = JsonSerializer.Deserialize<Meta>(jsonString, jsonoptions);
                    if ((path + $@"\{mod.ID}" == modpath) && !ModDataGrid.Items.Contains(mod))
                    {
                        if (mod.Tags != null && settings.EnableBlacklist && blacklist != null && mod.Tags.Any(tag => blacklist.Contains(tag, StringComparer.OrdinalIgnoreCase)))
                        {

                        }
                        else
                        {
                            if (enabledmods.Contains(mod.ID))
                                mod.IsChecked = true;
                            else
                                mod.IsChecked = false;
                            mod.LinkImage = CreateLinkImage(mod.Link);
                            ModDataGrid.Items.Add(mod);
                            if (!Sort.Items.Contains(mod.Type) && !string.IsNullOrEmpty(mod.Type))
                                Sort.Items.Add(mod.Type);
                            if (!Sort2.Items.Contains(mod.Category) && !string.IsNullOrEmpty(mod.Category))
                                Sort2.Items.Add(mod.Category);
                            if (mod.Tags != null)
                            {
                                for (int i = 0; i < mod.Tags.Count; i++)
                                {
                                    if (!Sort3.Items.Contains(mod.Tags[i]) && !string.IsNullOrEmpty(mod.Tags[i]))
                                    {
                                        Sort3.Items.Add(mod.Tags[i]);
                                        BlacklistBox.Items.Add(mod.Tags[i]);
                                    }
                                }
                            }
                        }
                    }
                    else
                        ParallelLogger.Log($"[ERROR] Folder and ID mismatch. Mod ''{mod.Name}'' will not be used.");
                }
                else
                    ParallelLogger.Log($"[ERROR] Mod ''{mod.Name}'' does not have an ID, and will not be used.");
            }
            ParallelLogger.Log($"[INFO] Refreshed mods.");
            ParallelLogger.Log($"[INFO] Refreshed settings.");
            Search.Text = "Search...";
            Sort.SelectedIndex = 0;
            Sort2.SelectedIndex = 0;
            Sort3.SelectedIndex = 0;
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
                System.IO.File.Copy(file, destFilePath, overwrite);
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
            Preview.Source = new BitmapImage(new Uri($"/Images/Preview{DefPrevBox.SelectedIndex}.png", UriKind.Relative));
            newpack.ShowDialog();
            Refresh();
        }

        private void Edit_OnClick(object sender, RoutedEventArgs e)
        {
            Meta row = (Meta)ModDataGrid.SelectedItem;
            MakePack edit = new MakePack(row);
            Preview.Source = new BitmapImage(new Uri($"/Images/Preview{DefPrevBox.SelectedIndex}.png", UriKind.Relative));
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
            AssignHideAll(true);
            DetectSlots();
            Assign_Click(null, null);
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
                            System.IO.File.Delete($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Temp\{row.ID}\{row.Name}\files.json");
                            System.IO.File.Delete($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Temp\{row.ID}\{row.Name}\deploy.json");
                            System.IO.File.Delete($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Temp\{row.ID}\{row.Name}\userconfig.json");

                            var jsonoptions = new JsonSerializerOptions
                            {
                                WriteIndented = true
                            };
                            row.Tags.Clear();
                            string jsonString = JsonSerializer.Serialize(row, jsonoptions);
                            string filepath = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Temp\{row.ID}\{row.Name}" + $@"\meta.json";
                            System.IO.File.WriteAllText(filepath, jsonString);
                            var libpath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Environment.Is64BitProcess ? "x64" : "x86", "7z.dll");
                            SevenZipCompressor.SetLibraryPath(libpath);
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
            Refresh();
        }

        private void Delete_OnClick(object sender, RoutedEventArgs e)
        {
            
            foreach (var item in ModDataGrid.SelectedItems)
            {
                Meta row = (Meta)item;
                if (row != null)
                {
                    Alert aw = new Alert($@"Are you sure you want to delete {row.Name}?", false);
                    aw.OnAlertHandled = () =>
                    {
                        Directory.Delete($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Mods\{row.ID}", true);
                    };
                    aw.ShowDialog();
                }
            }
            Refresh();
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
                if (row.Authors != null || !string.IsNullOrWhiteSpace(row.Authors[0]))
                {
                    string authors = $"Authors of {row.Name}:\n";
                    for (int i = 0; i < row.Authors.Count; i++)
                    {
                        authors = authors + "\n" + row.Authors[i];
                    }
                    AuthorBox.Text = authors;
                    if (authors.Trim() == $"Authors of {row.Name}:\n".Trim())
                    {
                        AuthorBox.Text = "Solt11 made Pulsar.\n\nAuthors of mods will show up here if provided.";
                    }
                }
                else
                    AuthorBox.Text = "Solt11 made Pulsar.\n\nAuthors of mods will show up here if provided.";
            }
            catch
            {
                AuthorBox.Text = "Solt11 made Pulsar.\n\nAuthors of mods will show up here if provided.";
            }
            try
            {
                if (System.IO.File.Exists($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Mods\{row.ID}\preview.webp"))
                {
                    BitmapImage bitmap = new BitmapImage();
                    using (FileStream fs = new FileStream($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Mods\{row.ID}\preview.webp", FileMode.Open, FileAccess.Read))
                    {
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = fs;
                        bitmap.EndInit();
                    }
                    Preview.Source = bitmap;
                }
                else
                {
                    Preview.Source = new BitmapImage(new Uri($"/Images/Preview{DefPrevBox.SelectedIndex}.png", UriKind.Relative));
                }
            }
            catch
            {
                Preview.Source = new BitmapImage(new Uri($"/Images/Preview{DefPrevBox.SelectedIndex}.png", UriKind.Relative));
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

        private void DetectSlots()
        {
            List<string> files = new List<string>();
            List<string> deployfiles = new List<string>();
            foreach (var item in ModDataGrid.SelectedItems)
            {
                Meta row = (Meta)item;
                if (row != null)
                {
                    if (System.IO.File.Exists($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Mods\{row.ID}\files.json"))
                    {
                        try
                        {
                            files = QuickJson(false, null, $@"Mods\{row.ID}\files.json");
                            if (System.IO.File.Exists($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Mods\{row.ID}\deploy.json"))
                            {
                                deployfiles = QuickJson(false, null, $@"Mods\{row.ID}\deploy.json");
                            }
                            else
                            {
                                deployfiles = files;
                            }
                            bool hasassign = false;
                            for (int i = 0; i < files.Count; i++)
                            {
                                for (int ii = 0; ii <= 7; ii++)
                                {
                                    if (files[i].Contains("\\ui\\replace\\chara\\") && files[i].Contains($"0{ii}.bntx"))
                                    {
                                        for (int iii = 0; iii <= 7; iii++)
                                        {
                                            if (deployfiles[i].Contains("\\ui\\replace\\chara\\") && deployfiles[i].Contains($"0{iii}.bntx"))
                                            {
                                                Sub(ii, false);
                                                assignboxes[ii].SelectedIndex = iii;
                                                Sub(ii, true);
                                            }
                                        }
                                        AssignHide(ii + 1, false);
                                        hasassign = true;
                                    }
                                    if (files[i].Contains($"c0{ii}"))
                                    {
                                        for (int iii = 0; iii <= 7; iii++)
                                        {
                                            if (deployfiles[i].Contains($"c0{iii}"))
                                            {
                                                Sub(ii, false);
                                                assignboxes[ii].SelectedIndex = iii;
                                                Sub(ii, true);
                                            }
                                        }
                                        AssignHide(ii + 1, false);
                                        hasassign = true;
                                    }
                                }
                            }
                            if (hasassign)
                            {
                                ModName1.Text = row.Name;
                                TypeName1.Text = row.Type;
                                CatName1.Text = row.Category;
                                ModName2.Text = row.Name;
                                TypeName2.Text = row.Type;
                                CatName2.Text = row.Category;
                                ModName3.Text = row.Name;
                                TypeName3.Text = row.Type;
                                CatName3.Text = row.Category;
                                ModName4.Text = row.Name;
                                TypeName4.Text = row.Type;
                                CatName4.Text = row.Category;
                                ModName5.Text = row.Name;
                                TypeName5.Text = row.Type;
                                CatName5.Text = row.Category;
                                ModName6.Text = row.Name;
                                TypeName6.Text = row.Type;
                                CatName6.Text = row.Category;
                                ModName7.Text = row.Name;
                                TypeName7.Text = row.Type;
                                CatName7.Text = row.Category;
                                ModName8.Text = row.Name;
                                TypeName8.Text = row.Type;
                                CatName8.Text = row.Category;
                                ModID0.Text = row.ID;
                            }
                        }
                        catch (Exception ex)
                        {
                            ParallelLogger.Log($@"[ERROR] Couldn't open \Mods\{row.ID} ({ex.Message})");
                        }
                    }
                }
            }
        }
        private void Sub(int sender, bool yes)
        {
            if ((sender == 0) && yes)
            {
                ChangeBox1.SelectionChanged += ChangeBox1_SelectionChanged;
            }
            else if ((sender == 1) && yes)
            {
                ChangeBox2.SelectionChanged += ChangeBox2_SelectionChanged;
            }
            else if ((sender == 2) && yes)
            {
                ChangeBox3.SelectionChanged += ChangeBox3_SelectionChanged;
            }
            else if ((sender == 3) && yes)
            {
                ChangeBox4.SelectionChanged += ChangeBox4_SelectionChanged;
            }
            else if ((sender == 4) && yes)
            {
                ChangeBox5.SelectionChanged += ChangeBox5_SelectionChanged;
            }
            else if ((sender == 5) && yes)
            {
                ChangeBox6.SelectionChanged += ChangeBox6_SelectionChanged;
            }
            else if ((sender == 6) && yes)
            {
                ChangeBox7.SelectionChanged += ChangeBox7_SelectionChanged;
            }
            else if ((sender == 7) && yes)
            {
                ChangeBox8.SelectionChanged += ChangeBox8_SelectionChanged;
            }
            else if ((sender == 0) && yes == false)
            {
                ChangeBox1.SelectionChanged -= ChangeBox1_SelectionChanged;
            }
            else if ((sender == 1) && yes == false)
            {
                ChangeBox2.SelectionChanged -= ChangeBox2_SelectionChanged;
            }
            else if ((sender == 2) && yes == false)
            {
                ChangeBox3.SelectionChanged -= ChangeBox3_SelectionChanged;
            }
            else if ((sender == 3) && yes == false)
            {
                ChangeBox4.SelectionChanged -= ChangeBox4_SelectionChanged;
            }
            else if ((sender == 4) && yes == false)
            {
                ChangeBox5.SelectionChanged -= ChangeBox5_SelectionChanged;
            }
            else if ((sender == 5) && yes == false)
            {
                ChangeBox6.SelectionChanged -= ChangeBox6_SelectionChanged;
            }
            else if ((sender == 6) && yes == false)
            {
                ChangeBox7.SelectionChanged -= ChangeBox7_SelectionChanged;
            }
            else if ((sender == 7) && yes == false)
            {
                ChangeBox8.SelectionChanged -= ChangeBox8_SelectionChanged;
            }
        }

        private void AssignHide(int sender, bool hide)
        {
            if ((sender == 1) && hide)
            {
                ModName1.Visibility = Visibility.Collapsed;
                TypeName1.Visibility = Visibility.Collapsed;
                CatName1.Visibility = Visibility.Collapsed;
                Origin1.Visibility = Visibility.Collapsed;
                Change1.Visibility = Visibility.Collapsed;
                ChangeBox1.Visibility = Visibility.Collapsed;
                ChangeBox1.SelectionChanged -= ChangeBox1_SelectionChanged;
                ChangeBox1.SelectedIndex = 0;
                ChangeBox1.SelectionChanged += ChangeBox1_SelectionChanged;
            }
            else if ((sender == 2) && hide)
            {
                ModName2.Visibility = Visibility.Collapsed;
                TypeName2.Visibility = Visibility.Collapsed;
                CatName2.Visibility = Visibility.Collapsed;
                Origin2.Visibility = Visibility.Collapsed;
                Change2.Visibility = Visibility.Collapsed;
                ChangeBox2.Visibility = Visibility.Collapsed;
                ChangeBox2.SelectionChanged -= ChangeBox2_SelectionChanged;
                ChangeBox2.SelectedIndex = 1;
                ChangeBox2.SelectionChanged += ChangeBox2_SelectionChanged;
            }
            else if ((sender == 3) && hide)
            {
                ModName3.Visibility = Visibility.Collapsed;
                TypeName3.Visibility = Visibility.Collapsed;
                CatName3.Visibility = Visibility.Collapsed;
                Origin3.Visibility = Visibility.Collapsed;
                Change3.Visibility = Visibility.Collapsed;
                ChangeBox3.Visibility = Visibility.Collapsed;
                ChangeBox3.SelectionChanged -= ChangeBox3_SelectionChanged;
                ChangeBox3.SelectedIndex = 2;
                ChangeBox3.SelectionChanged += ChangeBox3_SelectionChanged;
            }
            else if ((sender == 4) && hide)
            {
                ModName4.Visibility = Visibility.Collapsed;
                TypeName4.Visibility = Visibility.Collapsed;
                CatName4.Visibility = Visibility.Collapsed;
                Origin4.Visibility = Visibility.Collapsed;
                Change4.Visibility = Visibility.Collapsed;
                ChangeBox4.Visibility = Visibility.Collapsed;
                ChangeBox4.SelectionChanged -= ChangeBox4_SelectionChanged;
                ChangeBox4.SelectedIndex = 3;
                ChangeBox4.SelectionChanged += ChangeBox4_SelectionChanged;
            }
            else if ((sender == 5) && hide)
            {
                ModName5.Visibility = Visibility.Collapsed;
                TypeName5.Visibility = Visibility.Collapsed;
                CatName5.Visibility = Visibility.Collapsed;
                Origin5.Visibility = Visibility.Collapsed;
                Change5.Visibility = Visibility.Collapsed;
                ChangeBox5.Visibility = Visibility.Collapsed;
                ChangeBox5.SelectionChanged -= ChangeBox5_SelectionChanged;
                ChangeBox5.SelectedIndex = 4;
                ChangeBox5.SelectionChanged += ChangeBox5_SelectionChanged;
            }
            else if ((sender == 6) && hide)
            {
                ModName6.Visibility = Visibility.Collapsed;
                TypeName6.Visibility = Visibility.Collapsed;
                CatName6.Visibility = Visibility.Collapsed;
                Origin6.Visibility = Visibility.Collapsed;
                Change6.Visibility = Visibility.Collapsed;
                ChangeBox6.Visibility = Visibility.Collapsed;
                ChangeBox6.SelectionChanged -= ChangeBox6_SelectionChanged;
                ChangeBox6.SelectedIndex = 5;
                ChangeBox6.SelectionChanged += ChangeBox6_SelectionChanged;
            }
            else if ((sender == 7) && hide)
            {
                ModName7.Visibility = Visibility.Collapsed;
                TypeName7.Visibility = Visibility.Collapsed;
                CatName7.Visibility = Visibility.Collapsed;
                Origin7.Visibility = Visibility.Collapsed;
                Change7.Visibility = Visibility.Collapsed;
                ChangeBox7.Visibility = Visibility.Collapsed;
                ChangeBox7.SelectionChanged -= ChangeBox7_SelectionChanged;
                ChangeBox7.SelectedIndex = 6;
                ChangeBox7.SelectionChanged += ChangeBox7_SelectionChanged;
            }
            else if ((sender == 8) && hide)
            {
                ModName8.Visibility = Visibility.Collapsed;
                TypeName8.Visibility = Visibility.Collapsed;
                CatName8.Visibility = Visibility.Collapsed;
                Origin8.Visibility = Visibility.Collapsed;
                Change8.Visibility = Visibility.Collapsed;
                ChangeBox8.Visibility = Visibility.Collapsed;
                ChangeBox8.SelectionChanged -= ChangeBox8_SelectionChanged;
                ChangeBox8.SelectedIndex = 7;
                ChangeBox8.SelectionChanged += ChangeBox8_SelectionChanged;
            }
            else if ((sender == 1) && hide == false)
            {
                ModName1.Visibility = Visibility.Visible;
                TypeName1.Visibility = Visibility.Visible;
                CatName1.Visibility = Visibility.Visible;
                Origin1.Visibility = Visibility.Visible;
                Change1.Visibility = Visibility.Visible;
                ChangeBox1.Visibility = Visibility.Visible;
            }
            else if ((sender == 2) && hide == false)
            {
                ModName2.Visibility = Visibility.Visible;
                TypeName2.Visibility = Visibility.Visible;
                CatName2.Visibility = Visibility.Visible;
                Origin2.Visibility = Visibility.Visible;
                Change2.Visibility = Visibility.Visible;
                ChangeBox2.Visibility = Visibility.Visible;
            }
            else if ((sender == 3) && hide == false)
            {
                ModName3.Visibility = Visibility.Visible;
                TypeName3.Visibility = Visibility.Visible;
                CatName3.Visibility = Visibility.Visible;
                Origin3.Visibility = Visibility.Visible;
                Change3.Visibility = Visibility.Visible;
                ChangeBox3.Visibility = Visibility.Visible;
            }
            else if ((sender == 4) && hide == false)
            {
                ModName4.Visibility = Visibility.Visible;
                TypeName4.Visibility = Visibility.Visible;
                CatName4.Visibility = Visibility.Visible;
                Origin4.Visibility = Visibility.Visible;
                Change4.Visibility = Visibility.Visible;
                ChangeBox4.Visibility = Visibility.Visible;
            }
            else if ((sender == 5) && hide == false)
            {
                ModName5.Visibility = Visibility.Visible;
                TypeName5.Visibility = Visibility.Visible;
                CatName5.Visibility = Visibility.Visible;
                Origin5.Visibility = Visibility.Visible;
                Change5.Visibility = Visibility.Visible;
                ChangeBox5.Visibility = Visibility.Visible;
            }
            else if ((sender == 6) && hide == false)
            {
                ModName6.Visibility = Visibility.Visible;
                TypeName6.Visibility = Visibility.Visible;
                CatName6.Visibility = Visibility.Visible;
                Origin6.Visibility = Visibility.Visible;
                Change6.Visibility = Visibility.Visible;
                ChangeBox6.Visibility = Visibility.Visible;
            }
            else if ((sender == 7) && hide == false)
            {
                ModName7.Visibility = Visibility.Visible;
                TypeName7.Visibility = Visibility.Visible;
                CatName7.Visibility = Visibility.Visible;
                Origin7.Visibility = Visibility.Visible;
                Change7.Visibility = Visibility.Visible;
                ChangeBox7.Visibility = Visibility.Visible;
            }
            else if ((sender == 8) && hide == false)
            {
                ModName8.Visibility = Visibility.Visible;
                TypeName8.Visibility = Visibility.Visible;
                CatName8.Visibility = Visibility.Visible;
                Origin8.Visibility = Visibility.Visible;
                Change8.Visibility = Visibility.Visible;
                ChangeBox8.Visibility = Visibility.Visible;
            }
        }
        private void AssignHideAll(bool hide)
        {
            for (int i = 1; i <= 8; i++)
            {
                if (hide)
                {
                    AssignHide(i, true);
                }
                else
                {
                    AssignHide(i, false);
                }
            }
        }
        // I'd really like to make this work, but it'd take a lot of time.
        // If you're looking at the source code, that's great!
        // I'll make a window eventually, but it'd take a lot of time that I don't have, so use 1-Click for now.

        private void Download_Click(object sender, RoutedEventArgs e)
        {
            // Original function:
            /*
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
            */
            try
            {
                System.Diagnostics.Process.Start(new ProcessStartInfo
                {
                    FileName = "https://gamebanana.com/games/6498",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                ParallelLogger.Log($"[ERROR] Failed to open link: {ex.Message}");
            }
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
            string jsonString = System.IO.File.ReadAllText(settingspath);
            var jsonoptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            Settings settings = JsonSerializer.Deserialize<Settings>(jsonString, jsonoptions);
            if (!string.IsNullOrWhiteSpace(settings.DeployPath) && settings != null)
            {
                DefPrevBox.SelectedIndex = settings.DefaultImage;
                System.IO.File.Delete($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\conflictlog.json");
                Alert aw = new Alert($@"This will delete all files inside {settings.DeployPath}. Is this okay?", false);
                aw.OnAlertHandled = () =>
                {
                    ParallelLogger.Log($@"[INFO] THIS COULD TAKE A WHILE!");
                    Directory.Delete(settings.DeployPath, true);
                    Directory.CreateDirectory(settings.DeployPath);
                    foreach (string ID in enabledmods)
                    {
                        string sourceDir = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Mods\{ID}";
                        string destinationDir = settings.DeployPath + $@"\" + ID;
                        bool failed = false;
                        if (!Directory.Exists(sourceDir))
                        {
                            failed = true;
                            try
                            {
                                throw new DirectoryNotFoundException($"Source directory not found: {sourceDir}");
                            }
                            catch (Exception ex)
                            {
                                ParallelLogger.Log($@"[ERROR] {ex.Message}");
                                ParallelLogger.Log($@"[ERROR] A mod ID was found in the enabled mods that does not exist. Consider removing it from the enabledmods.json");
                            }
                        }
                        if (!failed)
                        {
                            List<string> files = QuickJson(false, null, $@"Mods\{ID}\files.json");
                            List<string> deployfiles = QuickJson(false, null, $@"Mods\{ID}\deploy.json");
                            Directory.CreateDirectory(destinationDir);
                            for (int i = 0; i < files.Count; i++)
                            {
                                if (!Conflict.Detect(deployfiles[i], ID, files[i]))
                                {
                                    string ogFilePath = sourceDir + files[i];
                                    string destFilePath = destinationDir + deployfiles[i];
                                    Directory.CreateDirectory(System.IO.Path.GetDirectoryName(destFilePath));
                                    System.IO.File.Copy(ogFilePath, destFilePath, true);
                                    ParallelLogger.Log($@"[INFO] Copied {ogFilePath} to {destFilePath}");
                                }
                            }
                        }
                    }
                    Alert done = new Alert($@" Succesfully deployed mods to {settings.DeployPath}!", false);
                    ParallelLogger.Log($@"[INFO] Succesfully deployed mods to {settings.DeployPath}!");
                    done.ShowDialog();
                };
                aw.ShowDialog();
            }
            else
            {
                ParallelLogger.Log($@"[ERROR] Couldn't find a deploy path. Go to the settings page and provide one.");
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
            if (!isInitialized) return;
            ParallelLogger.Log($"[INFO] Updated settings.");
            Settings settings = new Settings();
            settings.DeployPath = PathBox.Text;
            settings.DefaultImage = DefPrevBox.SelectedIndex;
            bool yes = false;
            if (BlacklistOn.IsChecked == null || BlacklistOn.IsChecked == false)
                yes = false;
            if (BlacklistOn.IsChecked == true)
                yes = true;
            settings.EnableBlacklist = yes;
            string filepath = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\settings.json";

            string jsonString = System.IO.File.ReadAllText(filepath);
            var jsonoptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            settings.Blacklist = JsonSerializer.Deserialize<Settings>(jsonString, jsonoptions).Blacklist;

            jsonString = JsonSerializer.Serialize(settings, jsonoptions);
            System.IO.File.WriteAllText(filepath, jsonString);
            settings = JsonSerializer.Deserialize<Settings>(jsonString, jsonoptions);
            Refresh();
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
            if (!isInitialized) return;
            try
            {
                ModDataGrid.Items.Clear();
                string path = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Mods";
                string[] griditems = CountFolders(path);
                string settingspath = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\settings.json";
                List<string> blacklist = new List<string>();
                bool blackliston = false;
                if (System.IO.File.Exists(settingspath))
                {
                    var jsonoptions = new JsonSerializerOptions
                    {
                        WriteIndented = true
                    };
                    string jsonString = System.IO.File.ReadAllText(settingspath);
                    blacklist = JsonSerializer.Deserialize<Settings>(jsonString, jsonoptions).Blacklist;
                    blackliston = JsonSerializer.Deserialize<Settings>(jsonString, jsonoptions).EnableBlacklist;

                }
                foreach (string modpath in griditems)
                {
                    Meta mod = new Meta();
                    string filepath = modpath + $@"\meta.json";
                    if (System.IO.File.Exists(filepath))
                    {
                        var jsonoptions = new JsonSerializerOptions
                        {
                            WriteIndented = true
                        };
                        string jsonString = System.IO.File.ReadAllText(filepath);
                        mod = JsonSerializer.Deserialize<Meta>(jsonString, jsonoptions);
                        if ((path + $@"\{mod.ID}" == modpath) && !ModDataGrid.Items.Contains(mod)
                            && (mod.ID.Contains(Search.Text) || mod.Name.Contains(Search.Text) || Search.Text.Contains("Search...") || string.IsNullOrWhiteSpace(Search.Text))
                            && (Sort.SelectedItem.Equals(mod.Type) || Sort.SelectedItem.Equals("---"))
                            && (Sort2.SelectedItem.Equals(mod.Category) || Sort2.SelectedItem.Equals("---")))
                        {
                            if (mod.Tags == null || mod.Tags.Count == 0)
                            {
                                if (Sort3.SelectedItem.Equals("---"))
                                {
                                    mod.IsChecked = enabledmods.Contains(mod.ID);
                                    mod.LinkImage = CreateLinkImage(mod.Link);
                                    ModDataGrid.Items.Add(mod);
                                }
                            }
                            else if (mod.Tags != null && blacklist != null && blackliston && mod.Tags.Any(tag => blacklist.Contains(tag, StringComparer.OrdinalIgnoreCase)))
                            {

                            }
                            else
                            {
                                if (mod.Tags.Any(tag => object.Equals(Sort3.SelectedItem, tag) || object.Equals(Sort3.SelectedItem, "---")))
                                {
                                    mod.IsChecked = enabledmods.Contains(mod.ID);
                                    mod.LinkImage = CreateLinkImage(mod.Link);
                                    ModDataGrid.Items.Add(mod);
                                }
                            }
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
        private void Sort3_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Search_TextChanged(sender, null);
        }

        private void InstallArchive_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var libpath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Environment.Is64BitProcess ? "x64" : "x86", "7z.dll");
                SevenZip.SevenZipExtractor.SetLibraryPath(libpath);
                System.Windows.Forms.OpenFileDialog openarchive = new System.Windows.Forms.OpenFileDialog();
                openarchive.Filter = "Mod Archive (*.*)|*.*";
                openarchive.Title = "Select Mod Archive";
                string archivePath = null;
                string filename = null;
                string filetype = null;
                string firstDirectoryPath = null;
                string extpath = null;
                string newpath = null;
                string temppath = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Temp";
                string outputFolder = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Mods";
                Directory.Delete(temppath, true);
                Directory.CreateDirectory(temppath);
                if (openarchive.ShowDialog() != null)
                {
                    archivePath = openarchive.FileName;
                    filename = System.IO.Path.GetFileNameWithoutExtension(archivePath);
                    filetype = System.IO.Path.GetExtension(archivePath);
                }
                ParallelLogger.Log($@"[DEBUG] File format {filetype}");
                HashSet<string> validNames = new HashSet<string>
                {
                    "append",
                    "assist",
                    "boss",
                    "camera",
                    "campaign",
                    "common",
                    "effect",
                    "enemy",
                    "fighter",
                    "finalsmash",
                    "item",
                    "mihat",
                    "param",
                    "pokemon",
                    "prebuilt",
                    "render",
                    "snapshot",
                    "sound",
                    "spirits",
                    "stage",
                    "standard",
                    "stream",
                    "ui"
                };
                if (filetype == ".rar")
                {
                    using (ArchiveFile archiveFile = new ArchiveFile(archivePath))
                    {
                        foreach (var fileData in archiveFile.Entries)
                        {
                            if (fileData.FileName.Contains("/") || fileData.FileName.Contains("\\"))
                            {
                                var parts = fileData.FileName.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
                                firstDirectoryPath = parts[0].TrimEnd('/', '\\');
                                ParallelLogger.Log($@"[DEBUG] Found first directory {firstDirectoryPath}");
                                break;
                            }
                        }
                        if (validNames.Contains(firstDirectoryPath))
                            extpath = temppath;
                        else
                            extpath = temppath + $@"\" + firstDirectoryPath.ToLower();
                        newpath = outputFolder + $@"\" + filename.ToLower();
                        Directory.CreateDirectory(extpath);
                        archiveFile.Extract(temppath, true);
                    }
                }
                else
                {
                    using (var extractor = new SevenZip.SevenZipExtractor(archivePath))
                    {
                        foreach (var fileData in extractor.ArchiveFileData)
                        {
                            if (fileData.FileName.Contains("/") || fileData.FileName.Contains("\\"))
                            {
                                var parts = fileData.FileName.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
                                firstDirectoryPath = parts[0].TrimEnd('/', '\\');
                                ParallelLogger.Log($@"[DEBUG] Found first directory {firstDirectoryPath}");
                                break;
                            }
                        }
                        if (validNames.Contains(firstDirectoryPath))
                            extpath = temppath;
                        else
                            extpath = temppath + $@"\" + firstDirectoryPath.ToLower();
                        newpath = outputFolder + $@"\" + filename.ToLower();
                        Directory.CreateDirectory(extpath);
                        extractor.ExtractArchive(temppath);
                    }
                }
                if (Directory.Exists(extpath))
                {
                    try
                    {
                        if (Directory.Exists(newpath))
                        {
                            bool moved = false;
                            Alert aw = new Alert($@"This will delete {newpath}. Okay?", false);
                            aw.OnAlertHandled = () =>
                            {
                                Directory.Delete(newpath, true);
                                Directory.Move(extpath, newpath);
                                ParallelLogger.Log($@"[DEBUG] Renamed '{extpath}' to '{newpath}'");
                                ParallelLogger.Log($"[INFO] Extracted ''{firstDirectoryPath}''.");
                                moved = true;
                            };
                            aw.ShowDialog();
                            if (moved == false)
                            {
                                Directory.Delete(temppath, true);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ParallelLogger.Log($@"[ERROR] {ex.Message}");
                    }
                }
                if (System.IO.File.Exists(newpath + $@"\meta.json"))
                {
                    var jsonoptions = new JsonSerializerOptions
                    {
                        WriteIndented = true
                    };
                    string jsonString = System.IO.File.ReadAllText(newpath + $@"\meta.json");
                    Meta extmod = JsonSerializer.Deserialize<Meta>(jsonString, jsonoptions);
                    if (System.IO.File.Exists(newpath + $@"\preview.webp"))
                        extmod.ArchiveImage = true;
                    MakePack finish = new MakePack(extmod);
                    finish.ShowDialog();
                }
                if (System.IO.File.Exists(newpath + $@"\info.toml"))
                {
                    Meta extmod = Parser.InfoTOML(newpath + $@"\info.toml");
                    extmod.ID = filename.ToLower();
                    if (System.IO.File.Exists(newpath + $@"\preview.webp"))
                        extmod.ArchiveImage = true;
                    MakePack finish = new MakePack(extmod);
                    finish.ShowDialog();
                }
                else
                {
                    Meta extmod = new Meta();
                    if (validNames.Contains(firstDirectoryPath))
                        extmod.Name = filename;
                    else
                        extmod.Name = firstDirectoryPath;
                    extmod.ID = filename.ToLower();
                    if (System.IO.File.Exists(newpath + $@"\preview.webp"))
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
                System.IO.File.WriteAllText(root + filename, jsonString);
                return null;
            }
            else
            {
                var jsonoptions = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                string jsonString = System.IO.File.ReadAllText(root + filename);
                what = JsonSerializer.Deserialize<List<string>>(jsonString, jsonoptions);
                return what;
            }
        }

        private void AssignAlert_Click(object sender, RoutedEventArgs e)
        {
            Alert aw = new Alert("This may not work as intended. \nIf it doesn't, let me know along with the mod's files.json", false);
            aw.ShowDialog();
        }

        private void ChangeBox_SelectionChanged()
        {
            List<string> files = new List<string>();
            List<string> deployfiles = new List<string>();
            List<string> config = new List<string>();
            List<string> userconfig = new List<string>();
            if (System.IO.File.Exists($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Mods\{ModID0.Text}\files.json"))
            {
                try
                {
                    files = QuickJson(false, null, $@"Mods\{ModID0.Text}\files.json");
                    for (int i = 0; i < files.Count; i++)
                    {
                        deployfiles.Add(files[i]);
                        for (int ii = 0; ii <= 7; ii++)
                        {
                            if (files[i].Contains($"c0{ii}"))
                            {
                                deployfiles[i] = deployfiles[i].Replace($"c0{ii}", $"c0{assignboxes[ii].SelectedIndex}");
                            }
                            if ((files[i].Contains("\\ui\\replace\\chara\\") || files[i].Contains("\\ui\\replace_patch\\chara\\")) && files[i].Contains($"0{ii}.bntx"))
                            {
                                deployfiles[i] = deployfiles[i].Replace($"0{ii}.bntx", $"0{assignboxes[ii].SelectedIndex}.bntx");
                            }
                            if (files[i].Contains($"\\config.json"))
                            {
                                files[i] = files[i].Replace($"\\config.json", $"\\userconfig.json");
                            }
                            if (deployfiles[i].Contains($"\\userconfig.json"))
                            {
                                files[i] = files[i].Replace($"\\userconfig.json", $"\\config.json");
                            }
                        }
                    }
                    QuickJson(true, deployfiles, $@"Mods\{ModID0.Text}\deploy.json");
                }
                catch (Exception ex)
                {
                    ParallelLogger.Log($@"[ERROR] Couldn't open \Mods\{ModID0.Text} ({ex.Message})");
                }
            }
            if (System.IO.File.Exists($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Mods\{ModID0.Text}\config.json"))
            {
                try
                {
                    config = new List<string>(System.IO.File.ReadAllLines($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Mods\{ModID0.Text}\config.json"));
                    for (int i = 0; i < config.Count; i++)
                    {
                        userconfig.Add(config[i]);
                        for (int ii = 0; ii <= 7; ii++)
                        {
                            if (config[i].Contains($"c0{ii}"))
                            {
                                userconfig[i] = userconfig[i].Replace($"c0{ii}", $"c0{assignboxes[ii].SelectedIndex}");
                            }
                            if (config[i].Contains("\\ui\\replace\\chara\\") && config[i].Contains($"0{ii}.bntx"))
                            {
                                userconfig[i] = userconfig[i].Replace($"0{ii}.bntx", $"0{assignboxes[ii].SelectedIndex}.bntx");
                            }
                        }
                    }
                    QuickJson(true, files, $@"Mods\{ModID0.Text}\files.json");
                    QuickJson(true, deployfiles, $@"Mods\{ModID0.Text}\deploy.json");
                    System.IO.File.WriteAllLines($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Mods\{ModID0.Text}\config.json", ListToArray(config));
                    System.IO.File.WriteAllLines($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Mods\{ModID0.Text}\userconfig.json", ListToArray(userconfig));
                }
                catch (Exception ex)
                {
                    ParallelLogger.Log($@"[ERROR] Couldn't open \Mods\{ModID0.Text} ({ex.Message})");
                }
            }
        }

        public static string[] ListToArray(List<string> sender)
        {
            string[] send = new string[sender.Count];
            for (var i = 0; i < sender.Count; ++i)
                send[i] = sender[i];
            return send;
        }

        private void ChangeBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ChangeBox_SelectionChanged();
        }

        private void ChangeBox2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ChangeBox_SelectionChanged();
        }

        private void ChangeBox3_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ChangeBox_SelectionChanged();
        }

        private void ChangeBox4_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ChangeBox_SelectionChanged();
        }

        private void ChangeBox5_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ChangeBox_SelectionChanged();
        }

        private void ChangeBox6_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ChangeBox_SelectionChanged();
        }

        private void ChangeBox7_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ChangeBox_SelectionChanged();
        }

        private void ChangeBox8_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ChangeBox_SelectionChanged();
        }

        private void DefPrevBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isInitialized) return;
            PathBox_TextChanged(null, null);
            Refresh();
        }

        private void BlacklistAdd_Click(object sender, RoutedEventArgs e)
        {
            string settingspath = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\settings.json";
            string jsonString = System.IO.File.ReadAllText(settingspath);
            var jsonoptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            Settings settings = JsonSerializer.Deserialize<Settings>(jsonString, jsonoptions);
            try
            {
                if (settings.Blacklist == null)
                    settings.Blacklist = new List<string>();
                if (!string.IsNullOrWhiteSpace(BlacklistBox.SelectedItem.ToString()))
                    settings.Blacklist.Add(BlacklistBox.SelectedItem.ToString());
            }
            catch { }
            jsonString = JsonSerializer.Serialize(settings, jsonoptions);
            System.IO.File.WriteAllText(settingspath, jsonString);
            settings = JsonSerializer.Deserialize<Settings>(jsonString, jsonoptions);
            Refresh();
        }

        private void BlacklistClear_Click(object sender, RoutedEventArgs e)
        {
            string settingspath = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\settings.json";
            string jsonString = System.IO.File.ReadAllText(settingspath);
            var jsonoptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            Settings settings = JsonSerializer.Deserialize<Settings>(jsonString, jsonoptions);
            settings.Blacklist = new List<string>();
            jsonString = JsonSerializer.Serialize(settings, jsonoptions);
            System.IO.File.WriteAllText(settingspath, jsonString);
            settings = JsonSerializer.Deserialize<Settings>(jsonString, jsonoptions);
            Refresh();
        }

        private void BlacklistOn_Checked(object sender, RoutedEventArgs e)
        {
            if (!isInitialized) return;
            PathBox_TextChanged(null, null);
        }
        private void BlacklistOn_Unchecked(object sender, RoutedEventArgs e)
        {
            if (!isInitialized) return;
            PathBox_TextChanged(null, null);
        }

        private void Rescan_OnClick(object sender, RoutedEventArgs e)
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
                            System.IO.File.Create($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Mods\{row.ID}\rescan").Dispose();
                        }
                        catch (Exception ex)
                        {
                            ParallelLogger.Log($@"[ERROR] Couldn't open \Mods\{row.ID} ({ex.Message})");
                        }
                    }
                }
            }
            Refresh();
        }
    }
}
