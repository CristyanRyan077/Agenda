using AgendaNovo.Interfaces;
using AgendaNovo.Services;
using AgendaNovo.ViewModels;
using AgendaNovo.Views;
using HandyControl.Tools;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
        public IServiceProvider ServiceProvider { get; private set; }
        protected override void OnStartup(StartupEventArgs e)
        {
            ConfigHelper.Instance.SetLang("pt-br");
            base.OnStartup(e);
            var services = new ServiceCollection();
            services.AddDbContext<AgendaContext>(options =>
            //options.UseSqlServer("Data Source=2857AL19;Initial Catalog=AgendaDB;Integrated Security=True;Trust Server Certificate=True;"));
            options.UseSqlServer("Data Source=PCBRANCOGAMER\\SQLEXPRESS;Initial Catalog=AgendaStudio;Integrated Security=True;Trust Server Certificate=True"));

            //Services
            services.AddTransient<IClienteService, ClienteService>();
            services.AddTransient<ICriancaService, CriancaService>();
            services.AddTransient<IPacoteService, PacoteService>();
            services.AddTransient<IServicoService, ServicoService>();
            services.AddTransient<IAgendamentoService, AgendamentoService>();
            //Views
            services.AddSingleton<WindowManager>();
            services.AddTransient<Agendar>();
            services.AddTransient<MainWindow>();
            services.AddTransient<GerenciarClientes>();
            services.AddTransient<Login>();
            services.AddTransient<Calendario>(sp =>
            new Calendario(sp.GetRequiredService<CalendarioViewModel>()));

            //ViewModels
            services.AddTransient<CalendarioViewModel>();
            services.AddTransient<ClienteCriancaViewModel>();
            services.AddSingleton<AgendaViewModel>();

            ServiceProvider = services.BuildServiceProvider();
            var login = ServiceProvider.GetRequiredService<Login>();
            login.Show();

        }

    }
}
