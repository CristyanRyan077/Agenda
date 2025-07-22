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

namespace AgendaNovo
{
    public partial class AgendaViewModel : ObservableObject
    {
        //Agendamento
        [ObservableProperty] private Agendamento novoAgendamento = new();
        [ObservableProperty] private ObservableCollection<Agendamento> listaAgendamentos = new();
        [ObservableProperty] private ObservableCollection<Agendamento> agendamentosFiltrados = new();
        [ObservableProperty] private decimal valorPacote;
        public ObservableCollection<string> ListaPacotes { get; } = new();
        [ObservableProperty] private Agendamento? itemSelecionado;
        [ObservableProperty] private ObservableCollection<ClienteCriancaView> listaClienteCrianca = new();


        //Cliente
        [ObservableProperty] private Cliente? clienteSelecionado;
        [ObservableProperty] private Cliente novoCliente = new();
        [ObservableProperty] private ObservableCollection<Cliente> listaClientes = new();
        [ObservableProperty] private ObservableCollection<Crianca> listaCriancasDoCliente = new();
        private bool _verificacaoJaFeita;
        public bool VerificacaoJaFeita
        {
            get => _verificacaoJaFeita;
            set => SetProperty(ref _verificacaoJaFeita, value);
        }

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



        public void Inicializar()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                CarregarDadosDoBanco();
                CarregarPacotes();
                AtualizarHorariosDisponiveis();
            });
        }

        public void CarregarPacotes()
        {
            ListaPacotes.Clear();
            foreach (var nome in _pacotesFixos.Keys.OrderBy(p => p))
                ListaPacotes.Add(nome);
        }
        private void ResetarFormulario()
        {
            NovoAgendamento = new Agendamento
            {
                Cliente = new Cliente(),
                Crianca = new Crianca(),
                Data = DateTime.Today
            };
            NovoCliente = new Cliente();
            ClienteSelecionado = null;
            CriancaSelecionada = new Crianca();
            ListaCriancas.Clear();
            ListaCriancasDoCliente.Clear();
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

        public ObservableCollection<string> UnidadesIdade { get; } = new()
        {
            "meses",
            "anos"
        };

        [RelayCommand]
        private void LimparAnteriores()
        {
            var resultado = MessageBox.Show(
                "Deseja apagar:\n\nSim - Apenas agendamentos pagos\nNão - Todos os anteriores\nCancelar - Nenhum",
                "Remover Agendamentos Antigos",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Warning);
            var fantasmas = _db.Agendamentos
            .Where(a => a.Crianca == null)
            .ToList();

            if (fantasmas.Any())
            {
                _db.Agendamentos.RemoveRange(fantasmas);
                _db.SaveChanges();
            }

            var anteriores = ListaAgendamentos
                .Where(a => a.Data.Date < DateTime.Today)
                .ToList(); // Evita CollectionChanged erro



            if (resultado == MessageBoxResult.Yes)
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
            }

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
            try
            {
                LimparCampos();
                if (value == null) return;
                // 1) Atualiza a data e recalcula horários
                DataSelecionada = value.Data;
                AtualizarHorariosDisponiveis();
                if (!string.IsNullOrEmpty(value.Horario)
                && !HorariosDisponiveis.Contains(value.Horario))
                {
                    HorariosDisponiveis.Insert(0, value.Horario);
                }

                // 2) Localiza instância exata do cliente e da criança
                var cliente = ListaClientes.FirstOrDefault(c => c.Id == value.ClienteId);
                if (cliente == null)
                    return;
                ClienteSelecionado = cliente;
                NovoCliente = cliente;

                ListaCriancas.Clear();
                foreach (var cr in cliente.Criancas ?? Enumerable.Empty<Crianca>())
                    ListaCriancas.Add(cr);

                var crianca = value.Crianca != null
                    ? cliente.Criancas.FirstOrDefault(c => c.Id == value.Crianca.Id)
                    : null;
                CriancaSelecionada = crianca;

                NovoAgendamento = new Agendamento
                {
                    Id = value.Id,
                    Cliente = cliente,
                    Crianca = crianca ?? new Crianca(),
                    Data = value.Data,
                    Horario = value.Horario,
                    Pacote = value.Pacote,
                    Tema = value.Tema,
                    Valor = value.Valor,
                    ValorPago = value.ValorPago
                };
                OnPropertyChanged(nameof(HorariosDisponiveis));
                OnPropertyChanged(nameof(NovoAgendamento));
                OnPropertyChanged(nameof(NovoAgendamento.Horario));
                OnPropertyChanged(nameof(NovoAgendamento.Tema));
                OnPropertyChanged(nameof(CriancaSelecionada));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao selecionar item: " + ex.Message);
            }
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
            NovoAgendamento = new Agendamento
            {
                Cliente = new Cliente(),
                Crianca = new Crianca(),
                Data = DateTime.Today
            };

            NovoCliente = new Cliente();
            NovoCliente.Id = 0;
            CriancaSelecionada = new Crianca();
            ClienteSelecionado = null;
            ListaCriancas.Clear();
            ListaCriancasDoCliente.Clear();
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



        partial void OnClienteSelecionadoChanged(Cliente value)
        {
            // Se desmarcou, limpa tudo
            if (value == null)
            {
                NovoCliente = new Cliente();
                ListaCriancas.Clear();
                CriancaSelecionada = null;
                return;
            }

            // 1) Usa a instância exata
            NovoCliente = value;
            ClienteSelecionado = value;

            // 2) Repopula a lista de crianças
            ListaCriancas.Clear();
            foreach (var cr in value.Criancas ?? Enumerable.Empty<Crianca>())
                ListaCriancas.Add(cr);

            // 3) Se só tem 1 criança, já a seleciona
            CriancaSelecionada = value.Criancas != null && value.Criancas.Count == 1
            ? value.Criancas[0]
            : null;  

            // dispara atualização de bindings
            OnPropertyChanged(nameof(NovoCliente));
            OnPropertyChanged(nameof(ListaCriancas));
            OnPropertyChanged(nameof(CriancaSelecionada));
        }

        private readonly AgendaContext _db;
        public AgendaViewModel(AgendaContext db)
        {

            _db = db;
            AtualizarHorariosDisponiveis();
            DiaAtual = DateTime.Today.DayOfWeek;
            NovoCliente = new Cliente();
            NovoAgendamento = new Agendamento
            {
                Cliente = NovoCliente,
                Crianca = new Crianca(),
            };

        }
        public AgendaContext DbContext => _db;
        public void PreencherCamposSeClienteExistir(string? nomeDigitado, Action<Cliente> preencher)
        {
            if (string.IsNullOrWhiteSpace(nomeDigitado))
                return;

            var cliente = ListaClientes.FirstOrDefault(c =>
                string.Equals(c.Nome.Trim(), nomeDigitado.Trim(), StringComparison.OrdinalIgnoreCase));

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
        public void CarregarDadosDoBanco()
        {

            var clientes = _db.Clientes.Include(c => c.Criancas).ToList();
            var agendamentos = _db.Agendamentos.Include(a => a.Cliente).Include(a => a.Crianca).ToList();

            Application.Current.Dispatcher.Invoke(() =>
            {
                ListaAgendamentos.Clear();
                foreach (var agendamento in agendamentos)
                    ListaAgendamentos.Add(agendamento);

                ListaClientes.Clear();
                foreach (var cliente in clientes)
                    ListaClientes.Add(cliente);
            });
            FiltrarAgendamentos();
            AtualizarHorariosDisponiveis();
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
            var clienteExistente = _db.Clientes
                .Include(c => c.Criancas)
                .FirstOrDefault(c => c.Id == NovoCliente.Id);

            if (clienteExistente == null)
                return;


            var criancaParaAgendar = clienteExistente.Criancas
            .FirstOrDefault(c => c.Nome == NovoAgendamento.Crianca?.Nome);

            if (criancaParaAgendar == null)
            {
                criancaParaAgendar = new Crianca
                {
                    Nome = NovoAgendamento.Crianca.Nome,
                    Idade = NovoAgendamento.Crianca.Idade,
                    Genero = NovoAgendamento.Crianca.Genero,
                    IdadeUnidade = NovoAgendamento.Crianca.IdadeUnidade,
                    ClienteId = clienteExistente.Id
                };
                var jaRastreada = _db.Criancas.Local
                    .FirstOrDefault(c => c.Nome == criancaParaAgendar.Nome
                     && c.ClienteId == criancaParaAgendar.ClienteId);

                if (jaRastreada == null)
                {
                    _db.Criancas.Add(criancaParaAgendar);
                }
            }
            else
            {
                criancaParaAgendar.Idade = NovoAgendamento.Crianca.Idade;
                criancaParaAgendar.Genero = NovoAgendamento.Crianca.Genero;
                criancaParaAgendar.IdadeUnidade = NovoAgendamento.Crianca.IdadeUnidade;
            }


            if (NovoAgendamento.Id > 0)
            {
                var agendamentoExistente = _db.Agendamentos.Find(NovoAgendamento.Id);
                agendamentoExistente.ClienteId = clienteExistente.Id;
                agendamentoExistente.CriancaId = criancaParaAgendar.Id;
                agendamentoExistente.Pacote = NovoAgendamento.Pacote;
                agendamentoExistente.Tema = NovoAgendamento.Tema;
                agendamentoExistente.Horario = NovoAgendamento.Horario;
                agendamentoExistente.Data = DataSelecionada.Date;
                agendamentoExistente.Valor = NovoAgendamento.Valor;
                agendamentoExistente.ValorPago = NovoAgendamento.ValorPago;

               
            }
            else
            {
                // Cria um novo agendamento
                var novo = new Agendamento
                {
                    ClienteId = clienteExistente.Id,
                    CriancaId = criancaParaAgendar.Id,
                    Pacote = NovoAgendamento.Pacote,
                    Horario = NovoAgendamento.Horario,
                    Data = DataSelecionada.Date,
                    Tema = NovoAgendamento.Tema,
                    Valor = NovoAgendamento.Valor,
                    ValorPago = NovoAgendamento.ValorPago
                };
                _db.Agendamentos.Add(novo);

                string textoCrianca = "";

                if (novo.Crianca != null && !string.IsNullOrWhiteSpace(novo.Crianca.Nome))
                {
                    textoCrianca = $" {novo.Crianca.Nome} ({novo.Crianca.Idade} {novo.Crianca.IdadeUnidade})\n";
                }

                var texto = Uri.EscapeDataString($"✅ Agendado: {novo.Data:dd/MM/yyyy} às {novo.Horario} ({novo.Data.ToString("dddd", new CultureInfo("pt-BR"))}) \n\n" +
                            $"Cliente: {novo.Cliente.Nome} - {textoCrianca}" +
                            $"Telefone: {novo.Cliente.Telefone}\n" +
                            $"Tema: {novo.Tema}\n" +
                            $"Pacote: {novo.Pacote}\n" +
                            $"Valor: R$ {novo.Valor:N2} | Pago: R$ {novo.ValorPago:N2}\n" +

                            $"📍 *AVISOS*:\r\n\r\n-  A criança tem direito a *dois* acompanhantes 👶👩🏻‍\U0001f9b0👨🏻‍\U0001f9b0" +
                            $" o terceiro acompanhante paga R$ 20,00\r\n- A sessão fotográfica tem duração de até 1 hora." +
                            $"\r\n- *Tolerância máxima de atraso: 30 minutos*🚨" +
                            $"  (A partir de 30 minutos de atraso não atendemos mais, será necessário agendar outra data)." +
                            $" *PRAZO DE ENVIAR FOTOS TRATADAS DE 48HS DIAS ÚTEIS; APÓS O CLIENTE ESCOLHER NO APLICATIVO ALBOOM*");
                Clipboard.SetText(texto);
                MessageBox.Show("Agendamento copiado para a área de transferência!");


                var telefone = novo.Cliente.Telefone;
                string telefoneFormatado = $"55859{Regex.Replace(telefone, @"\D", "")}";
                string url = $"https://web.whatsapp.com/send?phone={telefoneFormatado}&text={texto}";
                Thread.Sleep(1000);
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });

            }


            _db.SaveChanges();

            foreach (var c in clienteExistente.Criancas)
                ListaCriancas.Add(c);
            var clientes = _db.Clientes.Include(c => c.Criancas).ToList();
            var agendamentos = _db.Agendamentos.Include(a => a.Cliente).Include(a => a.Crianca).ToList();
                ListaAgendamentos.Clear();
                foreach (var agendamento in agendamentos)
                    ListaAgendamentos.Add(agendamento);

                ListaClientes.Clear();
                foreach (var cliente in clientes)
                    ListaClientes.Add(cliente);
            CarregarDadosDoBanco();
            AtualizarAgendamentos();
            AtualizarHorariosDisponiveis();
            FiltrarAgendamentos();
            LimparCampos();
            ItemSelecionado = null;

        }
        public void AtualizarPago(Agendamento agendamento)
        {
            if (agendamento == null) return;

            // Anexa ou obtém a entidade rastreada pelo contexto
            var agendamentoDb = _db.Agendamentos.Find(agendamento.Id);

            if (agendamentoDb != null)
            {
                _db.Agendamentos.Remove(agendamentoDb);
                _db.SaveChanges();
                ListaAgendamentos.Remove(agendamento);
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
            if (ItemSelecionado is not null)
            {
                var entidade = _db.Agendamentos.FirstOrDefault(a => a.Id == ItemSelecionado.Id);
                if (entidade is not null)
                {
                    _db.Agendamentos.Remove(entidade);
                }
            }

            _db.SaveChanges();
            CarregarDadosDoBanco();

            ListaAgendamentos.Remove(ItemSelecionado);
            AgendamentosFiltrados.Remove(ItemSelecionado);

            FiltrarAgendamentos();
            AtualizarAgendamentos();

            bool clienteAindaTemAgendamentos = ListaAgendamentos.Any(a =>
            a.Cliente?.Nome == ClienteSelecionado?.Nome);

            if (!clienteAindaTemAgendamentos && ClienteSelecionado != null)
            {
                var clienteNaLista = ListaClientes.FirstOrDefault(c => c.Nome == ClienteSelecionado.Nome);
                if (clienteNaLista != null)
                    ListaClientes.Remove(clienteNaLista);
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
