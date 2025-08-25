using AgendaNovo.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AgendaNovo.Views
{
    public class WindowManager
    {
        private MainWindow? _mainWindow;
        private GerenciarClientes? _clientes;
        private Calendario? _calendario;
        private Agendar? _agendar;
        private Financeiro? _finWin;
        private IServiceScope _finScope;
        private readonly IServiceProvider _sp;

        public WindowManager(IServiceProvider sp)
        {
            _sp = sp;
        }
        public async Task AbrirFinanceiroNaMainAsync()
        {
            var owner = GetMainWindow();

            if (_finWin == null || !_finWin.IsLoaded)
            {
                System.Diagnostics.Debug.WriteLine("[WM] AbrirFinanceiroNaMainAsync START");
                _finScope = _sp.CreateScope();
                // cria janela, exibe rápido
                _finWin = _sp.GetRequiredService<Financeiro>();
                _finWin.Owner = owner;
                _finWin.Closed += (_, __) => _finWin = null;

                var vm = _sp.GetRequiredService<FinanceiroViewModel>();
                _finWin.DataContext = vm;
                _finWin.Show();

                System.Diagnostics.Debug.WriteLine("[WM] calling vm.CarregarAsync()");
                await vm.CarregarAsync();
                System.Diagnostics.Debug.WriteLine("[WM] AbrirFinanceiroNaMainAsync END");
            }
            else
            {
                if (_finWin.WindowState == WindowState.Minimized)
                    _finWin.WindowState = WindowState.Normal;
                _finWin.Activate();
            }
        }



        public MainWindow GetMainWindow()
        {
            if (_mainWindow == null || !_mainWindow.IsLoaded)
            {
                _mainWindow = _sp.GetRequiredService<MainWindow>();
                _mainWindow.Closed += (s, e) => _mainWindow = null;
                _mainWindow.Show();
            }
            else
            {
                if (_mainWindow.WindowState == WindowState.Minimized)
                    _mainWindow.WindowState = WindowState.Normal;
                _mainWindow.Activate();
            }
            return _mainWindow;
        }
        public GerenciarClientes GetGerenciarClientes()
        {
            if (_clientes == null || !_clientes.IsLoaded)
            {
                _clientes = _sp.GetRequiredService<GerenciarClientes>();
                _clientes.Closed += (s, e) => _clientes = null;
                _clientes.Show();
            }
            else
            {
                if (_clientes.WindowState == WindowState.Minimized)
                    _clientes.WindowState = WindowState.Normal;
                _clientes.Activate();
            }
            return _clientes;
        }
        public Calendario GetCalendario()
        {
            if (_calendario == null || !_calendario.IsLoaded)
            {
                _calendario = _sp.GetRequiredService<Calendario>();
                _calendario.Closed += (s, e) => _calendario = null;
                _calendario.Show();
            }
            else
            {
                if (_calendario.WindowState == WindowState.Minimized)
                    _calendario.WindowState = WindowState.Normal;
                _calendario.Activate();
            }
            return _calendario;
        }
        public Agendar GetAgendar()
        {
            if ( _agendar == null || !_agendar.IsLoaded)
            {
                _agendar = _sp.GetRequiredService<Agendar>();
                _agendar.Closed += (s, e) => _agendar = null;
                _agendar.Show();
            }
            else
            {
                if (_agendar.WindowState == WindowState.Minimized)
                    _agendar.WindowState = WindowState.Normal;
                _agendar.Activate();
            }
            return _agendar;
        }


    }
}
