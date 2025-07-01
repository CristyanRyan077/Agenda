using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using AgendaNovo.Models;
using System.Windows.Data;

namespace AgendaNovo
{
    public partial class AgendaViewModel : ObservableObject
    {
        //Agendamento
        [ObservableProperty] private Agendamento novoAgendamento = new();
        [ObservableProperty] private ObservableCollection<Agendamento> listaAgendamentos = new();
        [ObservableProperty] private ObservableCollection<Agendamento> agendamentosFiltrados = new();

        //Cliente
        [ObservableProperty] private Cliente? clienteSelecionado;
        [ObservableProperty] private Cliente novoCliente = new();
        [ObservableProperty] private ObservableCollection<Cliente> listaClientes = new();

        //Data e horario
        [ObservableProperty] private DateTime dataSelecionada = DateTime.Today;
        [ObservableProperty] private ObservableCollection<string> horariosDisponiveis = new();

        private readonly List<string> _horariosFixos = new()
        {
            "8:00", "9:00", "10:00", "13:00", "14:00", "15:00", "16:00", "17:00", "18:00"
        };

        private DayOfWeek _diaAtual;
        public DayOfWeek DiaAtual
        {
            get => _diaAtual;
            set
            {
                SetProperty(ref _diaAtual, value);
                OnPropertyChanged(nameof(DiaChkSeg));
                OnPropertyChanged(nameof(DiaChkTer));
                OnPropertyChanged(nameof(DiaChkQua));
                OnPropertyChanged(nameof(DiaChkQui));
                OnPropertyChanged(nameof(DiaChkSex));
                OnPropertyChanged(nameof(DiaChkSab));
                OnPropertyChanged(nameof(DiaChkDom));
            }
        }

        public bool DiaChkSeg => DiaAtual == DayOfWeek.Monday;
        public bool DiaChkTer => DiaAtual == DayOfWeek.Tuesday;
        public bool DiaChkQua => DiaAtual == DayOfWeek.Wednesday;
        public bool DiaChkQui => DiaAtual == DayOfWeek.Thursday;
        public bool DiaChkSex => DiaAtual == DayOfWeek.Friday;
        public bool DiaChkSab => DiaAtual == DayOfWeek.Saturday;
        public bool DiaChkDom => DiaAtual == DayOfWeek.Sunday;

        private void FiltrarAgendamentos()
        {
            AgendamentosFiltrados.Clear();

            var filtrados = ListaAgendamentos
                .Where(a => a.Data.Date == DataSelecionada.Date);   
            foreach (var agendamento in filtrados)
                AgendamentosFiltrados.Add(agendamento);
        }

        partial void OnDataSelecionadaChanged(DateTime value)
        {
            FiltrarAgendamentos();
            AtualizarHorariosDisponiveis();
        }
        public void AtualizarHorariosDisponiveis()
        {
            var ocupados = ListaAgendamentos
                .Where(a => a.Data.Date == DataSelecionada.Date)
                .Select(a => a.Horario)
                .ToList();

            var livres = _horariosFixos
                .Where(h => !ocupados.Contains(h))
                .ToList();

            HorariosDisponiveis.Clear();
            foreach (var h in livres)
                HorariosDisponiveis.Add(h);
        }

        partial void OnNovoAgendamentoChanged(Agendamento? value)
        {
            if (value?.Cliente != null)
            {
                NovoCliente = new Cliente
                {
                    Nome = value.Cliente.Nome,
                    Telefone = value.Cliente.Telefone
                };

                ClienteSelecionado = ListaClientes
                    .FirstOrDefault(c => c.Nome == NovoCliente.Nome);
            }
            else
            {
                NovoCliente = new Cliente();
                ClienteSelecionado = null;
            }
        }


        public AgendaViewModel()
        {
            AtualizarHorariosDisponiveis();
            DiaAtual = DateTime.Today.DayOfWeek;
            NovoAgendamento = new Agendamento
            {
                Cliente = NovoCliente
            };
            novoCliente = new Cliente();
            ListaAgendamentos = new ObservableCollection<Agendamento>();
            ListaClientes = new ObservableCollection<Cliente>();
            agendamentosFiltrados = new ObservableCollection<Agendamento>();
        }

