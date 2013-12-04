using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace Zhihu
{
    public partial class SettingsPage : PhoneApplicationPage
    {
        public SettingsPage()
        {
            InitializeComponent();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("", "确定要退出？", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                return;
            ZhihuAPI.Account accout = new ZhihuAPI.Account();
            accout.Logout();
            NavigationService.Navigate(new Uri("/StartPage.xaml?type=logout", UriKind.Relative));
        }
    }
}