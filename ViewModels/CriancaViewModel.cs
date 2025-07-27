using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgendaNovo.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AgendaNovo.ViewModels
{
    public class CriancaViewModel : ObservableObject
    {
        private Crianca _crianca;

        public CriancaViewModel(Crianca crianca)
        {
            _crianca = crianca;
        }

        public DateOnly? Nascimento
        {
            get => _crianca.Nascimento;
            set
            {
                if (_crianca.Nascimento != value)
                {
                    _crianca.Nascimento = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(NascimentoDateTime));
                }
            }
        }

        public DateTime? NascimentoDateTime
        {
            get => Nascimento.HasValue ? Nascimento.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null;
            set
            {
                Nascimento = value.HasValue ? DateOnly.FromDateTime(value.Value) : null;
            }
        }

    }
}