        [RelayCommand] private void Agendar()
        {
            if (NovoAgendamento == null)
                NovoAgendamento = new Agendamento();
            if (string.IsNullOrWhiteSpace(NovoCliente.Nome))
                return;
            var clienteExistente = ListaClientes.FirstOrDefault(c => c.Nome == NovoCliente.Nome);
            if (clienteExistente == null)
            {
                ListaClientes.Add(new Cliente
                {
                    Nome = NovoCliente.Nome,
                    Telefone = NovoCliente.Telefone,
                    //Crianca = NovoCliente.Crianca
                });
            }


            

            var novo = new Agendamento
            {
                Cliente = new Cliente
                {
                    Nome = NovoCliente.Nome,
                    //Crianca = NovoCliente.Crianca,
                    Telefone = NovoCliente.Telefone,
                },
                Pacote = NovoAgendamento.Pacote,
                Horario = NovoAgendamento.Horario,
                Data = NovoAgendamento.Data.Date,
                Tema = NovoAgendamento.Tema
            };

            ListaAgendamentos.Add(novo);


            NovoAgendamento = new Agendamento { Data = NovoAgendamento.Data.Date };
            NovoCliente = new Cliente();
            AtualizarAgendamentos();
            AtualizarHorariosDisponiveis();

        }
        private void AtualizarAgendamentos()
        {
            OnPropertyChanged(nameof(AgendamentosDomingo));
            OnPropertyChanged(nameof(AgendamentosSegunda));
            OnPropertyChanged(nameof(AgendamentosTerca));
            OnPropertyChanged(nameof(AgendamentosQuarta));
            OnPropertyChanged(nameof(AgendamentosQuinta));
            OnPropertyChanged(nameof(AgendamentosSexta));
            OnPropertyChanged(nameof(AgendamentosSabado));
        }
        [RelayCommand]
        private void Excluir()
        {
            ListaAgendamentos.Remove(NovoAgendamento);
            AtualizarAgendamentos();

            bool clienteAindaTemAgendamentos = ListaAgendamentos.Any(a =>
            a.Cliente?.Nome == clienteSelecionado?.Nome);

            if (!clienteAindaTemAgendamentos && clienteSelecionado != null)
            {
                var clienteNaLista = ListaClientes.FirstOrDefault(c => c.Nome == clienteSelecionado.Nome);
                if (clienteNaLista != null)
                    ListaClientes.Remove(clienteNaLista);
            }
            NovoCliente = new Cliente();
            NovoAgendamento = new Agendamento
            {
                Cliente = new Cliente()
            };
            novoCliente.Telefone = string.Empty;
            ClienteSelecionado = null;
            OnPropertyChanged(nameof(ClienteSelecionado));
            ListaClientes = new ObservableCollection<Cliente>(ListaClientes.ToList());
        }
        private IEnumerable<Agendamento> FiltrarPorDia(DayOfWeek dia) =>
        ListaAgendamentos.Where(a =>
        a.Data.DayOfWeek == dia &&
        a.Data.Date >= DateTime.Today &&
        a.Data.Date <= FimDaSemana);

        private DateTime FimDaSemana => DateTime.Today.AddDays(6);

        public IEnumerable<Agendamento> AgendamentosDomingo => FiltrarPorDia(DayOfWeek.Sunday);


        public IEnumerable<Agendamento> AgendamentosSegunda => FiltrarPorDia(DayOfWeek.Monday);


        public IEnumerable<Agendamento> AgendamentosTerca => FiltrarPorDia(DayOfWeek.Tuesday);

        public IEnumerable<Agendamento> AgendamentosQuarta => FiltrarPorDia(DayOfWeek.Wednesday);


        public IEnumerable<Agendamento> AgendamentosQuinta => FiltrarPorDia(DayOfWeek.Thursday);

        public IEnumerable<Agendamento> AgendamentosSexta => FiltrarPorDia(DayOfWeek.Friday);


        public IEnumerable<Agendamento> AgendamentosSabado => FiltrarPorDia(DayOfWeek.Saturday);
    }


}
