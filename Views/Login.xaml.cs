using System;
using System.Collections.Generic;
using System.Linq;
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
using Microsoft.Extensions.DependencyInjection;
using SplashScreen = AgendaNovo.Views.SplashScreen;

namespace AgendaNovo
{
    /// <summary>
    /// Lógica interna para Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        private readonly IServiceProvider _serviceProvider;
        public Login(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
        }

        private void txtNome_GotFocus(object sender, RoutedEventArgs e)
        {
            txtbNomeplaceholder.Visibility = Visibility.Collapsed;
        }

        private void txtNome_LostFocus(object sender, RoutedEventArgs e)
        {
             if (string.IsNullOrEmpty(txtNome.Text))
             {
                txtbNomeplaceholder.Visibility = Visibility.Visible;
             }
        }

        private void passboxSenha_GotFocus(object sender, RoutedEventArgs e)
        {
            txtbSenhaPlaceholder.Visibility = Visibility.Collapsed;
        }

        private void passboxSenha_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(passboxSenha.Password))
            {
                txtbSenhaPlaceholder.Visibility = Visibility.Visible;
            }
        }
        private bool AutenticacaoLogin(string senha)
        {
            return (string.Equals(senha.Trim(), "cristyan653", StringComparison.OrdinalIgnoreCase));
        }
        private async void LoginSucesso()
        {
            if (AutenticacaoLogin(passboxSenha.Password))
            {
                var splash = new SplashScreen();
                splash.Show();
                await System.Windows.Threading.Dispatcher.Yield(System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                await Task.Delay(350);

                var main = _serviceProvider.GetRequiredService<MainWindow>();
                var vm = ActivatorUtilities.CreateInstance<AgendaViewModel>(_serviceProvider);

                await Task.Run(() =>
                {
                    vm.Inicializar();
                });

                var mainWindow = ActivatorUtilities.CreateInstance<MainWindow>(_serviceProvider, vm);
                mainWindow.Visibility = Visibility.Hidden;
                await Dispatcher.InvokeAsync(() =>
                {
                    mainWindow.Visibility = Visibility.Visible;
                    mainWindow.Show();
                });
                splash.Close();
                this.Close();
            }
            else
            {
                MessageBox.Show("Credenciais inválidas!", "Erro", MessageBoxButton.OK);
            }
        }

        private void passboxSenha_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LoginSucesso();
            }
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            LoginSucesso();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
    }
}
