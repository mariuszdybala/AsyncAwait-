using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MyLogin
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            InitializeComponent();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowLoginBusyIndicator(true);

                LoginButton.Content  =   LoginAsync();

                ShowLoginBusyIndicator(false);
            }
            catch (Exception)
            {
                // Raygun
            }
        }

        private async Task<string> LoginAsync()
        {
            var loginTask = await Task.Run(() => {
                Thread.Sleep(2000);
                return "Login successful!";

            });

            return  loginTask;
        }

        private void ShowLoginBusyIndicator(bool show)
        {
            if (show)
            {
                BusyIndicator.IsEnabled = false;
                BusyIndicator.Visibility = Visibility.Visible;
            }
            else
            {
                BusyIndicator.IsEnabled = true;
                BusyIndicator.Visibility = Visibility.Hidden;
            }
        }
    }
}
