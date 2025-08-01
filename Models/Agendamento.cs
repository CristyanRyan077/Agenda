﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgendaNovo.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using ControlzEx.Standard;

namespace AgendaNovo
{
    public partial class Agendamento : ObservableObject
    {
        public Agendamento()
        {
            MostrarCheck = true;
        }
        public int Id { get; set; }

        public int ClienteId { get; set; }
        public Cliente ?Cliente { get; set; }   
        public int? CriancaId { get; set; }
        public Crianca? Crianca { get; set; }
        
        public Servico? Servico { get; set; }
        public int? ServicoId { get; set; }
        public Pacote? Pacote { get; set; }
        public int? PacoteId { get; set; }

        [ObservableProperty] private TimeSpan? horario;

        [ObservableProperty] private string? tema;

        [ObservableProperty] private decimal valor;

        [ObservableProperty] private decimal valorPago;

        [ObservableProperty]
        private StatusAgendamento status = StatusAgendamento.Pendente;


        private bool _mostrarCheck;

        [NotMapped]
        public bool MostrarCheck
        {
            get => _mostrarCheck;
            set => SetProperty(ref _mostrarCheck, value);
        }

        public bool EstaPago => Math.Round(Valor, 2) == Math.Round(ValorPago, 2);
        public bool Pago { get; set; }

        [ObservableProperty] private DateTime data = DateTime.Today;
        partial void OnValorChanged(decimal oldValue, decimal newValue)
        {
            OnPropertyChanged(nameof(EstaPago));
        }
        partial void OnValorPagoChanged(decimal oldValue, decimal newValue)
        {
            OnPropertyChanged(nameof(EstaPago));
        }
    }
}
