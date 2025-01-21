using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Reflection;
using System.IO;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.LinkLabel;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using System.Diagnostics;

namespace Pulsar
{
    /// <summary>
    /// Interaction logic for MakePack.xaml
    /// </summary>
    public partial class MakePack : Window
    {
        private Meta modmetadata = new Meta();
        private List<string> authors = new List<string>();
        private List<string> tags = new List<string>();
        private List<System.Windows.Controls.TextBox> authorboxes = new List<System.Windows.Controls.TextBox>();
        private List<System.Windows.Controls.TextBox> tagboxes = new List<System.Windows.Controls.TextBox>();
        private bool UserID = false;
        public MakePack(Meta sender)
        {
            this.Topmost = true;
            InitializeComponent();
            modmetadata = sender;
            try
            {
                if (sender.Name != null || sender.ID != null)
                {
                    Title = $"Edit {sender.Name}";
                    NameBox.Text = sender.Name;
                    DescBox.Text = sender.Description;
                    TypeBox.Text = sender.Type;
                    CatBox.Text = sender.Category;
                    VersionBox.Text = sender.Version;
                    LinkBox.Text = sender.Link;
                    IDBox.Text = sender.ID;
                    if (!string.IsNullOrWhiteSpace(sender.ID))
                    {
                        IDBox.IsEnabled = false;
                        UserID = true;
                    }
                    OpenButton.IsEnabled = !sender.ArchiveImage;
                    authors = sender.Authors;
                    tags = sender.Tags;
                    InfoCategoryBox.SelectedIndex = sender.InfoCat;
                    try
                    {
                        authorboxes.Add(AuthorBox0);
                        authorboxes[0].Text = authors[0];
                        for (int i = 1; i < authors.Count; i++)
                        {
                            Add_Click(new object(), new RoutedEventArgs());
                            authorboxes[i].Text = authors[i];
                        }
                    }
                    catch { }
                    try
                    {
                        tagboxes.Add(TagBox0);
                        tagboxes[0].Text = tags[0];
                        for (int i = 1; i < tags.Count; i++)
                        {
                            AddTag_Click(new object(), new RoutedEventArgs());
                            tagboxes[i].Text = tags[i];
                        }
                    }
                    catch { }
                }
            }
            catch
            {
                Close();
            }
            if (!authorboxes.Contains(AuthorBox0))
                authorboxes.Add(AuthorBox0);
        }
        private void Open_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openPng = new System.Windows.Forms.OpenFileDialog();
            openPng.Filter = "Preview Image (*.*)|*.*";
            openPng.Title = "Select Preview";
            if (openPng.ShowDialog() != null)
            {
                PreviewBox.Text = openPng.FileName;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            modmetadata.Name = NameBox.Text;
            modmetadata.Description = DescBox.Text;
            modmetadata.Type = TypeBox.Text;
            modmetadata.Category = CatBox.Text;
            modmetadata.Version = VersionBox.Text;
            modmetadata.Link = LinkBox.Text;
            modmetadata.ID = IDBox.Text;
            modmetadata.Authors = new List<string>();
            modmetadata.InfoCat = InfoCategoryBox.SelectedIndex;
            modmetadata.Tags = new List<string>();
            if (string.IsNullOrWhiteSpace(modmetadata.ID))
            {
                AemulusModManager.Utilities.ParallelLogger.Log($"[ERROR] The ID was blank.");
                Close();
            }
            else
            {
                try
                {
                    for (int i = 0; i < authorboxes.Count; i++)
                    {
                        if (!string.IsNullOrWhiteSpace(authorboxes[i].Text))
                            modmetadata.Authors.Add(authorboxes[i].Text);
                    }
                }
                catch (Exception ex)
                {
                    AemulusModManager.Utilities.ParallelLogger.Log($"[ERROR] {ex}");
                }
                try
                {
                    for (int i = 0; i < tagboxes.Count; i++)
                    {
                        if (!string.IsNullOrWhiteSpace(tagboxes[i].Text))
                            modmetadata.Tags.Add(tagboxes[i].Text);
                    }
                }
                catch (Exception ex)
                {
                    AemulusModManager.Utilities.ParallelLogger.Log($"[ERROR] {ex}");
                }
                string path = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Mods";
                var jsonoptions = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                string jsonString = JsonSerializer.Serialize(modmetadata, jsonoptions);
                string filepath = path + $@"\{IDBox.Text}\meta.json";
                Directory.CreateDirectory(path + $@"\{IDBox.Text}");
                File.WriteAllText(filepath, jsonString);
                AemulusModManager.Utilities.ParallelLogger.Log($"[INFO] meta.json written to {filepath}.");
                filepath = path + $@"\{IDBox.Text}\preview.webp";
                if (File.Exists(PreviewBox.Text))
                {
                    using (SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(PreviewBox.Text))
                    {
                        image.Save(filepath, new WebpEncoder());
                        AemulusModManager.Utilities.ParallelLogger.Log($"[INFO] Image converted to WebP successfully!");
                    }
                }
                if (OverrideBox.IsChecked == true)
                {
                    NewInfo();
                }
                Close();
            }
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.TextBox newTextBox = new System.Windows.Controls.TextBox
            {
                Name = $"AuthorBox{authorboxes.Count}",
                Height = 20,
                Width = 179,
                Margin = new Thickness(5),
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#a10943"),
                Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#ffd9da"),
            };
            try
            {
                newTextBox.Text = authors[authorboxes.Count];
            }
            catch { }
            TextBoxContainer.Children.Add(newTextBox);
            authorboxes.Add(newTextBox);
            Height = Height + 30;
        }
        private void AddTag_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.TextBox newTextBox = new System.Windows.Controls.TextBox
            {
                Name = $"TagBox{tagboxes.Count}",
                Height = 20,
                Width = 179,
                Margin = new Thickness(5),
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#a10943"),
                Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#ffd9da"),
            };
            try
            {
                newTextBox.Text = tags[tagboxes.Count];
            }
            catch { }
            TagBoxContainer.Children.Add(newTextBox);
            tagboxes.Add(newTextBox);
            Height = Height + 30;
        }

