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

        //Crianca
        [ObservableProperty] private ObservableCollection<Crianca> listaCriancas = new();
        [ObservableProperty] private Crianca? criancaSelecionada = new();

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



        public void FiltrarAgendamentos()
        {
            AgendamentosFiltrados.Clear();

            var filtrados = ListaAgendamentos
                .Where(a => a.Data.Date == DataSelecionada.Date)
                .ToList();
            foreach (var item in filtrados)
                AgendamentosFiltrados.Add(item);
        }

        [RelayCommand]
        private void CopiarHorariosLivres()
        {
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

            string texto = $"Horários livres em {DataSelecionada:dd/MM/yyyy}:\n\n" + string.Join(", ", livres);
            Clipboard.SetText(texto);

            MessageBox.Show("Horários livres copiados para a área de transferência!", "Copiado", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        partial void OnItemSelecionadoChanged(Agendamento? value)
        {
            if (value?.Cliente == null)
                return;

            NovoAgendamento = new Agendamento
            {
                Id = value.Id,
                Cliente = new Cliente
                
                {
                    Nome = value.Cliente?.Nome ?? string.Empty,
                    Telefone = value.Cliente?.Telefone ?? string.Empty
                },
                Crianca = value.Crianca is not null ? new Crianca
                {
                    Nome = value.Crianca.Nome,
                    Idade = value.Crianca.Idade,
                    Genero = value.Crianca.Genero,
                    IdadeUnidade = value.Crianca.IdadeUnidade
                } : new Crianca(),
                Data = value.Data,
                Horario = value.Horario,
                Pacote = value.Pacote,
                Tema = value.Tema,
                Valor = value.Valor,
                ValorPago = value.ValorPago
            };
            DataSelecionada = value.Data;
            NovoCliente = new Cliente
            {
                Nome = value.Cliente?.Nome ?? string.Empty,
                Telefone = value.Cliente?.Telefone ?? string.Empty
            };

            if (value.Cliente?.Criancas != null)
            {
                foreach (var crianca in value.Cliente.Criancas)
                    ListaCriancas.Add(crianca);
            }
            OnPropertyChanged(nameof(NovoAgendamento.Tema));
            OnPropertyChanged(nameof(NovoAgendamento.Horario));
            OnPropertyChanged(nameof(NovoAgendamento.Crianca));
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
            var ocupados = ListaAgendamentos
                .Where(a => a.Data.Date == DataSelecionada.Date)
                .Select(a => a.Horario)
                .ToList();

            var livres = _horariosFixos
                .Where(h => !ocupados.Contains(h))
                .ToList();

            HorariosDisponiveis.Clear();
            foreach (var h in livres)
                HorariosDisponiveis.Add(h);
        }

        partial void OnNovoAgendamentoChanged(Agendamento value)
        {
            if (value?.Cliente != null)
            {
                NovoCliente = new Cliente
                {
                    Nome = value.Cliente.Nome,
                    Telefone = value.Cliente.Telefone
                };

                ClienteSelecionado = ListaClientes
                    .FirstOrDefault(c => c.Nome == NovoCliente.Nome);
            }
            else
            {
                NovoCliente = new Cliente();
                ClienteSelecionado = null;
            }
            
        }

        partial void OnClienteSelecionadoChanged(Cliente value)
        {
            if (value != null)
            {
                NovoCliente.Nome = value.Nome;
                NovoCliente.Telefone = value.Telefone;
                NovoCliente.Email = value.Email;

                var criancas = value.Criancas ?? new List<Crianca>();
                foreach (var crianca in criancas)
                    ListaCriancas.Add(crianca);

                if (criancas.Count == 1)
                {
                    var unica = criancas.First();
                    NovoAgendamento.Crianca = new Crianca
                    {
                        Nome = unica.Nome,
                        Idade = unica.Idade,
                        Genero = unica.Genero,
                        IdadeUnidade = unica.IdadeUnidade
                    };
                }
            }
            else
            {
                NovoCliente = new Cliente();
                ListaCriancas.Clear();
            }

            // Força a atualização dos bindings
            OnPropertyChanged(nameof(NovoAgendamento));
            OnPropertyChanged(nameof(NovoAgendamento.Crianca));
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
            if (NovoAgendamento == null)
                NovoAgendamento = new Agendamento();
            if (string.IsNullOrWhiteSpace(NovoCliente.Nome))
                return;
            var clienteExistente = _db.Clientes
                .Include(c => c.Criancas)
                .FirstOrDefault(c => c.Nome == NovoCliente.Nome);
            if (clienteExistente == null)
            {
                clienteExistente = new Cliente
                {
                    Nome = NovoCliente.Nome,
                    Telefone = NovoCliente.Telefone,
                    Criancas = new List<Crianca>()
                };

                _db.Clientes.Add(clienteExistente);
                _db.SaveChanges();
                ListaClientes.Add(clienteExistente);
            }
            else
            {
                clienteExistente.Telefone = NovoCliente.Telefone;
                clienteExistente.Criancas ??= new List<Crianca>();
            }

            var criancaParaAgendar = clienteExistente.Criancas
            .FirstOrDefault(c => c.Nome == NovoAgendamento.Crianca.Nome);

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

            var agendamentoExistente = _db.Agendamentos
                .Include(a => a.Cliente)
                .Include(a => a.Crianca)
                .FirstOrDefault(a => a.Id == NovoAgendamento.Id);
            if (agendamentoExistente != null)
            {
                agendamentoExistente.Cliente.Nome = NovoCliente.Nome;
                agendamentoExistente.Cliente.Telefone = NovoCliente.Telefone;

                agendamentoExistente.Crianca ??= new Crianca();
                agendamentoExistente.Crianca.Nome = NovoAgendamento.Crianca.Nome;
                agendamentoExistente.Crianca.Idade = NovoAgendamento.Crianca.Idade;
                agendamentoExistente.Crianca.Genero = NovoAgendamento.Crianca.Genero;
                agendamentoExistente.Crianca.IdadeUnidade = NovoAgendamento.Crianca.IdadeUnidade;

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
                    Cliente = clienteExistente,
                    Crianca = criancaParaAgendar,
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

                var texto = Uri.EscapeDataString($"Agendamento Confirmado\n\n" +
                            $"Cliente: {novo.Cliente.Nome} - {textoCrianca}" +
                            $"Telefone: {novo.Cliente.Telefone}\n" +
                            $"Tema: {novo.Tema}\n" +
                            $"Pacote: {novo.Pacote}\n" +
                            $"Data: {novo.Data:dd/MM/yyyy} às {novo.Horario}\n" +
                            $"Valor: R$ {novo.Valor:N2} | Pago: R$ {novo.ValorPago:N2}");
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

            CarregarDadosDoBanco();
            AtualizarAgendamentos();
            AtualizarHorariosDisponiveis();
            FiltrarAgendamentos();
            ItemSelecionado = null;
            LimparCampos();

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
        private IEnumerable<Agendamento> FiltrarPorDia(DayOfWeek dia) =>
        ListaAgendamentos.Where(a =>
        a.Data.DayOfWeek == dia &&
        a.Data.Date >= DateTime.Today &&
        a.Data.Date <= FimDaSemana);

        private DateTime FimDaSemana => DateTime.Today.AddDays(6);

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
    }



}
