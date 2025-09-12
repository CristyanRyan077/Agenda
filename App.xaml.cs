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
            var cultura = new System.Globalization.CultureInfo("pt-BR");
            System.Threading.Thread.CurrentThread.CurrentCulture = cultura;
            System.Threading.Thread.CurrentThread.CurrentUICulture = cultura;
            ConfigHelper.Instance.SetLang("pt-br");
            base.OnStartup(e);
            var services = new ServiceCollection();
            services.AddDbContext<AgendaContext>(options =>
            //options.UseSqlServer("Data Source=2857AL17;Initial Catalog=AgendaDB;Integrated Security=True;Trust Server Certificate=True;"));
            options.UseSqlServer("Data Source=PCBRANCOGAMER\\SQLEXPRESS;Initial Catalog=AgendaStudio;Integrated Security=True;Trust Server Certificate=True"));

            services.AddDbContextFactory<AgendaContext>(options => options.UseSqlServer
            ("Data Source=PCBRANCOGAMER\\SQLEXPRESS;Initial Catalog=AgendaStudio;Integrated Security=True;Trust Server Certificate=True"));

            //Services
            services.AddTransient<IClienteService, ClienteService>();
            services.AddTransient<ICriancaService, CriancaService>();
            services.AddTransient<IPacoteService, PacoteService>();
            services.AddTransient<IServicoService, ServicoService>();
            services.AddTransient<IAgendamentoService, AgendamentoService>();
            services.AddTransient<IPagamentoService, PagamentoService>();
            services.AddTransient<INotificacaoService, NotificacaoService>();
            services.AddTransient<IProdutoService, ProdutoService>();
            //Views
            services.AddScoped<WindowManager>();
            services.AddTransient<Agendar>();
            services.AddTransient<MainWindow>();
            services.AddTransient<GerenciarClientes>();
            services.AddTransient<Login>();
            services.AddTransient<Calendario>();
            services.AddTransient<Financeiro>();

            //ViewModels
            services.AddTransient<CalendarioViewModel>();
            services.AddTransient<ClienteCriancaViewModel>();
            services.AddSingleton<AgendaViewModel>();
            services.AddTransient<FinanceiroViewModel>();
            services.AddTransient<PagamentosViewModel>();

            ServiceProvider = services.BuildServiceProvider();
            var login = ServiceProvider.GetRequiredService<Login>();
            login.Show();

        }

    }
}
