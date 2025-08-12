using AgendaNovo.Interfaces;
using AgendaNovo.Models;
using AgendaNovo.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgendaNovo.ViewModels
{
    public partial class AdicionarServicoPacoteViewModel : ObservableObject
    {
        private readonly IPacoteService _pacoteService;
        private readonly IServicoService _servicoService;


        public AdicionarServicoPacoteViewModel(IPacoteService pacoteService, IServicoService servicoService)
        {
            _pacoteService = pacoteService;
            _servicoService = servicoService;

        SalvarCommand = new RelayCommand(Salvar, PodeSalvar);
        }

        [ObservableProperty]
        private string? novoServico;

        [ObservableProperty]
        private string? novoPacote;

        [ObservableProperty]
        private decimal valor;

        [ObservableProperty]
        private bool possuiCrianca = true;

        [ObservableProperty]
        private bool possuiAcompanhamentoMensal = true;

        public RelayCommand SalvarCommand { get; }

        partial void OnNovoServicoChanged(string? value)
        {
            SalvarCommand.NotifyCanExecuteChanged();
        }

        partial void OnNovoPacoteChanged(string? value)
        {
            SalvarCommand.NotifyCanExecuteChanged();
        }

        partial void OnValorChanged(decimal value)
        {
            SalvarCommand.NotifyCanExecuteChanged();
        }

        private bool PodeSalvar()
        {
            return !string.IsNullOrWhiteSpace(NovoServico)
                   && !string.IsNullOrWhiteSpace(NovoPacote)
                   && Valor > 0;
        }

        private void Salvar()
        {
            var servico = new Servico
            {
                Nome = NovoServico,
                PossuiCrianca = PossuiCrianca
            };

            _servicoService.Add(servico);

            var pacote = new Pacote
            {
                Nome = NovoPacote,
                Valor = Valor,
                ServicoId = servico.Id,
                possuiAcompanhamentoMensal = PossuiAcompanhamentoMensal
            };

            _pacoteService.Add(pacote);

            // Limpa os campos
            NovoServico = string.Empty;
            NovoPacote = string.Empty;
            Valor = 0;
            PossuiCrianca = true;
            PossuiAcompanhamentoMensal = true;
        }
    }
}
