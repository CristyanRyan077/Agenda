using AgendaNovo.Models;
using AgendaNovo.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AgendaNovo.Views
{
    /// <summary>
    /// Lógica interna para Calendario.xaml
    /// </summary>
    public partial class Calendario : Window
    {
        public Calendario(CalendarioViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }

        private void Dia_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is DiaCalendario dia && DataContext is CalendarioViewModel vm)
            {
                vm.SelecionarDia(dia.Data);
            }
        }

        private void btnToggle_Checked(object sender, RoutedEventArgs e)
        {
            var anim = new DoubleAnimation(0, TimeSpan.FromMilliseconds(250));
            DrawerTransform.BeginAnimation(TranslateTransform.XProperty, anim);
        }

        private void btnToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            var anim = new DoubleAnimation(-200, TimeSpan.FromMilliseconds(250));
            DrawerTransform.BeginAnimation(TranslateTransform.XProperty, anim);
        }
        private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is CalendarioViewModel vm && vm.AgendamentoSelecionado != null)
            {
                // Cria instância da AgendaViewModel ou reaproveita uma já existente
                var agendaVM = new AgendaViewModel(
                    vm.AgendamentoService,
                    vm.ClienteService,
                    vm.CriancaService,
                    vm.PacoteService,
                    vm.ServicoService);

                agendaVM.Inicializar();
                var agendamentoCompleto = vm.AgendamentoService.GetById(vm.AgendamentoSelecionado.Id);
                if (agendamentoCompleto == null) return;

                agendaVM.ItemSelecionado = agendamentoCompleto;
                agendaVM.NovoAgendamento = agendamentoCompleto;
                agendaVM.ClienteSelecionado = agendaVM.ListaClientes
                .FirstOrDefault(c => c.Id == agendamentoCompleto.ClienteId);
                agendaVM.NovoCliente = agendaVM.ClienteSelecionado;

                agendaVM.ListaCriancas.Clear();
                foreach (var crianca in agendaVM.ClienteSelecionado.Criancas ?? Enumerable.Empty<Crianca>())
                    agendaVM.ListaCriancas.Add(crianca);

                agendaVM.ServicoSelecionado = agendaVM.ListaServicos
                .FirstOrDefault(s => s.Id == agendamentoCompleto.ServicoId);

                agendaVM.Pacoteselecionado = agendaVM.ListaPacotes
                    .FirstOrDefault(p => p.Id == agendamentoCompleto.PacoteId);

                var janela = new EditarAgendamento
                {
                    DataContext = agendaVM,
                    Owner = this
                };

                if (janela.ShowDialog() == true)
                {
                    vm.SelecionarDia(vm.AgendamentoSelecionado.Data); // recarrega o dia
                }
            }
        }
    }
}