        public static string InfoCatDeEnum(int i)
        {
            if (i == 0)
            {
                return "Fighter";
            }
            else if (i == 1)
            {
                return "Stage";
            }
            else if (i == 2)
            {
                return "Effects";
            }
            else if (i == 3)
            {
                return "UI";
            }
            else if (i == 4)
            {
                return "Param";
            }
            else if (i == 5)
            {
                return "Music";
            }
            else if (i == 6)
            {
                return "Misc";
            }
            else
            {
                return null;
            }
        }
        public static int InfoCatEnum(string str)
        {
            if (str.Contains("Fighter"))
            {
                return 0;
            }
            else if (str.Contains("Stage"))
            {
                return 1;
            }
            else if (str.Contains("Effects"))
            {
                return 2;
            }
            else if (str.Contains("UI"))
            {
                return 3;
            }
            else if (str.Contains("Param"))
            {
                return 4;
            }
            else if (str.Contains("Music"))
            {
                return 5;
            }
            else if (str.Contains("Misc"))
            {
                return 6;
            }
            else
            {
                return 0;
            }
        }

        private void NameChanged(object sender, TextChangedEventArgs e)
        {
            string idtext = NameBox.Text.Trim();
            idtext = idtext.ToLower();
            idtext = idtext.Replace(" ", string.Empty);
            if (UserID == false)
                IDBox.Text = idtext;
        }

        private void IDChanged(object sender, TextChangedEventArgs e)
        {
            string idtext = IDBox.Text.Trim();
            idtext = idtext.ToLower();
            idtext = idtext.Replace(" ", string.Empty);
            IDBox.Text = idtext;
        }
        private void IDBox_KeyDown(object sender, KeyPressEventArgs e)
        {
            UserID = true;
            e.Handled = !char.IsLetterOrDigit(e.KeyChar) || !char.IsPunctuation(e.KeyChar);
            if (e.Handled == false)
            {
                IDBox.Text = IDBox.Text.TrimEnd(e.KeyChar);
            }
        }

        private void IDBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            UserID = true;
        }

        private void NewInfo()
        {
            string filePath = $@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Mods\{modmetadata.ID}\info.toml";
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine($"display_name = \"{modmetadata.Name}\"");
                writer.Write($"authors = \"{modmetadata.Authors[0]}");
                for (int i = 1; i < modmetadata.Authors.Count; i++)
                {
                    writer.Write($", {modmetadata.Authors[i]}");
                }
                writer.WriteLine("\"");
                writer.WriteLine($"version = \"{modmetadata.Version}\"");
                writer.WriteLine("description = \"\"\"");
                writer.WriteLine($"{modmetadata.Description}");
                writer.WriteLine("\"\"\"");
                writer.WriteLine($"category = \"{InfoCatDeEnum(modmetadata.InfoCat)}\"");
                AemulusModManager.Utilities.ParallelLogger.Log($"[INFO] info.toml written to {filePath}.");
            }
        }
    }
}
