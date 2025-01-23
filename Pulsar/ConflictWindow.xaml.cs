using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Pulsar
{
    /// <summary>
    /// Interaction logic for ConflictWindow.xaml
    /// </summary>
    public partial class ConflictWindow : Window
    {
        public Action OnConflictHandled;
        public ConflictWindow(string sender, string ID, string source, string checkID)
        {
            this.Topmost = true;
            InitializeComponent();
            SourceBlock.Text = System.IO.Path.GetFileName(checkID);
            SenderBlock.Text = ID;
            Cancel.Content = System.IO.Path.GetFileName(checkID);
            Confirm.Content = ID;
            SourceBox.Text = $@"<output>\" + System.IO.Path.GetFileName(checkID) + sender;
            SenderBox.Text = $@"Mods\" + ID + source;
            string jsonString = System.IO.File.ReadAllText($@"{System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\settings.json");
            var jsonoptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            Settings settings = JsonSerializer.Deserialize<Settings>(jsonString, jsonoptions);
            FileBox.Text = settings.DeployPath + $@"\<mod>" + sender;
        }

        

        private async void Confirm_Click(object sender, RoutedEventArgs e)
        {
            WhatBlock.Text = "Working on it...";
            await Task.Delay(100);
            OnConflictHandled?.Invoke();
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}