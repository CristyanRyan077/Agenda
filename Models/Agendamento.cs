﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgendaNovo.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AgendaNovo
{
    public partial class Agendamento : ObservableObject
    {
        public int Id { get; set; }

        public int ClienteId { get; set; }
        public Cliente ?Cliente { get; set; }
        public int? CriancaId { get; set; }
        public Crianca? Crianca { get; set; }

        [ObservableProperty] private string? pacote;

        [ObservableProperty] private TimeSpan? horario;

        [ObservableProperty] private string? tema;

        [ObservableProperty] private decimal valor;

        [ObservableProperty] private decimal valorPago;

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
