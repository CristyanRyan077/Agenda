using AgendaNovo.Migrations;
using AgendaNovo.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Globalization;
using System;
using System.Collections.Generic;
using System.Linq;
using AgendaNovo.Interfaces;
using AgendaNovo.Services;
using System.ComponentModel;
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
        public ObservableCollection<string> ListaPacotes { get; } = new();
        [ObservableProperty] private Agendamento? itemSelecionado;


        //Cliente
        [ObservableProperty] private Cliente? clienteSelecionado;
        [ObservableProperty] private Cliente novoCliente = new();
        [ObservableProperty] private ObservableCollection<Cliente> listaClientes = new();
        private bool _verificacaoJaFeita;
        public bool VerificacaoJaFeita
        {
            get => _verificacaoJaFeita;
            set => SetProperty(ref _verificacaoJaFeita, value);
        }
        public ICollectionView FilteredClientes { get; }

        //Crianca
        [ObservableProperty] private ObservableCollection<Crianca> listaCriancas = new();
        [ObservableProperty] private Crianca? criancaSelecionada = new();
        [ObservableProperty]
        private int? criancaId;


        //Data e horario
        [ObservableProperty] private DateTime dataSelecionada = DateTime.Today;
        [ObservableProperty] private ObservableCollection<string> horariosDisponiveis = new();

        //Outros
        [ObservableProperty] private string textoPesquisa = string.Empty;
        public IEnumerable<IdadeUnidade> IdadesUnidadeDisponiveis => Enum.GetValues(typeof(IdadeUnidade)).Cast<IdadeUnidade>();
        public IEnumerable<Genero> GenerosLista => Enum.GetValues(typeof(Genero)).Cast<Genero>();

        private readonly IAgendamentoService _agendamentoService;
        private readonly IClienteService _clienteService;
        private readonly ICriancaService _criancaService;

        public AgendaViewModel(IAgendamentoService agendamentoService,
        IClienteService clienteService,
        ICriancaService criancaService)
        {

            _agendamentoService = agendamentoService;
            _clienteService = clienteService;
            _criancaService = criancaService;
            AtualizarHorariosDisponiveis();
            DiaAtual = DateTime.Today.DayOfWeek;
            NovoCliente = new Cliente();
            NovoAgendamento = new Agendamento();
           
        }

        public void Inicializar()
        {
                CarregarDadosDoBanco();
                CarregarPacotes();
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


            AtualizarHorariosDisponiveis();
        }

        public void CarregarPacotes()
        {
            ListaPacotes.Clear();
            foreach (var nome in _pacotesFixos.Keys.OrderBy(p => p))
                ListaPacotes.Add(nome);
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

        public IEnumerable<String> PacotesDisponiveis =>
        _pacotesFixos
        .OrderBy(p => p.Key)
        .Select(p => p.Key);


        private readonly Dictionary<string, decimal> _pacotesFixos = new()
        {      
            {"Smash The Cake - Compartilhado (Pré-Definido)",350m},
            {"Smash The Cake - Pct 01: Basico (Mediante Catálogo)",450m},
            {"Smash The Cake - Pct 02: Premium (Personalidado Individual)",600m},
            {"Acompanhamento Mensal: pct01",80m},
            {"Acompanhamento Mensal: pct02",150m},
            {"Acompanhamento Mensal: - Datas Comemorativas",100m},
            {"Gestante - pct01: Prata",200m},
            {"Gestante - pct02: Ouro",350m},
            {"Gestante - pct03: Diamante",550m},
            {"Infantil pct01",150m},
            {"Infantil pct02",250m},
            {"Aniversario pct01",450m},   
            {"Aniversario pct02",600m},
            {"Aniversario pct03",900m},
            {"Aniversario pct04",1300m},
            {"Evento - Casamento Civil: pct01",350m},
            {"Evento - Casamento Civil: pct02",550m},
            {"Evento - Casamentos: pct01",500m},
            {"Evento - Casamentos: pct02",900m},
            {"Evento - Casamentos: pct03",1400m},
            {"B-Day Adulto - pct01: Prata",200m},
            {"B-Day Adulto - pct02: Ouro",350m},
            {"B-Day Adulto - pct03: Diamante",550},
            {"Casal - pct01 (Fundo Preto/Branco)",150m},
            {"Familia - pct01 (Recamier e Biombo",200m},
            {"Ensaio Infantil(B-Day) - pct01",300m},
            {"Ensaio Infantil(B-Day) - pct02",550m},
            {"B-Day Infantil - pct03",500m},
            {"Chá de Revelação + Vídeo - pct01",350m},
            {"Pack de Fotos - pct01 (Individual)",100m},
            {"Pack de Fotos - pct02 (Indivídual/Produtos)",150m},
            {"Produtos Corporativos - Interno",100m},
            {"Evento Religioso - pct01",550m},
            {"Evento Religioso - pct02",900m},
            {"Evento Religioso - pct03",1400m},
            {"Evento 15 Anos - pct01",550m},
            {"Book Niver Fest - Aniversário + Sessão Infantil",700m}
        };


        [RelayCommand]
        private void LimparAnteriores()
        {
            var resultado = MessageBox.Show(
                "Deseja apagar:\n\nSim - Apenas agendamentos pagos\nNão - Todos os anteriores\nCancelar - Nenhum",
                "Remover Agendamentos Antigos",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Warning);

            // 2) Lista para guardar quais foram "fantasmas"
            var todos = _agendamentoService.GetAll().ToList();

            // 2) Lista para armazenar (Id, lista de campos nulos)
            var fantasmas = new List<(int Id, List<string> CamposNulos)>();

            // 3) Percorre cada agendamento e verifica campos null/empty
            foreach (var ag in todos)
            {
                var nulos = new List<string>();

                // Exemplo: cheque Horario
                if (string.IsNullOrWhiteSpace(ag.Horario))
                    nulos.Add(nameof(ag.Horario));

                // Exemplo: cheque ClienteId
                if (ag.ClienteId == 0)
                    nulos.Add(nameof(ag.ClienteId));

                // Adicione aqui outras propriedades que quer validar…

                if (nulos.Any())
                    fantasmas.Add((ag.Id, nulos));
            }

            // 4) Se não achou nenhum, avisa e sai
            if (!fantasmas.Any())
            {
                MessageBox.Show("Nenhum agendamento com campos nulos encontrado.", "Tudo certo",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // 5) Exibe relatório dos que serão removidos
            var sb = new StringBuilder("Serão removidos estes agendamentos:\n\n");
            foreach (var (id, campos) in fantasmas)
                sb.AppendLine($"• Id={id} → campos nulos: {string.Join(", ", campos)}");
            MessageBox.Show(sb.ToString(), "Confirme remoção",
                MessageBoxButton.OK, MessageBoxImage.Warning);

            // 6) Remove via serviço
            foreach (var (id, _) in fantasmas)
                _agendamentoService.Delete(id);




           /* if (resultado == MessageBoxResult.Yes)
            {
                var json = JsonSerializer.Serialize(anteriores, new JsonSerializerOptions 
                {
                    WriteIndented = true,
                    ReferenceHandler = ReferenceHandler.Preserve
                });

                string pastaBackup = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups");
                Directory.CreateDirectory(pastaBackup);

                string caminhoArquivo = Path.Combine(pastaBackup, $"backup_agendamentos_{DateTime.Today:yyyyMMdd}.json");

                File.WriteAllText(caminhoArquivo, json);

                MessageBox.Show($"Backup salvo com sucesso em:\n{caminhoArquivo}", "Backup", MessageBoxButton.OK, MessageBoxImage.Information);

                var pagosAnteriores = anteriores
                    .Where(a => a.EstaPago)
                    .ToList();

                foreach (var item in pagosAnteriores)
                    ListaAgendamentos.Remove(item);
            }
            else if (resultado == MessageBoxResult.No)
            {
                foreach (var item in anteriores)
                    ListaAgendamentos.Remove(item);
            } */

            AtualizarAgendamentos();
            AtualizarHorariosDisponiveis();
            FiltrarAgendamentos();
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

        public void VerificarClientesComMesmoNome()
        {
            if (string.IsNullOrWhiteSpace(NovoCliente?.Nome))
                return;

            var nome = NovoCliente.Nome.Trim();

            var clientesIguais = ListaClientes
                .Where(c => c.Nome.Equals(nome, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (clientesIguais.Count > 1 ||
               (clientesIguais.Count == 1 && clientesIguais[0].Id != NovoCliente.Id))
            {
                var texto = string.Join("\n\n", clientesIguais.Select(c =>
                    $"ID: {c.Id}\nNome: {c.Nome}\n" +
                    $"Crianças:\n{string.Join("\n", c.Criancas.Select(cr => $"- {cr.Nome} ({cr.Idade} anos)"))}\n" +
                    $"Telefone: {c.Telefone}\nEmail: {c.Email}"));
                MessageBox.Show($"⚠️ Já existe(m) cliente(s) com este nome:\n\n{texto}", "Aviso de duplicidade", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        public void FiltrarAgendamentos()
        {
           /* AgendamentosFiltrados.Clear();

            var filtrados = ListaAgendamentos
                .Where(a => a != null && a.Data.Date == DataSelecionada.Date)
                .ToList();
            foreach (var item in filtrados)
                AgendamentosFiltrados.Add(item);*/
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
                .Select(a => a.Horario)
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
                ClienteSelecionado = cliente;
                NovoCliente = cliente;
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
                    Pacote = ag.Pacote,
                    Tema = ag.Tema,
                    Valor = ag.Valor,
                    ValorPago = ag.ValorPago
                };
                OnPropertyChanged(nameof(ClienteSelecionado));
                OnPropertyChanged(nameof(NovoCliente));
                OnPropertyChanged(nameof(NovoAgendamento.Pacote));
                OnPropertyChanged(nameof(HorariosDisponiveis));
                OnPropertyChanged(nameof(NovoAgendamento));
                OnPropertyChanged(nameof(NovoAgendamento.Horario));
                OnPropertyChanged(nameof(NovoAgendamento.Tema));
                OnPropertyChanged(nameof(CriancaSelecionada));
                _suspendendoDataChanged = true;
                DataSelecionada = ag.Data;
                AtualizarHorariosDisponiveis();
                _suspendendoDataChanged = false;
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
            NovoCliente = new Cliente();
            NovoCliente.Id = 0;
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
            if (NovoAgendamento.Horario != null && !HorariosDisponiveis.Contains(NovoAgendamento.Horario))
                NovoAgendamento.Horario = null;

            var ocupados = ListaAgendamentos
                .Where(a => a.Data.Date == DataSelecionada.Date && a.Id != NovoAgendamento.Id)
                .Select(a => a.Horario)
                .ToList();

            var livres = _horariosFixos
                .Where(h => !ocupados.Contains(h))
                .ToList();

            HorariosDisponiveis.Clear();
            foreach (var h in livres)
                HorariosDisponiveis.Add(h);

            if (!HorariosDisponiveis.Contains(NovoAgendamento.Horario))
                NovoAgendamento.Horario = null;
        }

        partial void OnCriancaSelecionadaChanged(Crianca? value)
        {
            // Garante que qualquer binding que dependa de CriancaSelecionada seja notificado:
            OnPropertyChanged(nameof(CriancaSelecionada));
            // Se precisar expor algo derivado:
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
            if (_selecionandoDaGrid) return;
            if (cliente == null)
            {
                NovoCliente = new Cliente();
                ListaCriancas.Clear();
                CriancaSelecionada = null;
                OnPropertyChanged(nameof(NovoCliente));
                OnPropertyChanged(nameof(ListaCriancas));
                OnPropertyChanged(nameof(CriancaSelecionada));
                return;
            }
            // Se desmarcou, limpa tudo
            if (CriancaSelecionada == null)
            {
                CriancaSelecionada = new Crianca();
                OnPropertyChanged(nameof(CriancaSelecionada));
            }
            NovoCliente = cliente;
            ListaCriancas.Clear();
            foreach (var cr in cliente.Criancas ?? Enumerable.Empty<Crianca>())
                ListaCriancas.Add(cr);

      
            // 2) Repopula a lista de crianças
            ListaCriancas.Clear();
            foreach (var cr in cliente.Criancas ?? Enumerable.Empty<Crianca>())
                ListaCriancas.Add(cr);

            if (cliente.Criancas?.Count == 1)
                CriancaSelecionada = cliente.Criancas[0];

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

        public void PreencherCampoCrianca(string? nomeDigitado, Action<Crianca> preencher)
        {
            if (string.IsNullOrWhiteSpace(nomeDigitado))
                return;

            var crianca = ListaCriancas.FirstOrDefault(c =>
                string.Equals(c.Nome.Trim(), nomeDigitado.Trim(), StringComparison.OrdinalIgnoreCase));
            if (crianca is not null)
                preencher(crianca);
        }

        public void PreencherPacote(string? pacoteDigitado, Action<decimal> preencher)
        {
            if (string.IsNullOrWhiteSpace(pacoteDigitado))
                return;

            if (_pacotesFixos.TryGetValue(pacoteDigitado.Trim(), out var valor))
            {
                preencher(valor);
            }
        }
      

        [RelayCommand]
        private void Agendar()
        {
            if (NovoCliente == null || NovoCliente.Id == 0 || string.IsNullOrWhiteSpace(NovoCliente.Nome))
                return;

            if (string.IsNullOrWhiteSpace(NovoAgendamento.Horario))
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
                    Idade = NovoAgendamento.Crianca.Idade,
                    Genero = NovoAgendamento.Crianca.Genero,
                    IdadeUnidade = NovoAgendamento.Crianca.IdadeUnidade,
                    ClienteId = clienteExistente.Id
                };
                _criancaService.AddOrUpdate(criancaParaAgendar);
            }

            // 4) Prepara o objeto a salvar
            NovoAgendamento.ClienteId = clienteExistente.Id;
            NovoAgendamento.CriancaId = criancaParaAgendar?.Id;

            bool agendamentoNovo = NovoAgendamento.Id == 0;

            if (agendamentoNovo)
            {
                _agendamentoService.Add(NovoAgendamento);
            }
            else
            {
                _agendamentoService.Update(NovoAgendamento);
            }
            var cliente = _clienteService.GetById(NovoAgendamento.ClienteId);
            var crianca = NovoAgendamento.CriancaId.HasValue
                ? _criancaService.GetById(NovoAgendamento.CriancaId.Value)
                : null;

            NovoAgendamento.Cliente = cliente;
            NovoAgendamento.Crianca = crianca;
            var textoCrianca = crianca != null
            ? $" {crianca.Nome} ({crianca.Idade} {crianca.IdadeUnidade})\n"
            : "";


            var texto = Uri.EscapeDataString($"✅ Agendado: {NovoAgendamento.Data:dd/MM/yyyy} às {NovoAgendamento.Horario} ({NovoAgendamento.Data.ToString("dddd", new CultureInfo("pt-BR"))}) \n\n" +
                            $"Cliente: {cliente.Nome} - {textoCrianca}" +
                            $"Telefone: {cliente.Telefone}\n" +
                            $"Tema: {NovoAgendamento.Tema}\n" +
                            $"Pacote: {NovoAgendamento.Pacote}\n" +
                            $"Valor: R$ {NovoAgendamento.Valor:N2} | Pago: R$ {NovoAgendamento.ValorPago:N2}\n" +

                            $"📍 *AVISOS*:\r\n\r\n-  A criança tem direito a *dois* acompanhantes 👶👩🏻‍\U0001f9b0👨🏻‍\U0001f9b0" +
                            $" o terceiro acompanhante paga R$ 20,00\r\n- A sessão fotográfica tem duração de até 1 hora." +
                            $"\r\n- *Tolerância máxima de atraso: 30 minutos*🚨" +
                            $"  (A partir de 30 minutos de atraso não atendemos mais, será necessário agendar outra data)." +
                            $" *PRAZO DE ENVIAR FOTOS TRATADAS DE 48HS DIAS ÚTEIS; APÓS O CLIENTE ESCOLHER NO APLICATIVO ALBOOM*");
                Clipboard.SetText(texto);
                MessageBox.Show("Agendamento copiado para a área de transferência!");

                var telefone = cliente.Telefone;
                string telefoneFormatado = $"55859{Regex.Replace(telefone, @"\D", "")}";
                string url = $"https://web.whatsapp.com/send?phone={telefoneFormatado}&text={texto}";
                Thread.Sleep(500);
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });

           


            CarregarDadosDoBanco();
            AtualizarAgendamentos();
            AtualizarHorariosDisponiveis();
            LimparCampos();
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
            OnPropertyChanged(nameof(AgendamentosDomingo));
            OnPropertyChanged(nameof(AgendamentosSegunda));
            OnPropertyChanged(nameof(AgendamentosTerca));
            OnPropertyChanged(nameof(AgendamentosQuarta));
            OnPropertyChanged(nameof(AgendamentosQuinta));
            OnPropertyChanged(nameof(AgendamentosSexta));
            OnPropertyChanged(nameof(AgendamentosSabado));
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
            var dataDoDia = inicioSemana.AddDays((int)dia - 1);

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

        public IEnumerable<Agendamento> AgendamentosDomingo => FiltrarPorDia(DayOfWeek.Sunday);


        public IEnumerable<Agendamento> AgendamentosSegunda => FiltrarPorDia(DayOfWeek.Monday);


        public IEnumerable<Agendamento> AgendamentosTerca => FiltrarPorDia(DayOfWeek.Tuesday);

        public IEnumerable<Agendamento> AgendamentosQuarta => FiltrarPorDia(DayOfWeek.Wednesday);


        public IEnumerable<Agendamento> AgendamentosQuinta => FiltrarPorDia(DayOfWeek.Thursday);

        public IEnumerable<Agendamento> AgendamentosSexta => FiltrarPorDia(DayOfWeek.Friday);


        public IEnumerable<Agendamento> AgendamentosSabado => FiltrarPorDia(DayOfWeek.Saturday);

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
    }



}
