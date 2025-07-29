using AgendaNovo.Interfaces;
using AgendaNovo.Models;
using AgendaNovo.Services;
using AgendaNovo.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using Microsoft.EntityFrameworkCore;
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
        private ObservableCollection<Agendamento> historicoAgendamentos = new();
        [ObservableProperty] private ObservableCollection<Crianca> listaCriancasDoCliente = new();
        [ObservableProperty] private Cliente novoCliente = new();
        [ObservableProperty] private Crianca? criancaSelecionada = new();
        private List<ClienteCriancaView> _todosClientes = new();
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
            CarregarClientesDoBanco();
            LimparCamposClienteCrianca();
            _criancaService.AtualizarIdadeDeTodasCriancas();

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
        private void CarregarClientesDoBanco()
        {
            var todos = _clienteService?.GetAllWithChildren()
           ?? new List<Cliente>();
            _todosClientes = todos.SelectMany(cliente =>
            {
                var filhos = cliente.Criancas ?? new List<Crianca>();
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
                        Observacao = cliente.Observacao
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
                        Observacao = cliente.Observacao


                    }
                };
                }
            }).ToList();
            ListaClientes.Clear();
            foreach (var cli in todos)
                ListaClientes.Add(cli);

            AtualizarPaginacao();
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


        public bool TemHistorico => HistoricoAgendamentos?.Any() == true;
        partial void OnClienteCriancaSelecionadoChanged(ClienteCriancaView? value)
        {
            if (value == null)
            {
                HistoricoAgendamentos.Clear();
                return;
            }

            var agendamentosdocliente = _clienteService.GetAgendamentos(value.ClienteId) ?? new List<Agendamento>();

            // Atualiza o histórico
            HistoricoAgendamentos = new ObservableCollection<Agendamento>(agendamentosdocliente
                .OrderByDescending(a => a.Data));
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
            AtualizarPaginacao();
            CarregarClientesDoBanco();
            LimparInputsClienteCrianca();
            NotifyAll();


        }
        private void LimparInputsClienteCrianca()
        {
            NovoCliente.Reset();
            CriancaSelecionada.Reset();
            ClienteExistenteDetectado = false;
            IsInEditMode = false;
            NotifyAll();
        }
        [RelayCommand]
        private void ExcluirClienteOuCriancaSelecionado()
        {
            if (clienteCriancaSelecionado == null) return;

            var cliId = ClienteCriancaSelecionado.ClienteId;
            var criId = ClienteCriancaSelecionado.CriancaId;


            if (criId != null)
            {
                _criancaService.Delete(criId.Value);
            }
            else
            {
                // confirme antes de apagar tudo
                if (System.Windows.MessageBox.Show($"Excluir cliente {clienteCriancaSelecionado.NomeCliente} e crianças?",
                                    "Confirma", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                    return;

                _clienteService.Delete(cliId);
            }

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
        partial void OnPesquisaTextChanged(string value)
        {
            PaginaAtual = 1;

            if (string.IsNullOrWhiteSpace(value))
            {
                // Sem filtro, paginação com todos os clientes
                AtualizarPaginacao();
                return;
            }

            var filtro = value.Trim().ToLower();

            // Aplica filtro na lista total
            var filtrados = _todosClientes.Where(c =>
                (!string.IsNullOrEmpty(c.NomeCliente) && c.NomeCliente.ToLower().Contains(filtro)) ||
                (!string.IsNullOrEmpty(c.Telefone) && c.Telefone.ToLower().Contains(filtro)) ||
                (!string.IsNullOrEmpty(c.NomeCrianca) && c.NomeCrianca.ToLower().Contains(filtro))
            ).ToList();

            AtualizarPaginacao(filtrados);
        }
        [ObservableProperty]
        private int paginaAtual = 1;
        [ObservableProperty]
        private int tamanhoPagina = 10;
        public List<int> OpcoesTamanhoPagina { get; } = new() { 10, 20, 50 };
        partial void OnTamanhoPaginaChanged(int value)
        {
            PaginaAtual = 1;
            AtualizarPaginacao();
        }


        [ObservableProperty]
        private int totalPaginas;

        public ObservableCollection<ClienteCriancaView> PaginaClientes { get; set; } = new();
        public void AtualizarPaginacao(List<ClienteCriancaView>? listaFiltrada = null)
        {
            var origem = listaFiltrada ?? _todosClientes;

            TotalPaginas = (int)Math.Ceiling(origem.Count / (double)TamanhoPagina);

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
                AtualizarPaginacao();
            }
        }

        [RelayCommand]
        private void PaginaAnterior()
        {
            if (PaginaAtual > 1)
            {
                PaginaAtual--;
                AtualizarPaginacao();
            }
        }

    }

}
