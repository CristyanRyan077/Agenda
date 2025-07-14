using AgendaNovo.Migrations;
using AgendaNovo.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace AgendaNovo.ViewModels
{
    public class MainViewModel
    {
        public AgendaViewModel AgendaVM { get; set; }
        public  FormularioViewModel FormularioVM { get; set; }
        public StaticDataViewModel StaticVM { get; set; }

        private readonly AgendaContext _db;
        public MainViewModel(AgendaContext db)
        {
            _db = db;
            StaticVM = new StaticDataViewModel(_db);
            AgendaVM = new AgendaViewModel(_db,StaticVM);
            FormularioVM = new (_db);
        }

        public void Inicializar()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                AgendaVM.CarregarDadosDoBanco();
                StaticVM.CarregarPacotes();
                AgendaVM.FiltrarAgendamentos();
                AgendaVM.AtualizarHorariosDisponiveis();
            });
        }
    }
}
