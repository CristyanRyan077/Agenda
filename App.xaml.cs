using System.Configuration;
using System.Data;
using System.Globalization;
using System.Windows;
using HandyControl.Tools;

namespace AgendaNovo
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            ConfigHelper.Instance.SetLang("pt-br");
            base.OnStartup(e);


            Login loginWindow = new Login();
            loginWindow.Show();

        }

    }
}
