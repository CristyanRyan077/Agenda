using System.Configuration;
using System.Data;
using System.Globalization;
using System.Windows;

namespace AgendaNovo
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private AgendaViewModel vm = new AgendaViewModel();
        protected override void OnStartup(StartupEventArgs e)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("pt-BR");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("pt-BR");
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("pt-BR");
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("pt-BR");
            HandyControl.Properties.Langs.Lang.Culture = new CultureInfo("pt-BR");
            base.OnStartup(e);


            Login loginWindow = new Login();
            loginWindow.Show();

        }

    }
}
