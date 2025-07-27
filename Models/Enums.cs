using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgendaNovo.Models
{
    public enum IdadeUnidade
    {
        Ano = 0,   
        Anos = 1,    
        Mês = 2,
        Meses = 3
    }

    public enum Genero
    {
        M = 0,
        F = 1,
    }
    public enum StatusCliente
    {
        Pendente = 0,
        Ativo = 1,
        Inativo = 2
    }
}
