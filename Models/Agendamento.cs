using AgendaNovo.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using ControlzEx.Standard;
using System;
using AgendaNovo.Helpers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AgendaNovo.Agendamento;

namespace AgendaNovo
{
    public partial class Agendamento : ObservableObject
    {

        public int Id { get; set; }

        public int ClienteId { get; set; }
        public Cliente ?Cliente { get; set; }   
        public int? CriancaId { get; set; }
        public Crianca? Crianca { get; set; }
        
        public Servico? Servico { get; set; }
        public int? ServicoId { get; set; }
        public Pacote? Pacote { get; set; }
        public int? PacoteId { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ValorPago))]
        public ObservableCollection<Pagamento> pagamentos = new();

        [ObservableProperty] private DateTime data = DateTime.Today;

        [ObservableProperty] private TimeSpan? horario;

        [ObservableProperty] private string? tema;

        [ObservableProperty] private decimal valor;

        [ObservableProperty] private StatusAgendamento status = StatusAgendamento.Pendente;

        [ObservableProperty] private FotosReveladas fotos;
        [ObservableProperty] private TipoEntrega tipoEntrega = TipoEntrega.Foto;

        [NotMapped] public decimal ValorPago => Pagamentos?.Sum(p => p.Valor) ?? 0m;
        [NotMapped] public int? NumeroMes { get; set; }
        

        public int? Mesversario { get; set; }

        public bool EstaPago => Math.Round(Valor, 2) <= Math.Round(ValorPago, 2);
        public bool Pago { get; set; }

        public ICollection<AgendamentoProduto> AgendamentoProdutos { get; set; } = new List<AgendamentoProduto>();
        public ObservableCollection<AgendamentoEtapa> Etapas { get; } = new ();

        // Reserva (Taxa inicial)
        [NotMapped] public bool TemReserva => Pagamentos?.Any(p => p.Tipo == TipoLancamento.Reserva) == true;
        [NotMapped] public string? MesReserva => Pagamentos ?.FirstOrDefault(p => p.Tipo == TipoLancamento.Reserva)?.Observacao;
        [NotMapped] public decimal? ValorReserva => Pagamentos.FirstOrDefault(p => p.Tipo == TipoLancamento.Reserva)?.Valor;
         
        public string? MesversarioFormatado
        {
            get
            {
                if (Mesversario == null)
                    return null;

                if (Mesversario < 12)
                    return $"{Mesversario} {(Mesversario == 1 ? "mês" : "meses")}";

                int anos = Mesversario.Value / 12;
                int mesesRestantes = Mesversario.Value % 12;

                if (mesesRestantes == 0)
                    return $"{anos} {(anos == 1 ? "ano" : "anos")}";

                return $"{anos} {(anos == 1 ? "ano" : "anos")} e {mesesRestantes} {(mesesRestantes == 1 ? "mês" : "meses")}";
            }
        }
        public partial class Pagamento : ObservableObject
        {
            public int Id { get; set; }
            public int AgendamentoId { get; set; }
            public Agendamento Agendamento { get; set; } = null!;

            public int? AgendamentoProdutoId { get; set; }
            public TipoLancamento Tipo { get; set; } = TipoLancamento.Pagamento;

            [ObservableProperty] private decimal valor;            // valor da parcela
            [ObservableProperty] private DateTime dataPagamento;   // quando pagou
            [ObservableProperty] private MetodoPagamento metodo;           // “PIX”, “Crédito”, etc. (opcional)
            [ObservableProperty] private string? observacao;       // opcional
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        }
       
        public partial class AgendamentoProduto : ObservableObject
        {
            public int Id { get; set; }

            public int AgendamentoId { get; set; }
            public Agendamento Agendamento { get; set; } = null!;

            public int ProdutoId { get; set; }
            public Produto Produto { get; set; } = null!;

            public int Quantidade { get; set; } = 1;
            public decimal ValorUnitario { get; set; }
            public decimal ValorTotal => Quantidade * ValorUnitario;
            public DateTime CreatedAt { get; set; } = DateTime.Now;
            [ObservableProperty] private DateTime? enviadoParaProducaoEm;
            [ObservableProperty] private int prazoProducaoDias = 15;
            [ObservableProperty] private DateTime? producaoConcluidaEm;

            [NotMapped]
            public bool ProducaoConcluida => ProducaoConcluidaEm.HasValue;
        }


        partial void OnPagamentosChanged(ObservableCollection<Pagamento> value)
        {
            if (value != null)
                value.CollectionChanged += Pagamentos_CollectionChanged;


            OnPropertyChanged(nameof(ValorPago));
        }
        private void Pagamentos_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(ValorPago));
        }
        public void NotifyEtapasChanged() => RaiseEtapaFlags();
        private void RaiseEtapaFlags()
        {
            OnPropertyChanged(nameof(Etapas));
            OnPropertyChanged(nameof(EscolhaConcluida));
            OnPropertyChanged(nameof(TratamentoConcluido));
            OnPropertyChanged(nameof(Reveladas));       // ou Produção, conforme seu enum
            OnPropertyChanged(nameof(Entregue));
        }
        [NotMapped]
        public bool EscolhaConcluida => Etapas?.Any(e => e.Etapa == EtapaFotos.Escolha) == true;
        [NotMapped]
        public bool TratamentoConcluido => Etapas?.Any(e => e.Etapa == EtapaFotos.Tratamento) == true;
        [NotMapped]
        public bool Reveladas => Etapas?.Any(e => e.Etapa == EtapaFotos.Revelar) == true;
        [NotMapped]
        public bool Entregue => Etapas?.Any(e => e.Etapa == EtapaFotos.Entrega) == true;
        [NotMapped] public DateTime? EscolhaEm => Etapas?.FirstOrDefault(e => e.Etapa == EtapaFotos.Escolha)?.DataConclusao;
        [NotMapped] public DateTime? TratamentoEm => Etapas?.FirstOrDefault(e => e.Etapa == EtapaFotos.Tratamento)?.DataConclusao;
        [NotMapped] public DateTime? RevelarEm => Etapas?.FirstOrDefault(e => e.Etapa == EtapaFotos.Revelar)?.DataConclusao;
        [NotMapped] public DateTime? EntregaEm => Etapas?.FirstOrDefault(e => e.Etapa == EtapaFotos.Entrega)?.DataConclusao;
        [NotMapped] public int PrazoTratarDiasEfetivo => Servico?.PrazoTratarDias > 0 ? Servico.PrazoTratarDias : 3;
        [NotMapped] public DateTime PrevistoTratamento => BusinessDays.AddBusinessDays(Data.Date, PrazoTratarDiasEfetivo);
        [NotMapped] public DateTime PrevistoRevelar => BusinessDays.AddBusinessDays(Data.Date, 15);
        [NotMapped] public DateTime PrevistoEntrega => BusinessDays.AddBusinessDays(Data.Date, 30);
        [NotMapped] public EtapaStatus StatusEscolha => EscolhaEm.HasValue ? EtapaStatus.Concluido : EtapaStatus.Pendente;
        [NotMapped] public EtapaStatus StatusTratamento => CalcStatus(TratamentoEm, PrevistoTratamento);
        [NotMapped] public EtapaStatus StatusRevelar => CalcStatus(RevelarEm, PrevistoRevelar);
        [NotMapped] public EtapaStatus StatusEntrega => CalcStatus(EntregaEm, PrevistoEntrega);
        private static EtapaStatus CalcStatus(DateTime? concluidoEm, DateTime previsto)
        {
            if (concluidoEm.HasValue) return EtapaStatus.Concluido;
            var hoje = DateTime.Today;
            if (hoje > previsto.Date) return EtapaStatus.Atrasado;
            if (hoje == previsto.Date) return EtapaStatus.Hoje;
            return EtapaStatus.Pendente;
        }


    }
}
