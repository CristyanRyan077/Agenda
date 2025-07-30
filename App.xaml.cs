using AgendaNovo.Interfaces;
using AgendaNovo.Services;
using AgendaNovo.ViewModels;
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
            //options.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=AgendaDB;"));
            options.UseSqlServer("Data Source=PCBRANCOGAMER\\SQLEXPRESS;Initial Catalog=AgendaStudio;Integrated Security=True;Trust Server Certificate=True"));
            services.AddScoped<ICriancaService, CriancaService>();
            services.AddScoped<IClienteService, ClienteService>();
            services.AddScoped<IPacoteService, PacoteService>();
            services.AddScoped<IServicoService, ServicoService>();
            services.AddScoped<IAgendamentoService, AgendamentoService>();

            services.AddTransient<ClienteService>();
            services.AddTransient<CriancaService>();
            services.AddTransient<PacoteService>();
            services.AddTransient<ServicoService>();
            services.AddTransient<AgendamentoService>();

            services.AddTransient<Agendar>();
            services.AddTransient<MainWindow>();
            services.AddTransient<GerenciarClientes>();
            services.AddTransient<Login>();

            services.AddScoped<ClienteCriancaViewModel>();

            services.AddSingleton<AgendaViewModel>();
            ServiceProvider = services.BuildServiceProvider();


            var login = ServiceProvider.GetRequiredService<Login>();
            login.Show();

        }

    }
}
