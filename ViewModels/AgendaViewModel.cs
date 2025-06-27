using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using AgendaNovo.Models;

namespace AgendaNovo
{
    public partial class AgendaViewModel : ObservableObject
    {
        [ObservableProperty] private Agendamento novoAgendamento = new();
        [ObservableProperty] private Cliente novoCliente = new();
        [ObservableProperty] private ObservableCollection<Agendamento> listaAgendamentos = new();
        [ObservableProperty] private Agendamento? agendamentoSelecionado;
        [ObservableProperty] private ObservableCollection<Cliente> listaClientes = new();


        public AgendaViewModel()
        {
            NovoAgendamento = new Agendamento();
            novoCliente = new Cliente();
            ListaAgendamentos = new ObservableCollection<Agendamento>();
            ListaClientes = new ObservableCollection<Cliente>
            {
                new Cliente { Id = 1, Nome = "Maria Silva", Telefone = "1111-1111" },
                new Cliente { Id = 2, Nome = "João Souza", Telefone = "2222-2222" },
                new Cliente { Id = 3, Nome = "Ana Oliveira", Telefone = "3333-3333" }
            };
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
                ListaClientes.Add(NovoCliente);
            }
            else
            {
                NovoCliente = clienteExistente;
            }




            var novo = new Agendamento
            {
                Cliente = new Cliente
                {
                    Nome = NovoCliente.Nome,
                },
                Pacote = NovoAgendamento.Pacote,
                Horario = NovoAgendamento.Horario,
                Data = NovoAgendamento.Data.Date
            };

            ListaAgendamentos.Add(novo);


            NovoAgendamento = new Agendamento { Data = NovoAgendamento.Data.Date };
            NovoCliente = new Cliente();
            AtualizarAgendamentos();
           
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
            NovoAgendamento = new Agendamento();
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
