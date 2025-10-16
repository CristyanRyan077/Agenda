using AgendaNovo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgendaNovo.Helpers
{
    public static class EtapaHelper
    {
        private static bool Concluida(DateTime? dt) => dt.HasValue && dt.Value > DateTime.MinValue;
        private static bool Concluida(DateTime dt) => dt > DateTime.MinValue;
        public static EtapaFotos? GetProximaEtapa(Agendamento a)
        {
            if (a?.Etapas == null) return EtapaFotos.Tratamento; // padrão de início

            DateTime? dtEscolha = a.Etapas.FirstOrDefault(e => e.Etapa == EtapaFotos.Escolha)?.DataConclusao;
            DateTime? dtTrat = a.Etapas.FirstOrDefault(e => e.Etapa == EtapaFotos.Tratamento)?.DataConclusao;
            DateTime? dtRevelar = a.Etapas.FirstOrDefault(e => e.Etapa == EtapaFotos.Revelar)?.DataConclusao;
            DateTime? dtEntrega = a.Etapas.FirstOrDefault(e => e.Etapa == EtapaFotos.Entrega)?.DataConclusao;

            if (Concluida(dtEntrega)) return null;                // acabou
            if (Concluida(dtRevelar)) return EtapaFotos.Entrega;  // falta Entrega
            if (Concluida(dtTrat)) return EtapaFotos.Revelar;  // falta Revelar
            if (Concluida(dtEscolha)) return EtapaFotos.Tratamento;
            // se nem Escolha está concluída (ou nem existe), começamos por Tratamento
            return EtapaFotos.Tratamento;
        }

    }
}
