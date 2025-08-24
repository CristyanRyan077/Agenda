using AgendaNovo.Helpers;
using AgendaNovo.Interfaces;
using AgendaNovo.Models;
using AgendaNovo.Services;
using AgendaNovo.ViewModels;
using ClosedXML.Excel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Spreadsheet;
using HandyControl.Controls;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AgendaNovo.ViewModels
{
    public static class ClienteExtensions
    {
        public static void Reset(this Cliente cliente)
        {
            if (cliente is null) return;

            cliente.Id = 0;
            cliente.Nome = string.Empty;
            cliente.Telefone = string.Empty;
            cliente.Email = string.Empty;
        }

        public static void Reset(this Crianca crianca)
        {
            if (crianca is null) return;

            crianca.Id = 0;
            crianca.ClienteId = 0;
            crianca.Nome = string.Empty;
        }
    }
    public partial class ClienteCriancaViewModel : ObservableObject
    {

        [ObservableProperty] private ClienteCriancaView? clienteCriancaSelecionado;
        [ObservableProperty] private bool clienteExistenteDetectado;
        [ObservableProperty]
        private bool completouAcompanhamento;
        [ObservableProperty] private bool isInEditMode;
        [ObservableProperty] private string pesquisaText;
        [ObservableProperty] private ObservableCollection<Crianca> listaCriancas = new();
        [ObservableProperty] private ObservableCollection<Cliente> listaClientes = new();
        [ObservableProperty]
        private ObservableCollection<AgendamentoHistoricoVM> historicoAgendamentos = new();
        [ObservableProperty] private ObservableCollection<Crianca> listaCriancasDoCliente = new();
        [ObservableProperty] private Cliente novoCliente = new();
        [ObservableProperty] private Crianca? criancaSelecionada = new();
        private List<ClienteCriancaView> _todosClientes = new();
        [ObservableProperty] private string filtroSelecionado;
        private List<ClienteCriancaView> _clientesFiltrados = new();

        public ObservableCollection<MesItem> Meses { get; } = new(
        System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat
        .MonthNames
        .Where(nome => !string.IsNullOrEmpty(nome))
        .Select((nome, index) => new MesItem { Numero = index + 1, Nome = nome })
        .ToList()
);
        public ObservableCollection<int> Anos { get; } = new(
        Enumerable.Range(DateTime.Now.Year - 5, 6).Reverse().ToList()
        );
        [ObservableProperty] private MesItem mesSelecionado;
        [ObservableProperty] private int anoSelecionado = DateTime.Now.Year;


        public IEnumerable<IdadeUnidade> IdadesUnidadeDisponiveis => Enum.GetValues(typeof(IdadeUnidade)).Cast<IdadeUnidade>();
        public IEnumerable<Genero> GenerosLista => Enum.GetValues(typeof(Genero)).Cast<Genero>();
        public IEnumerable<StatusCliente> StatusLista => Enum.GetValues(typeof(StatusCliente)).Cast<StatusCliente>();


        private readonly AgendaViewModel _agenda;
        private readonly IClienteService _clienteService;
        private readonly ICriancaService _criancaService;
        private readonly IAgendamentoService _agendamentoService;
        private readonly IServicoService _servicoService;
        private readonly IPacoteService _pacoteService;


        public ClienteCriancaViewModel(IAgendamentoService agendamentoService,
        IClienteService clienteService,
        ICriancaService criancaService,
        IPacoteService pacoteService,
        IServicoService servicoService)
        {

            _agendamentoService = agendamentoService;
            _clienteService = clienteService;
            _criancaService = criancaService;
            _pacoteService = pacoteService;
            _servicoService = servicoService;
            _clienteService.ClienteInativo();
            CarregarClientesDoBanco();
            LimparCamposClienteCrianca();
            _criancaService.AtualizarIdadeDeTodasCriancas();


        }
        partial void OnMesSelecionadoChanged(MesItem value)
        {
            AplicarFiltrosComPesquisa();
        }

        partial void OnAnoSelecionadoChanged(int value)
        {
            if (MesSelecionado != null)
                AplicarFiltrosComPesquisa();
        }
        private void NotifyAll()
        {
            OnPropertyChanged(nameof(NovoCliente));
            OnPropertyChanged(nameof(CriancaSelecionada));
            OnPropertyChanged(nameof(ClienteCriancaSelecionado));
            OnPropertyChanged(nameof(ListaCriancasDoCliente));
            OnPropertyChanged(nameof(ListaCriancas));
            OnPesquisaTextChanged(PesquisaText);
        }
        [RelayCommand]
        private void FiltrarPorMes()
        {
            var mes = MesSelecionado?.Numero;
            var ano = AnoSelecionado;

            _clientesFiltrados = ObterClientesFiltrados(mes, ano).ToList();
            AtualizarPaginacao(_clientesFiltrados);
            AplicarFiltrosComPesquisa();
        }
        private void CarregarClientesDoBanco()
        {
            var mesAtual = DateTime.Now.Month;
            var anoAtual = DateTime.Now.Year;
            var todos = _clienteService?.GetAllWithChildren()
           ?? new List<Cliente>();
            _todosClientes = todos.SelectMany(cliente =>
            {
                var agendamentos = _clienteService.GetAgendamentos(cliente.Id)?.ToList() ?? new List<Agendamento>();
                var filhos = cliente.Criancas ?? new List<Crianca>();
                decimal totalHistorico = agendamentos.Sum(a => a.ValorPago);
                decimal totalMesAtual = agendamentos
                    .Where(a => a.Data.Month == mesAtual && a.Data.Year == anoAtual)
                    .Sum(a => a.ValorPago);
                if (filhos.Any())
                {
                    return filhos.Select(crianca => new ClienteCriancaView
                    {
                        ClienteId = cliente.Id,
                        NomeCliente = cliente.Nome,
                        Telefone = cliente.Telefone,
                        Email = cliente.Email,

                        CriancaId = crianca.Id,
                        NomeCrianca = crianca.Nome,
                        Nascimento = crianca.Nascimento,
                        Idade = crianca.Idade,
                        IdadeUnidade = crianca.IdadeUnidade,
                        Genero = crianca.Genero,
                        Status = cliente.Status,
                        Facebook = cliente.Facebook,
                        Instagram = cliente.Instagram,
                        Observacao = cliente.Observacao,
                        Agendamentos = agendamentos,
                    });
                }
                else
                {
                    return new[] {
                    new ClienteCriancaView {
                        ClienteId = cliente.Id,
                        NomeCliente = cliente.Nome,
                        Telefone = cliente.Telefone,
                        Email = cliente.Email,
                        Status = cliente.Status,
                        Facebook = cliente.Facebook,
                        Instagram = cliente.Instagram,
                        Observacao = cliente.Observacao,
                        Agendamentos = agendamentos,


                    }
                };
                }
            }).ToList();
            ListaClientes.Clear();
            foreach (var cli in todos)
                ListaClientes.Add(cli);

            _clientesFiltrados = _todosClientes;
            AtualizarPaginacao(_clientesFiltrados);
        }
        public void VerificarClientesInativos()
        {
            _clienteService.ClienteInativo();
        }

        public void DetectarClientePorCampos()
        {
            if (IsInEditMode)
                return;

            var tel = NovoCliente.Telefone?.Trim();
            var email = NovoCliente.Email?.Trim();
            if (string.IsNullOrEmpty(tel) && string.IsNullOrEmpty(email))
            {
                ClienteExistenteDetectado = false;
                NovoCliente.Id = 0;
                ListaCriancasDoCliente.Clear();
                return;
            }


            var encontrado = _clienteService.DetectExisting(tel, email);
            if (encontrado != null)
            {
                // preenche campos
                NovoCliente.Id = encontrado.Id;
                NovoCliente.Nome = encontrado.Nome;
                NovoCliente.Telefone = encontrado.Telefone;
                NovoCliente.Email = encontrado.Email;
                NovoCliente.Observacao = encontrado.Observacao;
                NovoCliente.Facebook = encontrado.Facebook;
                NovoCliente.Instagram = encontrado.Instagram;


                ListaCriancasDoCliente.Clear();
                foreach (var c in _criancaService.GetByClienteId(encontrado.Id))
                    ListaCriancasDoCliente.Add(c);

                ClienteExistenteDetectado = true;
            }
            else
            {
                ClienteExistenteDetectado = false;
            }
        }
        private void AtualizarDadosClienteSelecionado()
        {
            if (ClienteCriancaSelecionado == null) return;

            var clienteAtualizado = _clienteService.GetById(ClienteCriancaSelecionado.ClienteId);
            if (clienteAtualizado == null) return;

            var agendamentosAtualizados = _clienteService.GetAgendamentos(clienteAtualizado.Id)?.ToList() ?? new List<Agendamento>();

            ClienteCriancaSelecionado.Agendamentos = agendamentosAtualizados;

            // Força a notificação visual
            OnPropertyChanged(nameof(ClienteCriancaSelecionado));
            OnPropertyChanged(nameof(PaginaClientes));
            AtualizarPaginacao(_clientesFiltrados);
        }


        public bool TemHistorico => HistoricoAgendamentos?.Any() == true;
        partial void OnClienteCriancaSelecionadoChanged(ClienteCriancaView? value)
        {
            if (value == null)
            {
                HistoricoAgendamentos.Clear();
                return;
            }

            var agendamentosdocliente = _clienteService.GetAgendamentos(value.ClienteId) ?? new List<Agendamento>();

            var acompanhamentos = agendamentosdocliente
                .Where(a => a.Servico.Nome == "Acompanhamento Mensal")
                .OrderBy(a => a.Data)
                .Select((a, index) => new { a.Id, NumeroMes = index + 1 })
                .ToList();

            var historico = agendamentosdocliente
                .OrderByDescending(a => a.Data)
                .Select(a => new AgendamentoHistoricoVM
                {
                    Agendamento = a,
                    NumeroMes = acompanhamentos.FirstOrDefault(x => x.Id == a.Id)?.NumeroMes
                })
                .ToList();


            HistoricoAgendamentos = new ObservableCollection<AgendamentoHistoricoVM>(historico);
            OnPropertyChanged(nameof(TemHistorico));// Ordena do mais recente para o mais antigo

            // Se quiser calcular acompanhamento mensal completo:
            var mensalcompleto = agendamentosdocliente
                .Where(a => a.Status == StatusAgendamento.Concluido
                            && a.Data.Year == DateTime.Now.Year
                            && a.ServicoId == 2)
                .Select(a => a.Data.Month)
                .Distinct();

            CompletouAcompanhamento = mensalcompleto.Count() == 12;
        }
        [RelayCommand]
        private void ExportarTodosClientesParaExcel()
        {
            int? mes = MesSelecionado?.Numero;
            int? ano = MesSelecionado != null ? AnoSelecionado : (int?)null;
            var clientes = (mes.HasValue ? ObterClientesFiltrados(mes, ano)
                : ObterClientesFiltrados()).ToList();
            var sufixoFiltro = string.IsNullOrWhiteSpace(FiltroSelecionado) ? "Filtrados" : FiltroSelecionado;
            var sufixoMes = mes.HasValue ? $"_{MesSelecionado?.Nome}_{ano}" : "";

            var salvar = new SaveFileDialog
            {
                FileName = $"Clientes_{sufixoFiltro}{sufixoMes}.xlsx",
                Filter = "Arquivo Excel (*.xlsx)|*.xlsx"
            };
            if (salvar.ShowDialog() != true) return;

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Clientes");

            ws.Cell("A1").Value = "Nome";
            ws.Cell("B1").Value = "Telefone";
            ws.Cell("C1").Value = "Email";
            ws.Cell("D1").Value = "Crianças";
            ws.Cell("E1").Value = "Pago no mês";
            ws.Cell("F1").Value = "Pago no total";
            ws.Cell("G1").Value = "Qtd. Agendamentos";
            ws.Cell("H1").Value = "Status";
            ws.Cell("I1").Value = "Observação";
            int linha = 2;

            foreach (var cliente in clientes)
            {
                var agendamentos = cliente.Agendamentos ?? new List<Agendamento>();
                var agora = DateTime.Now;
                int mesRef = mes ?? agora.Month;
                int anoRef = ano ?? agora.Year;
                var totalMes = agendamentos
                    .Where(a => a.Data.Month == mesRef && a.Data.Year == anoRef)
                    .Sum(a => a.ValorPago);

                var totalHistorico = agendamentos
                    .Sum(a => a.ValorPago);

                // Crianças em string separada por vírgula
                var nomesCriancas = string.Join(", ", cliente.NomeCrianca);

                ws.Cell(linha, 1).Value = cliente.NomeCliente;
                ws.Cell(linha, 2).Value = cliente.Telefone;
                ws.Cell(linha, 3).Value = cliente.Email;
                ws.Cell(linha, 4).Value = nomesCriancas;
                ws.Cell(linha, 5).Value = (double)totalMes;
                ws.Cell(linha, 6).Value = (double)totalHistorico;

                ws.Cell(linha, 5).Style.NumberFormat.Format = "R$ #,##0.00";
                ws.Cell(linha, 6).Style.NumberFormat.Format = "R$ #,##0.00";

                ws.Cell(linha, 7).Value = agendamentos.Count;
                ws.Cell(linha, 8).Value = cliente.Status.ToString();
                ws.Cell(linha, 9).Value = cliente.Observacao;


                linha++;
            }


            ws.Columns().AdjustToContents();

            workbook.SaveAs(salvar.FileName);
        }






        [RelayCommand]
        private void EditarClienteCriancaSelecionado()
        {

            if (ClienteCriancaSelecionado is null)
                return;
            IsInEditMode = true;

            var cliente = ListaClientes.FirstOrDefault(c => c.Id == ClienteCriancaSelecionado.ClienteId);
            if (cliente is null)
                return;

            NovoCliente.Id = cliente.Id;
            NovoCliente.Nome = cliente.Nome;
            NovoCliente.Telefone = cliente.Telefone;
            NovoCliente.Email = cliente.Email;
            NovoCliente.Observacao = cliente.Observacao;
            NovoCliente.Facebook = cliente.Facebook;
            NovoCliente.Instagram = cliente.Instagram;


            CarregarCriancasDoCliente(cliente);

            if (ClienteCriancaSelecionado.CriancaId is int criancaId)
            {
                var crianca = cliente.Criancas.FirstOrDefault(c => c.Id == criancaId);
                if (crianca is not null)
                {

                    CriancaSelecionada.Id = crianca.Id;
                    CriancaSelecionada.Nome = crianca.Nome;
                    CriancaSelecionada.Genero = crianca.Genero;
                    CriancaSelecionada.Nascimento = crianca.Nascimento;
                    CriancaSelecionada.Idade = crianca.Idade;
                    CriancaSelecionada.IdadeUnidade = crianca.IdadeUnidade;
                }
            }
            else
            {
                CriancaSelecionada = new Crianca();
            }
            NotifyAll();
        }
        private void CarregarCriancasDoCliente(Cliente cliente)
        {
            ListaCriancasDoCliente.Clear();
            foreach (var c in cliente.Criancas)
                ListaCriancasDoCliente.Add(c);
        }

        private void LimparCamposClienteCrianca()
        {
            if (NovoCliente is not null)
            {
                NovoCliente.Reset();
            }

            CriancaSelecionada = new Crianca();

            if (CriancaSelecionada is not null)
            {
                CriancaSelecionada.Reset();
            }
            ListaCriancas.Clear();
            ListaCriancasDoCliente.Clear();
            NotifyAll();
            _clientesFiltrados = _todosClientes;
            AtualizarPaginacao(_clientesFiltrados);
        }
        [RelayCommand]
        private void SalvarClienteCrianca()
        {
            if (string.IsNullOrWhiteSpace(NovoCliente.Nome))
                return;


            Cliente cliente;

            if (NovoCliente.Id != 0)
            {
                cliente = _clienteService.GetById(NovoCliente.Id)!;
                cliente.Nome = NovoCliente.Nome;
                cliente.Telefone = NovoCliente.Telefone;
                cliente.Email = NovoCliente.Email;
                cliente.Observacao = NovoCliente.Observacao;
                cliente.Instagram = NovoCliente.Instagram;
                cliente.Facebook = NovoCliente.Facebook;
                _clienteService.Update(cliente);
            }
            else
            {
                cliente = new Cliente
                {
                    Nome = NovoCliente.Nome,
                    Telefone = NovoCliente.Telefone,
                    Email = NovoCliente.Email,
                    Observacao = NovoCliente.Observacao,
                    Facebook = NovoCliente.Facebook,
                    Instagram = NovoCliente.Instagram,
                    Criancas = new List<Crianca>()
                };
                cliente = _clienteService.Add(cliente);

            }

            int? clienteIdNotificacao = cliente.Id;
            int? criancaIdNotificacao = null;

            // Verifica se há criança para salvar
            if (!string.IsNullOrWhiteSpace(CriancaSelecionada.Nome))
            {
                var crianca = CriancaSelecionada.Id != 0
                ? _criancaService.GetById(CriancaSelecionada.Id)!
                : new Crianca { ClienteId = cliente.Id };

                crianca.Nome = CriancaSelecionada.Nome;
                crianca.Genero = CriancaSelecionada.Genero;
                crianca.Nascimento = CriancaSelecionada.Nascimento;
                crianca.Idade = CriancaSelecionada.Idade;
                crianca.IdadeUnidade = CriancaSelecionada.IdadeUnidade;

                _criancaService.AddOrUpdate(crianca);
            }
            if (clienteIdNotificacao.HasValue || criancaIdNotificacao.HasValue)
                WeakReferenceMessenger.Default.Send(new DadosAtualizadosMessage(clienteIdNotificacao, criancaIdNotificacao));

            _clientesFiltrados = _todosClientes;
            CarregarClientesDoBanco();
            LimparInputsClienteCrianca();
            NotifyAll();
            AtualizarDadosClienteSelecionado();


        }
        private void LimparInputsClienteCrianca()
        {
            NovoCliente.Reset();
            CriancaSelecionada.Reset();
            ClienteExistenteDetectado = false;
            IsInEditMode = false;
            NovoCliente.Observacao = string.Empty;
            NovoCliente.Instagram = string.Empty;
            NovoCliente.Facebook = string.Empty;
            NotifyAll();


        }
        [RelayCommand]
        private void ExcluirClienteOuCriancaSelecionado()
        {
            if (clienteCriancaSelecionado == null) return;

            var cliId = ClienteCriancaSelecionado.ClienteId;
            var criId = ClienteCriancaSelecionado.CriancaId;
            // confirme antes de apagar tudo
            if (System.Windows.MessageBox.Show($"Excluir cliente {clienteCriancaSelecionado.NomeCliente} e crianças?",
                                "Confirma", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;
            if (criId != null)
            {
                _criancaService.Delete(criId.Value);
            }
            _clienteService.Delete(cliId);

            // 1) Monta o objeto de backup
            /*var backup = new
            {
                Cliente = cliente,
                Criancas = cliente.Criancas.ToList()
            };

            // 2) Serializa em JSON com identação e preservando referências
            var json = JsonSerializer.Serialize(
                backup,
                new JsonSerializerOptions
                {
                    WriteIndented = true,
                    ReferenceHandler = ReferenceHandler.Preserve
                }
            );

            // 3) Garante que a pasta exista
            string pastaBackup = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Backups"
            );
            Directory.CreateDirectory(pastaBackup);

            // 4) Nomeia o arquivo incluindo o Id do cliente (e a data)
            string fname = $"backup_cliente_{cliente.Id}_{DateTime.Today:yyyyMMdd}.json";
            string caminhoArquivo = Path.Combine(pastaBackup, fname);

            // 5) Grava no disco
            File.WriteAllText(caminhoArquivo, json);

            // 6) Informa o usuário
            MessageBox.Show(
                $"Backup do cliente salvo em:\n{caminhoArquivo}",
                "Backup",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );

            // 7) Remove do DbContext e da UI
            //    (se quiser remover as crianças individualmente, descomente a linha abaixo)
           // _db.Criancas.RemoveRange(cliente.Criancas);
            */

            IsInEditMode = false;
            ClienteExistenteDetectado = false;
            NotifyAll();
            CarregarClientesDoBanco();
            LimparCamposClienteCrianca();
            return;

        }
        partial void OnFiltroSelecionadoChanged(string value)
        {
            AplicarFiltrosComPesquisa();
        }
        private void AplicarFiltrosComPesquisa()
        {

            int? mes = MesSelecionado?.Numero;
            int? ano = MesSelecionado != null ? AnoSelecionado : (int?)null;

            _clientesFiltrados = ObterClientesFiltrados(mes, ano).ToList();
            AtualizarPaginacao(_clientesFiltrados);
        }
        private bool AgendamentoAtendeFiltro(Agendamento a)
        {
            switch (FiltroSelecionado)
            {
                case "Pendente":
                    return a.Pacote != null && a.ValorPago < a.Pacote.Valor;

                case "Concluido":
                    return a.Pacote != null && a.ValorPago >= a.Pacote.Valor;

                case "S/A":
                case "Inativo":
                    return true;

                // "Todos" (ou nulo)
                default:
                    return true;
            }
        }
        private IEnumerable<ClienteCriancaView> ObterClientesFiltrados(int? mes = null, int? ano = null)
        {
            var agora = DateTime.Now;
            var clientes = _todosClientes.AsEnumerable();

            switch (FiltroSelecionado)
            {
                case "Pendente":
                    clientes = clientes.Where(c =>
                        c.Agendamentos.Any(a => a.Pacote != null && a.ValorPago < a.Pacote.Valor));
                    break;

                case "Concluido":
                    clientes = clientes.Where(c =>
                        c.Status != StatusCliente.Inativo &&
                        c.Agendamentos.Any() &&
                        c.Agendamentos.All(a => a.Pacote != null && a.ValorPago >= a.Pacote.Valor));
                    break;

                case "Inativo":
                    clientes = clientes.Where(c =>
                        c.Status == StatusCliente.Inativo ||
                        (c.Agendamentos.Any() && c.Agendamentos.Max(a => a.Data) < agora.AddDays(-60)));
                    break;

                case "S/A":
                    clientes = clientes.Where(c => c.Agendamentos.Count == 0);
                    break;

                    // "Todos" (ou nulo) => sem filtro adicional
            }

            if (!string.IsNullOrWhiteSpace(PesquisaText))
            {
                var filtro = PesquisaText.Trim().ToLower();
                clientes = clientes.Where(c =>
                    (!string.IsNullOrEmpty(c.NomeCliente) && c.NomeCliente.ToLower().Contains(filtro)) ||
                    (!string.IsNullOrEmpty(c.Telefone) && c.Telefone.ToLower().Contains(filtro)) ||
                    (!string.IsNullOrEmpty(c.NomeCrianca) && c.NomeCrianca.ToLower().Contains(filtro)) ||
                    (c.Agendamentos.Any(a =>
                    !string.IsNullOrEmpty(a.Servico.Nome) &&
                    a.Servico.Nome.ToLower().Contains(filtro)))
                );
            }
            if (mes.HasValue && ano.HasValue)
            {
                clientes = clientes.Where(c =>
                {
                   
                    if (FiltroSelecionado == "S/A")
                        return c.Agendamentos.Count == 0;

                   
                    return c.Agendamentos.Any(a =>
                        a.Data.Month == mes.Value &&
                        a.Data.Year == ano.Value &&
                        AgendamentoAtendeFiltro(a));
                });
            }

            return clientes;
        }

        partial void OnPesquisaTextChanged(string value)
        {

            AplicarFiltrosComPesquisa();
        }
        [ObservableProperty]
        private int paginaAtual = 1;
        [ObservableProperty]
        private int tamanhoPagina = 10;
        public List<int> OpcoesTamanhoPagina { get; } = new() { 10, 20, 50 };
        partial void OnTamanhoPaginaChanged(int value)
        {

            AplicarFiltrosComPesquisa();
        }


        [ObservableProperty]
        private int totalPaginas;

        public ObservableCollection<ClienteCriancaView> PaginaClientes { get; set; } = new();
        public void AtualizarPaginacao(List<ClienteCriancaView>? listaFiltrada = null)
        {
            var origem = listaFiltrada ?? _todosClientes;

            TotalPaginas = (int)Math.Ceiling(origem.Count / (double)TamanhoPagina);
            if (PaginaAtual > TotalPaginas)
                PaginaAtual = TotalPaginas == 0 ? 1 : TotalPaginas;
            if (PaginaAtual < 1)
                PaginaAtual = 1;
            var pagina = origem
                .Skip((PaginaAtual - 1) * TamanhoPagina)
                .Take(TamanhoPagina)
                .ToList();

            PaginaClientes.Clear();
            foreach (var c in pagina)
                PaginaClientes.Add(c);
        }
        [RelayCommand]
        private void ProximaPagina()
        {
            if (PaginaAtual < TotalPaginas)
            {
                PaginaAtual++;
                AtualizarPaginacao(_clientesFiltrados);
            }
        }

        [RelayCommand(CanExecute = nameof(CanIrPaginaAnterior))]
        private void PaginaAnterior()
        {
            if (PaginaAtual > 1)
            {
                PaginaAtual--;
            }
            else
            {
                // Se estiver na página 1, vai para a última página
                PaginaAtual = TotalPaginas == 0 ? 1 : TotalPaginas;
            }

            AtualizarPaginacao(_clientesFiltrados);
        }
        private bool CanIrPaginaAnterior() => TotalPaginas > 1;

    }
}
