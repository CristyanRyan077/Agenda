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
        Inativo = 2,
        SA = 3
    }

    public enum StatusAgendamento
    {
        Pendente = 0,
        Concluido = 1,
        Cancelado = 2,
        Confirmado = 3
    }
    public enum FotosReveladas
    {
        Pendente = 0,
        Revelado = 1,
        Entregue = 2
    }
    public enum EtapaStatus { Pendente = 0, Hoje = 1, Atrasado = 2, Concluido = 3 }
    public enum EtapaFotos
    {
        Escolha = 0,            
        Tratamento = 1,         
        Revelar = 2,  
        Entrega = 3           
    }
    public enum FotoAtrasoTipo { Tratamento, Revelar, Entrega }
    public enum TipoBusca
    {
        Cliente,
        Agendamento
    }
    public enum TipoEntrega
    {
        Foto = 0,
        Album = 1,
        Painel = 2,
        Banner = 3
    }
    public enum MetodoPagamento
    {

        Pix = 0,
        Débito = 1,
        Crédito = 2,
        Dinheiro = 3,
        
    }
    public enum ExportTipo 
    {
        Ambos = 0,
        Resumo = 1,
        EmAberto = 2, 
    }
    public enum TipoLancamento 
    { 
        Pagamento = 0,
        Produto = 1
    }
}
