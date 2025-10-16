using AgendaNovo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgendaNovo.Dtos
{
        public record HistoricoFinanceiroDto(
       int Id,
       DateTime Data,
       string Tipo,          // "Pagamento", "Produto"
       string Descricao,     // "Entrada", "Foto extra", "Álbum impresso"
       decimal Valor,
       MetodoPagamento? Metodo
        );
}
