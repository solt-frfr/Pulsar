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
            InitializeComponent();
            AlertBlock.Text = sender;
            Title = "Alert!";
            if (!download)
            {
                QuickHide(true);
                WhatBlock.Text = sender;
            }
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            OnAlertHandled?.Invoke();
            Close();
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
