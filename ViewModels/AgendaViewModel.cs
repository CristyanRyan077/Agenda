﻿
using AgendaNovo.Interfaces;
using AgendaNovo.Models;
using AgendaNovo.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ControlzEx.Standard;
using Microsoft.EntityFrameworkCore;
using System;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace AgendaNovo
{
    public partial class AgendaViewModel : ObservableObject
    {
        //Agendamento
        private bool _suspendendoDataChanged = false;
        private bool _selecionandoDaGrid = false;
        [ObservableProperty] private Agendamento novoAgendamento = new();
        [ObservableProperty] private ObservableCollection<Agendamento> listaAgendamentos = new();
        [ObservableProperty] private ObservableCollection<Agendamento> agendamentosFiltrados = new();
        [ObservableProperty] private decimal valorPacote;
        [ObservableProperty] private Agendamento? itemSelecionado;
        [ObservableProperty] private Pacote? pacoteselecionado;


        //Cliente
        [ObservableProperty] private Cliente? clienteSelecionado;
        [ObservableProperty] private Cliente novoCliente = new();
        [ObservableProperty] private ObservableCollection<Cliente> listaClientes = new();
        [ObservableProperty]
        private bool usuarioDigitouNome;
        public ObservableCollection<Cliente> ClientesFiltrados { get; set; } = new();

        //Crianca
        [ObservableProperty] private ObservableCollection<Crianca> listaCriancas = new();
        [ObservableProperty] private Crianca? criancaSelecionada = new();
        [ObservableProperty]
        private int? criancaId;


        //Data e horario
        [ObservableProperty] private DateTime dataSelecionada = DateTime.Today;
        [ObservableProperty] private ObservableCollection<string> horariosDisponiveis = new();

        //Outros
        [ObservableProperty] private ObservableCollection<Servico> listaServicos = new();
        [ObservableProperty] private Servico? servicoSelecionado;
        [ObservableProperty] private string textoPesquisa = string.Empty;
        [ObservableProperty] private ObservableCollection<Pacote> listaPacotes = new();
        [ObservableProperty] private ObservableCollection<Pacote> listaPacotesFiltrada = new();
        [ObservableProperty]
        private string nomeDigitado = string.Empty;
        [ObservableProperty]
        private bool ignorarProximoTextChanged;
        [ObservableProperty] private bool mostrarCheck;

        [ObservableProperty]
        private bool mostrarSugestoes = false;
        public bool MostrarCrianca => ServicoSelecionado == null || ServicoSelecionado.PossuiCrianca;
        public IEnumerable<IdadeUnidade> IdadesUnidadeDisponiveis => Enum.GetValues(typeof(IdadeUnidade)).Cast<IdadeUnidade>();
        public IEnumerable<Genero> GenerosLista => Enum.GetValues(typeof(Genero)).Cast<Genero>();


        private readonly IAgendamentoService _agendamentoService;
        private readonly IClienteService _clienteService;
        private readonly ICriancaService _criancaService;
        private readonly IPacoteService _pacoteService;
        private readonly IServicoService _servicoService;

        public AgendaViewModel(IAgendamentoService agendamentoService,
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
            AtualizarHorariosDisponiveis();
            DiaAtual = DateTime.Today.DayOfWeek;
            NovoCliente = new Cliente();
            NovoAgendamento = new Agendamento
            {
                Servico = new Servico { PossuiCrianca = true } // Padrão inicial
            };
            mostrarCheck = true;


        }
        partial void OnServicoSelecionadoChanged(Servico? value)
        {
            if (value == null)
            {
                Debug.WriteLine("[OnServicoSelecionadoChanged] Serviço nulo -> limpando pacotes");
                NovoAgendamento.ServicoId = null;
                ListaPacotesFiltrada.Clear();
                OnPropertyChanged(nameof(MostrarCrianca));
                return;
            }

            Debug.WriteLine($"[OnServicoSelecionadoChanged] Serviço selecionado: {value.Nome} (Id={value.Id})");

            // Atualiza o Id do serviço
            NovoAgendamento.ServicoId = value.Id;

            // Filtra pacotes compatíveis
            FiltrarPacotesPorServico(value.Id);

            // Se não precisa de criança, limpa seleção
            if (!value.PossuiCrianca)
            {
                CriancaSelecionada = null;
                ListaCriancas.Clear();
                NovoAgendamento.CriancaId = null;
                Debug.WriteLine("[OnServicoSelecionadoChanged] Serviço sem criança -> limpando bindings de criança");
            }

            OnPropertyChanged(nameof(MostrarCrianca));
        }


        partial void OnNomeDigitadoChanged(string value)
        {
            var termo = value?.ToLower() ?? "";
            var filtrados = ListaClientes
                .Where(c => c.Nome.ToLower().Contains(termo))
                .ToList();

            ClientesFiltrados.Clear();
            foreach (var cliente in filtrados)
                ClientesFiltrados.Add(cliente);

            MostrarSugestoes = ClientesFiltrados.Count > 0 && !string.IsNullOrWhiteSpace(termo);
        }
        partial void OnPacoteselecionadoChanged(Pacote? value)
        {
            if (value == null)
            {
                NovoAgendamento.PacoteId = null;
                NovoAgendamento.Valor = 0;
                return;
            }

            Debug.WriteLine($"[OnPacoteselecionadoChanged] Pacote selecionado: {value.Nome} (Id={value.Id})");
            NovoAgendamento.PacoteId = value.Id;
            NovoAgendamento.Valor = value.Valor;
        }

        public void Inicializar()
        {
                CarregarDadosDoBanco();
                CarregarPacotes();
                CarregarServicos();
                AtualizarHorariosDisponiveis();
        }
        public void CarregarDadosDoBanco()
        {

            var agendamentos = _agendamentoService.GetAll();
            var clientes = _clienteService.GetAllWithChildren();

            Application.Current.Dispatcher.Invoke(() =>
            {
                ListaAgendamentos.Clear();
                foreach (var a in agendamentos)
                    ListaAgendamentos.Add(a);

                ListaClientes.Clear();
                foreach (var c in clientes)
                    ListaClientes.Add(c);

                ListaCriancas.Clear();
                foreach (var c in clientes)
                    foreach (var cr in _criancaService.GetByClienteId(c.Id))
                        ListaCriancas.Add(cr);
            });

            OnPropertyChanged(nameof(ListaAgendamentos));

            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                AtualizarAgendamentos(); // dispara OnPropertyChanged em todos os dias
            }), DispatcherPriority.Background);
            AtualizarHorariosDisponiveis();

        }

        public void CarregarPacotes()
        {
            Debug.WriteLine("[CarregarPacotes] Iniciando carregamento de pacotes.");
            var pacotes = _pacoteService.GetAll();
            Debug.WriteLine($"[CarregarPacotes] Pacotes carregados do banco: {pacotes.Count()}");

            Application.Current.Dispatcher.Invoke(() =>
            {
                ListaPacotes.Clear();
                foreach (var p in pacotes)
                {
                    Debug.WriteLine($"[CarregarPacotes] Adicionando pacote: {p.Nome} (ServicoId={p.ServicoId})");
                    ListaPacotes.Add(p);
                }

                if (NovoAgendamento.ServicoId.HasValue)
                {
                    Debug.WriteLine($"[CarregarPacotes] Chamando filtro para ServicoId={NovoAgendamento.ServicoId}");
                    FiltrarPacotesPorServico(NovoAgendamento.ServicoId.Value);
                }
                else
                {
                    Debug.WriteLine("[CarregarPacotes] Sem ServicoId definido, limpando ListaPacotesFiltrada.");
                    ListaPacotesFiltrada.Clear();
                }
            });
        }
        private void FiltrarPacotesPorServico(int servicoId)
        {
            Debug.WriteLine($"[FiltrarPacotesPorServico] Chamado para ServicoId={servicoId}");
            ListaPacotesFiltrada.Clear();

            var pacotesFiltrados = ListaPacotes.Where(p => p.ServicoId == servicoId).ToList();
            Debug.WriteLine($"[FiltrarPacotesPorServico] Pacotes encontrados: {pacotesFiltrados.Count}");

            foreach (var pacote in pacotesFiltrados)
            {
                Debug.WriteLine($"[FiltrarPacotesPorServico] Adicionando pacote: {pacote.Nome} (Id={pacote.Id})");
                ListaPacotesFiltrada.Add(pacote);
            }
        }
        public void CarregarServicos()
        {
            ListaServicos.Clear();
            var servicos = _servicoService.GetAll();
            foreach (var s in servicos)
                ListaServicos.Add(s);
        }
        private void ResetarFormulario()
        {
            NovoAgendamento = new Agendamento();
            NovoCliente = new Cliente();
            ClienteSelecionado = null;
            CriancaSelecionada = new Crianca();
            ListaCriancas.Clear();
            ValorPacote = 0;
            OnPropertyChanged(nameof(NovoAgendamento));
            OnPropertyChanged(nameof(NovoCliente));
            OnPropertyChanged(nameof(ClienteSelecionado));
            OnPropertyChanged(nameof(ListaCriancas));
        }

        private readonly List<string> _horariosFixos = new()
        {
            "9:00", "10:00", "11:00", "14:00", "15:00", "16:00", "17:00", "18:00", "19:00"
        };




        [RelayCommand]
        private void LimparAnteriores()
        {
            
        }

        private DayOfWeek _diaAtual;
        public DayOfWeek DiaAtual
        {
            get => _diaAtual;
            set
            {
                SetProperty(ref _diaAtual, value);
                OnPropertyChanged(nameof(DiaChkSeg));
                OnPropertyChanged(nameof(DiaChkTer));
                OnPropertyChanged(nameof(DiaChkQua));
                OnPropertyChanged(nameof(DiaChkQui));
                OnPropertyChanged(nameof(DiaChkSex));
                OnPropertyChanged(nameof(DiaChkSab));
                OnPropertyChanged(nameof(DiaChkDom));
            }
        }
        [ObservableProperty]
        private bool preenchendoViaId;
        public void VerificarDuplicidadeNome()
        {
            if (PreenchendoViaId || !UsuarioDigitouNome)
            {
                Debug.WriteLine("preenchendo via id");
                return;
            }
            UsuarioDigitouNome = false;

            var nomeDigitado = NomeDigitado?.Trim();
            if (string.IsNullOrWhiteSpace(nomeDigitado))
                return;


            var clientesIguais = ListaClientes
                .Where(c => string.Equals(c.Nome?.Trim(), nomeDigitado, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (clientesIguais.Count > 1)
            {
                Debug.WriteLine("encontrado duplicado");
                var textoDuplicados = string.Join("\n\n", clientesIguais.Select(c =>
                    $"ID: {c.Id}\nNome: {c.Nome}\n" +
                    $"Crianças:\n{string.Join("\n", c.Criancas.Select(cr => $"- {cr.Nome}"))}\n" +
                    $"Telefone: {c.Telefone}\nEmail: {c.Email}"));

                MessageBox.Show($"⚠ Existem vários clientes com esse nome:\n\n{textoDuplicados}",
                                "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);

                ClienteSelecionado = null;
                NomeDigitado = string.Empty;
                LimparCampos();
            }
            else if (clientesIguais.Count == 1)
            {
                ClienteSelecionado = clientesIguais.First();
            }
        }
        public void FiltrarAgendamentos()
        {
           AgendamentosFiltrados.Clear();

            var filtrados = ListaAgendamentos
                .Where(a => a != null && a.Data.Date == DataSelecionada.Date)
                .ToList();
            foreach (var item in filtrados)
                AgendamentosFiltrados.Add(item);
        } 

        [RelayCommand]
        private void CopiarHorariosLivres()
        {
            // Verifica se a data foi selecionada
            if (DataSelecionada == default)
            {
                MessageBox.Show("Selecione uma data para verificar os horários.", "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Verifica se o telefone foi digitado
            if (string.IsNullOrWhiteSpace(NovoCliente.Telefone))
            {
                MessageBox.Show("Digite um número de telefone.", "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var ocupados = ListaAgendamentos
                .Where(a => a.Data.Date == DataSelecionada.Date)
                .Select(a => a.Horario?.ToString(@"hh\:mm"))
                .Where(h => !string.IsNullOrEmpty(h)) // remove nulos
                .ToList();

            var livres = _horariosFixos
                .Where(h => !ocupados.Contains(h))
                .ToList();

            if (livres.Count == 0)
            {
                MessageBox.Show("Não há horários livres para o dia selecionado.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string texto = $"Olá! Estes são os horários livres para o dia {DataSelecionada:dd/MM/yyyy}:\n\n" +
                           string.Join(", ", livres);

            Clipboard.SetText(texto);
            MessageBox.Show("Horários livres copiados para a área de transferência!", "Copiado", MessageBoxButton.OK, MessageBoxImage.Information);

            // Enviar no WhatsApp
            string telefoneFormatado = $"55859{Regex.Replace(NovoCliente.Telefone, @"\D", "")}";
            string url = $"https://web.whatsapp.com/send?phone={telefoneFormatado}&text={Uri.EscapeDataString(texto)}";

            Thread.Sleep(1000); // Espera opcional
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        partial void OnItemSelecionadoChanged(Agendamento? value)
        {

            var ag = value;
            if (ag == null) return;
            try
            {
                _selecionandoDaGrid = true;


                var cliente = ListaClientes.FirstOrDefault(c => c.Id == ag.ClienteId);
                if (cliente == null)
                {
                    MessageBox.Show("Cliente não encontrado ou inválido.");
                    return;
                }
                ClienteSelecionado = null; // força reset e evita disparo intermediário
                NovoCliente = cliente;
                ClienteSelecionado = cliente;
                NomeDigitado = cliente.Nome;
                OnPropertyChanged(nameof(ClienteSelecionado));
                OnPropertyChanged(nameof(NovoCliente));



                ListaCriancas.Clear();
                foreach (var cr in cliente.Criancas ?? Enumerable.Empty<Crianca>())
                    ListaCriancas.Add(cr);

                var cri = ag.Crianca != null
                  ? cliente.Criancas.FirstOrDefault(c => c.Id == ag.Crianca.Id)
                  : null;
                CriancaSelecionada = cri;
                OnPropertyChanged(nameof(CriancaSelecionada));

                NovoAgendamento = new Agendamento
                {
                    Id = ag.Id,
                    ClienteId = cliente.Id,
                    Cliente = cliente,
                    Crianca = cri ?? new Crianca(),
                    Data = ag.Data,
                    Horario = ag.Horario,
                    Tema = ag.Tema,
                    Valor = ag.Valor,
                    ValorPago = ag.ValorPago,
                    ServicoId = value.ServicoId,
                    PacoteId = value.PacoteId
                };
                ServicoSelecionado = ListaServicos.FirstOrDefault(s => s.Id == value.ServicoId);
                Pacoteselecionado = ListaPacotes.FirstOrDefault(p => p.Id == value.PacoteId);

                Debug.WriteLine("Horario preenchido: " + ag.Horario);
                OnPropertyChanged(nameof(ClienteSelecionado));
                OnPropertyChanged(nameof(NovoCliente));
                OnPropertyChanged(nameof(NovoAgendamento.Pacote));
                OnPropertyChanged(nameof(HorariosDisponiveis));
                OnPropertyChanged(nameof(NovoAgendamento));
                OnPropertyChanged(nameof(NovoAgendamento.Horario));
                OnPropertyChanged(nameof(NovoAgendamento.Tema));
                OnPropertyChanged(nameof(HorarioTexto));
                OnPropertyChanged(nameof(CriancaSelecionada));
                _suspendendoDataChanged = true;
                DataSelecionada = ag.Data;
                _suspendendoDataChanged = false;
                AtualizarHorariosDisponiveis();
                NovoAgendamento.Horario = ag.Horario;
                OnPropertyChanged(nameof(HorariosDisponiveis));

            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao selecionar item: " + ex.Message);
            }
            _selecionandoDaGrid = false;
        }



        public void CriarNovoCliente()
        {
            NovoCliente = new Cliente();
            CriancaSelecionada = null;
            OnPropertyChanged(nameof(NovoCliente));
            OnPropertyChanged(nameof(NovoCliente.Nome));
        }
        [RelayCommand]
        private void LimparCampos()
        {
            NovoAgendamento = new Agendamento();
            ItemSelecionado = null;
            ClienteSelecionado = null;
            ServicoSelecionado = null;
            Pacoteselecionado = null;
            NomeDigitado = string.Empty;
            NovoCliente = new Cliente();
            NovoCliente.Id = 0;
            HorarioTexto = string.Empty;
            

            CriancaSelecionada = new Crianca();
            
            ListaCriancas.Clear();
            ValorPacote = 0;
            OnPropertyChanged(nameof(NovoAgendamento));
            OnPropertyChanged(nameof(NovoCliente));
            OnPropertyChanged(nameof(NovoCliente.Nome));
            OnPropertyChanged(nameof(NovoCliente.Telefone));
            OnPropertyChanged(nameof(NovoCliente.Email));
            OnPropertyChanged(nameof(NovoCliente.Criancas));
            OnPropertyChanged(nameof(CriancaSelecionada));
            OnPropertyChanged(nameof(CriancaSelecionada.Nome));
            OnPropertyChanged(nameof(ClienteSelecionado));
            OnPropertyChanged(nameof(ListaCriancas));
        }
            
        

        partial void OnDataSelecionadaChanged(DateTime value)
        {
            if (_suspendendoDataChanged) return;
            FiltrarAgendamentos();
            AtualizarHorariosDisponiveis();
        }
        public void AtualizarHorariosDisponiveis()
        {
            var horarioStr = NovoAgendamento?.Horario?.ToString(@"hh\:mm");

            var ocupados = ListaAgendamentos
                .Where(a => a.Data.Date == DataSelecionada.Date && a.Id != NovoAgendamento.Id)
                .Select(a => a.Horario?.ToString(@"hh\:mm"))
                .Where(h => !string.IsNullOrEmpty(h))
                .ToList();

            var livres = _horariosFixos
                .Where(h => !ocupados.Contains(h))
                .ToList();

            HorariosDisponiveis.Clear();
            foreach (var h in livres)
                HorariosDisponiveis.Add(h);

            if (!string.IsNullOrEmpty(horarioStr) && !HorariosDisponiveis.Contains(horarioStr))
                NovoAgendamento.Horario = null;
            OnPropertyChanged(nameof(HorarioTexto));
        }

        partial void OnCriancaSelecionadaChanged(Crianca? value)
        {
            if (value == null) return;

            // Notifica todos os bindings dependentes
            OnPropertyChanged(nameof(CriancaSelecionada));
            OnPropertyChanged(nameof(CriancaSelecionada.Genero));
            OnPropertyChanged(nameof(CriancaSelecionada.IdadeUnidade));
        }
        public void RefreshCriancaBindings()
        {
            OnPropertyChanged(nameof(CriancaSelecionada));
            OnPropertyChanged(nameof(CriancaSelecionada.Genero));
            OnPropertyChanged(nameof(CriancaSelecionada.IdadeUnidade));
        }

        partial void OnClienteSelecionadoChanged(Cliente? cliente)
        {
            if (_selecionandoDaGrid || cliente == null) return;


            NomeDigitado = cliente.Nome;

            // Atualiza telefone e email também
            NovoCliente.Id = cliente.Id;
            NovoCliente.Nome = cliente.Nome;
            NovoCliente.Telefone = cliente.Telefone;
            NovoCliente.Email = cliente.Email;
            novoCliente.Observacao = cliente.Observacao;

            ListaCriancas.Clear();
            foreach (var cr in cliente.Criancas ?? Enumerable.Empty<Crianca>())
                ListaCriancas.Add(cr);

            if (cliente.Criancas?.Count > 0)
            {
                CriancaSelecionada = cliente.Criancas[0];

                // *** Garante que os enums são atualizados e notificados ***
                OnPropertyChanged(nameof(CriancaSelecionada));
                OnPropertyChanged(nameof(CriancaSelecionada.Genero));
                OnPropertyChanged(nameof(CriancaSelecionada.IdadeUnidade));
            }

            // Dispara notificação geral:
            OnPropertyChanged(nameof(NovoCliente));
            OnPropertyChanged(nameof(ListaCriancas));
            OnPropertyChanged(nameof(CriancaSelecionada));
        }


        public void PreencherCamposSeClienteExistir(string? nomeDigitado, Action<Cliente> preencher)
        {
            if (string.IsNullOrWhiteSpace(nomeDigitado))
                return;

            var cliente = _clienteService.GetById(NovoCliente.Id);

            if (cliente is not null)
            {

                preencher(cliente);
                foreach (var crianca in cliente.Criancas)
                    ListaCriancas.Add(crianca);
            }
        }


        public void PreencherPacote(int? pacoteId, Action<decimal> preencher)
        {
            if (!pacoteId.HasValue)
                return;

            var pacote = _pacoteService.GetById(pacoteId.Value);
            if (pacote != null)
            {
                preencher(pacote.Valor);
            }
        }
        public void PreencherValorPacoteSelecionado(int? pacoteId)
        {
            if (!pacoteId.HasValue)
                return;

            var pacote = _pacoteService.GetById(pacoteId.Value);
            if (pacote != null)
            {
                // Preenche o valor do pacote no agendamento
                NovoAgendamento.Valor = pacote.Valor;
                OnPropertyChanged(nameof(NovoAgendamento));
            }
        }


        [RelayCommand]
        private void Agendar()
        {
            if (NovoCliente == null || NovoCliente.Id == 0 || string.IsNullOrWhiteSpace(NovoCliente.Nome))
                return;

            if (string.IsNullOrWhiteSpace(NovoAgendamento.Horario?.ToString(@"hh\:mm")))
            {
                MessageBox.Show(
                    "Por favor, selecione um horário antes de agendar.",
                    "Horário obrigatório",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }
            var clienteExistente = _clienteService.GetById(NovoCliente.Id);
            if (clienteExistente == null)
                return;



            Crianca criancaParaAgendar = null;
            if (CriancaSelecionada != null)
            {
                // tenta buscar no serviço
                criancaParaAgendar = _criancaService.GetById(CriancaSelecionada.Id);
            }

            if (criancaParaAgendar == null
               && NovoAgendamento.Crianca != null
               && !string.IsNullOrWhiteSpace(NovoAgendamento.Crianca.Nome))
            {
                criancaParaAgendar = new Crianca
                {
                    Nome = NovoAgendamento.Crianca.Nome,
                    Genero = NovoAgendamento.Crianca.Genero,
                    Nascimento = NovoAgendamento.Crianca.Nascimento,
                    Idade = NovoAgendamento.Crianca.Idade,
                    IdadeUnidade = NovoAgendamento.Crianca.IdadeUnidade,
                    ClienteId = clienteExistente.Id
                };
                _criancaService.AddOrUpdate(criancaParaAgendar);
            }
            if (!MostrarCrianca) // se serviço não possui criança
            {
                CriancaSelecionada = null;
                NovoAgendamento.CriancaId = null;
                NovoAgendamento.Crianca = null;
            }
            // 4) Prepara o objeto a salvar
            NovoAgendamento.ClienteId = clienteExistente.Id;
            NovoAgendamento.CriancaId = criancaParaAgendar?.Id;
            NovoAgendamento.ServicoId = ServicoSelecionado?.Id;
            NovoAgendamento.PacoteId = Pacoteselecionado?.Id;



            bool agendamentoNovo = NovoAgendamento.Id == 0;

            if (agendamentoNovo)
            {
                _agendamentoService.Add(NovoAgendamento);
            }
            else
            {
                _agendamentoService.Update(NovoAgendamento);
            }
            if (NovoAgendamento.Valor == NovoAgendamento.ValorPago)
            {
                _clienteService.AtivarSePendente(clienteExistente.Id);
                _agendamentoService.AtivarSePendente(NovoAgendamento.Id);
            }
            if (NovoAgendamento.Valor > NovoAgendamento.ValorPago)
                _clienteService.ValorIncompleto(clienteExistente.Id);


            var cliente = _clienteService.GetById(NovoAgendamento.ClienteId);
            var crianca = NovoAgendamento.CriancaId.HasValue
                ? _criancaService.GetById(NovoAgendamento.CriancaId.Value)
                : null;

            NovoAgendamento.Cliente = cliente;
            NovoAgendamento.Crianca = crianca;
            var textoCrianca = crianca != null
            ? $" {crianca.Nome} ({crianca.Idade} {crianca.IdadeUnidade})\n"
            : "";
            DataReferencia = NovoAgendamento.Data;
            var servicoNome = _servicoService.GetById(NovoAgendamento.ServicoId ?? 0)?.Nome ?? "Não informado";


            var texto = Uri.EscapeDataString($"✅ Agendado: {NovoAgendamento.Data:dd/MM/yyyy} às {NovoAgendamento.Horario} ({NovoAgendamento.Data.ToString("dddd", new CultureInfo("pt-BR"))}) \n\n" +
                            $"Cliente: {cliente.Nome} - {textoCrianca}" +
                            $"Telefone: {cliente.Telefone}\n" +
                            $"Tema: {NovoAgendamento.Tema}\n" +
                            $"Serviço: {servicoNome}\n" +
                            $"Valor: R$ {NovoAgendamento.Valor:N2} | Pago: R$ {NovoAgendamento.ValorPago:N2}\n" +
                            $"📍 *AVISOS*:\r\n\r\n-  A criança tem direito a *dois* acompanhantes 👶👩🏻‍\U0001f9b0👨🏻‍\U0001f9b0" +
                            $" o terceiro acompanhante paga R$ 20,00\r\n- A sessão fotográfica tem duração de até 1 hora." +
                            $"\r\n- *Tolerância máxima de atraso: 30 minutos*🚨" +
                            $"  (A partir de 30 minutos de atraso não atendemos mais, será necessário agendar outra data)." +
                            $" *PRAZO DE ENVIAR FOTOS TRATADAS DE 48HS DIAS ÚTEIS; APÓS O CLIENTE ESCOLHER NO APLICATIVO ALBOOM*");
                Clipboard.SetText(texto);
                MessageBox.Show("Agendamento copiado para a área de transferência!");
            if (agendamentoNovo)
            {
                var telefone = cliente.Telefone;
                string telefoneFormatado = $"55859{Regex.Replace(telefone, @"\D", "")}";
                string url = $"https://web.whatsapp.com/send?phone={telefoneFormatado}&text={texto}";
                Thread.Sleep(500);
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }

            CarregarDadosDoBanco();
            OnPropertyChanged(nameof(DataReferencia));
            AtualizarAgendamentos();
            FiltrarAgendamentos();
            AtualizarHorariosDisponiveis();
            LimparCampos();
            OnPropertyChanged(nameof(ListaAgendamentos));
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                ItemSelecionado = null;
            }), System.Windows.Threading.DispatcherPriority.Background);

        }
        public void AtualizarPago(Agendamento agendamento)
        {
            if (agendamento == null) return;

            // Anexa ou obtém a entidade rastreada pelo contexto
            var agendamentoDb = _agendamentoService.GetById(agendamento.Id);

            if (agendamentoDb != null)
            {
                _agendamentoService.Delete(agendamentoDb.Id);
                ListaAgendamentos.Remove(agendamentoDb);
                FiltrarAgendamentos();
                AtualizarAgendamentos();
                AtualizarHorariosDisponiveis();
            }
        }

        public void AtualizarAgendamentos()
        {
            PreencherColecao(AgendamentosDomingo, DayOfWeek.Sunday);
            PreencherColecao(AgendamentosSegunda, DayOfWeek.Monday);
            PreencherColecao(AgendamentosTerca, DayOfWeek.Tuesday);
            PreencherColecao(AgendamentosQuarta, DayOfWeek.Wednesday);
            PreencherColecao(AgendamentosQuinta, DayOfWeek.Thursday);
            PreencherColecao(AgendamentosSexta, DayOfWeek.Friday);
            PreencherColecao(AgendamentosSabado, DayOfWeek.Saturday);
            OnPropertyChanged(nameof(SemanaExibida));
        }
        [RelayCommand]
        private void Excluir()
        {
            if (ItemSelecionado == null) return;

            // chama o serviço
            _agendamentoService.Delete(ItemSelecionado.Id);

            // recarrega tudo de uma vez
            CarregarDadosDoBanco();
            FiltrarAgendamentos();
            AtualizarAgendamentos();
            AtualizarHorariosDisponiveis();

            ListaAgendamentos.Remove(ItemSelecionado);
            AgendamentosFiltrados.Remove(ItemSelecionado);

            bool clienteAindaTemAgendamentos = ListaAgendamentos.Any(a =>
            a.Cliente?.Nome == ClienteSelecionado?.Nome);

            if (!clienteAindaTemAgendamentos && ClienteSelecionado != null)
            {
                var cliente = ListaClientes.FirstOrDefault(c => c.Id == ClienteSelecionado.Id);
                if (cliente != null)
                    ListaClientes.Remove(cliente);
            }

            ResetarFormulario();
            AtualizarHorariosDisponiveis();
        }

        partial void OnTextoPesquisaChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                AgendamentosFiltrados = new ObservableCollection<Agendamento>(ListaAgendamentos);
            }
            else
            {
                var filtrado = ListaAgendamentos
                    .Where(a => a.Cliente.Nome.Contains(value, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                AgendamentosFiltrados = new ObservableCollection<Agendamento>(filtrado);
            }
        }
        [RelayCommand]
        private void MostrarTodos()
        {
            TextoPesquisa = string.Empty;
            AgendamentosFiltrados = new ObservableCollection<Agendamento>(ListaAgendamentos);
        }
        private IEnumerable<Agendamento> FiltrarPorDia(DayOfWeek dia)
        {
            int diasDesdeSegunda = ((int)DataReferencia.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
            var inicioSemana = DataReferencia.AddDays(-diasDesdeSegunda);
            int indiceDiaSemana = ((int)dia - 1 + 7) % 7;

            var dataDoDia = inicioSemana.AddDays(indiceDiaSemana);

            return ListaAgendamentos
                .Where(a => a.Data.Date == dataDoDia.Date)
                .OrderBy(a => a.Horario);
        }

        private DateTime InicioDaSemana
        {
            get
            {
                int diasDesdeSegunda = ((int)DataReferencia.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
                return DataReferencia.AddDays(-diasDesdeSegunda);
            }
        }

        private DateTime FimDaSemana => InicioDaSemana.AddDays(6);
        [ObservableProperty] private ObservableCollection<Agendamento> agendamentosDomingo = new();
        [ObservableProperty] private ObservableCollection<Agendamento> agendamentosSegunda = new();
        [ObservableProperty] private ObservableCollection<Agendamento> agendamentosTerca = new();
        [ObservableProperty] private ObservableCollection<Agendamento> agendamentosQuarta = new();
        [ObservableProperty] private ObservableCollection<Agendamento> agendamentosQuinta = new();
        [ObservableProperty] private ObservableCollection<Agendamento> agendamentosSexta = new();
        [ObservableProperty] private ObservableCollection<Agendamento> agendamentosSabado = new();
        private void PreencherColecao(ObservableCollection<Agendamento> colecao, DayOfWeek dia)
        {
            colecao.Clear();

            var itens = FiltrarPorDia(dia);
            foreach (var item in itens)
                colecao.Add(item);
            OnPropertyChanged(GetNomePropriedadeDia(dia));
        }
        private string GetNomePropriedadeDia(DayOfWeek dia) => dia switch
        {
            DayOfWeek.Sunday => nameof(AgendamentosDomingo),
            DayOfWeek.Monday => nameof(AgendamentosSegunda),
            DayOfWeek.Tuesday => nameof(AgendamentosTerca),
            DayOfWeek.Wednesday => nameof(AgendamentosQuarta),
            DayOfWeek.Thursday => nameof(AgendamentosQuinta),
            DayOfWeek.Friday => nameof(AgendamentosSexta),
            DayOfWeek.Saturday => nameof(AgendamentosSabado),
            _ => string.Empty
        };
        public bool DiaChkSeg => DiaAtual == DayOfWeek.Monday;
        public bool DiaChkTer => DiaAtual == DayOfWeek.Tuesday;
        public bool DiaChkQua => DiaAtual == DayOfWeek.Wednesday;
        public bool DiaChkQui => DiaAtual == DayOfWeek.Thursday;
        public bool DiaChkSex => DiaAtual == DayOfWeek.Friday;
        public bool DiaChkSab => DiaAtual == DayOfWeek.Saturday;
        public bool DiaChkDom => DiaAtual == DayOfWeek.Sunday;
        private DateTime _dataReferencia = DateTime.Today;
        public DateTime DataReferencia
        {
            get => _dataReferencia;
            set
            {
                if (_dataReferencia != value)
                {
                    _dataReferencia = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(SemanaExibida)); 
                    AtualizarAgendamentos();
                }
            }
        }
        public string SemanaExibida => $"{InicioDaSemana:dd/MM} - {FimDaSemana:dd/MM}";

        [RelayCommand]
        private void SemanaAnterior() => DataReferencia = DataReferencia.AddDays(-7);

        [RelayCommand]
        private void ProximaSemana() => DataReferencia = DataReferencia.AddDays(7);
        public string HorarioTexto
        {
            get => NovoAgendamento?.Horario?.ToString(@"hh\:mm") ?? "";
            set
            {
                if (TimeSpan.TryParse(value, out var parsed) && NovoAgendamento != null)
                {
                    NovoAgendamento.Horario = parsed;
                    OnPropertyChanged(nameof(HorarioTexto));
                }
            }
        }
        [RelayCommand]
        private void ConcluirAgendamento(Agendamento agendamento)
        {
            if (agendamento is null)
                return;

            _agendamentoService.AtivarSePendente(agendamento.Id);
            agendamento.MostrarCheck = false;

            
        }

    }



}
