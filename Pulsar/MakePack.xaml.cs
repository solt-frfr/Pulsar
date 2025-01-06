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

namespace Pulsar
{
    /// <summary>
    /// Interaction logic for MakePack.xaml
    /// </summary>
    public partial class MakePack : Window
    {
        private Meta modmetadata = new Meta();
        public MakePack(Meta sender)
        {
            InitializeComponent();
            modmetadata = sender;
            try
            {
                if (sender.Name != null)
                {
                    Title = $"Edit {sender.Name}";
                    NameBox.Text = sender.Name;
                    DescBox.Text = sender.Description;
                    TypeBox.Text = sender.Type;
                    CatBox.Text = sender.Category;
                    VersionBox.Text = sender.Version;
                    LinkBox.Text = sender.Link;
                    IDBox.Text = sender.ID;
                    OpenButton.IsEnabled = !sender.ArchiveImage;
                }
            }
            catch 
            {
                Close();
            }
        }
        private bool UserID = false;
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
            filepath = path + $@"\{IDBox.Text}\Preview.png";
            if (File.Exists(PreviewBox.Text))
            {
                System.IO.File.Copy(PreviewBox.Text, filepath, true);
                AemulusModManager.Utilities.ParallelLogger.Log($"[INFO] Preview.png written to {filepath}.");
            }
            Close();
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
    }
}
