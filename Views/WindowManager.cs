using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
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
        private readonly IServiceProvider _sp;

        public WindowManager(IServiceProvider sp)
        {
            _sp = sp;
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
