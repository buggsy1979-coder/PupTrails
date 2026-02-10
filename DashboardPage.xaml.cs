using System;
using System.Windows;
using System.Windows.Controls;

namespace PupTrailsV3
{
    public partial class DashboardPage : Page
    {
        public event EventHandler<string>? NavigateToPage;

        public DashboardPage()
        {
            InitializeComponent();
        }

        private void DashboardButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string pageName)
            {
                NavigateToPage?.Invoke(this, pageName);
            }
        }
    }
}
