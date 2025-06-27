using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AgendaNovo
{
    public partial class AgendaViewModel : ObservableObject
    {
        [ObservableProperty] private Agendamento novoAgendamento = new();
        [ObservableProperty] private ObservableCollection<Agendamento> listaAgendamentos = new();
        [ObservableProperty] private Agendamento? agendamentoSelecionado;


        public AgendaViewModel()
        {
            NovoAgendamento = new Agendamento();
            ListaAgendamentos = new ObservableCollection<Agendamento>();
        }

        [RelayCommand] private void Agendar()
        {
            if (NovoAgendamento == null)
                NovoAgendamento = new Agendamento();

            if (!string.IsNullOrWhiteSpace(NovoAgendamento.Cliente))
            {
                ListaAgendamentos.Add(new Agendamento
                {
                    Cliente = NovoAgendamento.Cliente,
                    Pacote = NovoAgendamento.Pacote,
                    Horario = NovoAgendamento.Horario,
                    Data = NovoAgendamento.Data.Date 

                });
                NovoAgendamento = new Agendamento { Data = NovoAgendamento.Data.Date };
                AtualizarAgendamentos();
            }
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
