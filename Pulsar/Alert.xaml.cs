using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Net.Http;
using System.Runtime.Remoting.Channels;

namespace Pulsar
{
    /// <summary>
    /// Interaction logic for Alert.xaml
    /// </summary>
    public partial class Alert : System.Windows.Window
    {
        public Action OnAlertHandled;
        public Alert(string sender, bool download)
        {
            this.Topmost = true;
            InitializeComponent();
            AlertBlock.Text = sender;
            Title = "Alert!";
            if (!download)
            {
                QuickHide(true);
                WhatBlock.Text = sender;
            }
            else
            {
                QuickHide(true);
                WhatBlock.Text = "Loading...";
            }
            AlertBlock.InvalidateMeasure();
            WhatBlock.InvalidateMeasure();
        }

        

        private async void Confirm_Click(object sender, RoutedEventArgs e)
        {
            WhatBlock.Text = "Working on it...";
            await Task.Delay(100);
            OnAlertHandled?.Invoke();
            Close();
        }

        public void Update(string title, string preview)
        {
            QuickHide(false);
            AlertBlock.Text = title;
            WhatBlock.Text = "Do you want to install?";
            BitmapImage bitmap = new BitmapImage();
            using (FileStream fs = new FileStream(preview, FileMode.Open, FileAccess.Read))
            {
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = fs;
                bitmap.EndInit();
            }
            Preview.Source = bitmap;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void QuickHide(bool hide)
        {
            if (hide)
            {
                AlertBlock.Visibility = Visibility.Collapsed;
                Preview.Visibility = Visibility.Collapsed;
            }
            else
            {
                AlertBlock.Visibility = Visibility.Visible;
                Preview.Visibility = Visibility.Visible;
            }
        }
    }
}
