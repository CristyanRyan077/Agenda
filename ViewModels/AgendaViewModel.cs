using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using AgendaNovo.Models;
using System.Windows.Data;
using System.Windows;
using Microsoft.EntityFrameworkCore;

namespace AgendaNovo
{
    public partial class AgendaViewModel : ObservableObject
    {
        //Agendamento
        [ObservableProperty] private Agendamento novoAgendamento = new();
        [ObservableProperty] private ObservableCollection<Agendamento> listaAgendamentos = new();
        [ObservableProperty] private ObservableCollection<Agendamento> agendamentosFiltrados = new();
        [ObservableProperty] private decimal valorPacote;
        [ObservableProperty] private Agendamento? itemSelecionado;
        public ObservableCollection<string> PacotesDisponiveis => new(_pacotesFixos.Keys);

        //Cliente
        [ObservableProperty] private Cliente? clienteSelecionado;
        [ObservableProperty] private Cliente novoCliente = new();
        [ObservableProperty] private ObservableCollection<Cliente> listaClientes = new();

        //Crianca
        [ObservableProperty] private ObservableCollection<Crianca> listaCriancas = new();

        //Data e horario
        [ObservableProperty] private DateTime dataSelecionada = DateTime.Today;
        [ObservableProperty] private ObservableCollection<string> horariosDisponiveis = new();

        public void Inicializar()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ListaAgendamentos.Clear();
                foreach (var item in _db.Agendamentos.Include(a => a.Cliente).Include(a => a.Crianca))
                    ListaAgendamentos.Add(item);

                ListaClientes.Clear();
                foreach (var cliente in _db.Clientes.Include(c => c.Criancas))
                    ListaClientes.Add(cliente);

                FiltrarAgendamentos();
                AtualizarHorariosDisponiveis();
            });
        }

        private readonly List<string> _horariosFixos = new()
        {
            "8:00", "9:00", "10:00", "13:00", "14:00", "15:00", "16:00", "17:00", "18:00"
        };

        private readonly Dictionary<string, decimal> _pacotesFixos = new()
                {
        
            {"Smash The Cake - Compartilhado",350m},
            {"Smash The Cake - Pct 01: Basico",450m},
            {"Smash The Cake - Pct 02: Premium", 600m},
            {"Book Mensal - Acompanhamento: pct01",200m},
            {"Book Mensal - Acompanhamento: pct02",150m},
            {"Book Mensal - Datas Comemorativas",100m},
            {"Gestante - pct01: Prata",200m },
            {"Gestante - pct02: Ouro",350m },
            {"Gestante - pct03: Diamante",550m },
            {"Infantil pct01", 150m },
            {"Infantil pct02", 250m },
            {"Aniversario pct01", 450m },   
            {"Aniversario pct02", 600m },
            {"Aniversario pct03", 900m },
            {"Aniversario pct04", 1300m },
            {"Evento - Casamento Civil: pct01",350m},
            {"Evento - Casamento Civil: pct02",550m},
            {"Evento - Casamentos: pct01",500m},

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
                // Apenas os pagos
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

        public bool DiaChkSeg => DiaAtual == DayOfWeek.Monday;
        public bool DiaChkTer => DiaAtual == DayOfWeek.Tuesday;
        public bool DiaChkQua => DiaAtual == DayOfWeek.Wednesday;
        public bool DiaChkQui => DiaAtual == DayOfWeek.Thursday;
        public bool DiaChkSex => DiaAtual == DayOfWeek.Friday;
        public bool DiaChkSab => DiaAtual == DayOfWeek.Saturday;
        public bool DiaChkDom => DiaAtual == DayOfWeek.Sunday;

        private void FiltrarAgendamentos()
        {
            AgendamentosFiltrados.Clear();

            var filtrados = ListaAgendamentos
                .Where(a => a.Data.Date == DataSelecionada.Date);   
            foreach (var agendamento in filtrados)
                AgendamentosFiltrados.Add(agendamento);
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
                ValorPendente = value.ValorPendente
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

            OnPropertyChanged(nameof(NovoAgendamento));
            OnPropertyChanged(nameof(NovoCliente));
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
            ClienteSelecionado = null;
            ListaCriancas.Clear();
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

                // Atualiza a lista de crianças
                var criancas = value.Criancas ?? new List<Crianca>();
                foreach (var crianca in criancas)
                    ListaCriancas.Add(crianca);

                // Se houver apenas uma criança, seleciona automaticamente
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

            ListaAgendamentos.Clear();
            foreach (var item in _db.Agendamentos
                                    .Include(a => a.Cliente)
                                    .Include(a => a.Crianca))
            {
                ListaAgendamentos.Add(item);
            }
      
        }
        public void PreencherCamposSeClienteExistir(string? nomeDigitado, Action<Cliente> preencher)
        {
            if (string.IsNullOrWhiteSpace(nomeDigitado))
                return;

            var cliente = ListaClientes.FirstOrDefault(c =>
                string.Equals(c.Nome.Trim(), nomeDigitado.Trim(), StringComparison.OrdinalIgnoreCase));

            if (cliente is not null)
            {
                preencher(cliente);

                ListaCriancas.Clear();
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
            using var db = new AgendaContext();

            var clientes = _db.Clientes.Include(c => c.Criancas).ToList();
            var agendamentos = _db.Agendamentos.Include(a => a.Cliente).Include(a => a.Crianca).ToList();

            Application.Current.Dispatcher.Invoke(() =>
            {
                ListaClientes.Clear();
                foreach (var cliente in clientes)
                    ListaClientes.Add(cliente);

                ListaAgendamentos.Clear();
                foreach (var a in agendamentos)
                    ListaAgendamentos.Add(a);

                FiltrarAgendamentos();
                AtualizarHorariosDisponiveis();
            });
        }

        [RelayCommand]
        private void Agendar()
        {
            if (NovoAgendamento == null)
                NovoAgendamento = new Agendamento();
            if (string.IsNullOrWhiteSpace(NovoCliente.Nome))
                return;
            var clienteExistente = ListaClientes.FirstOrDefault(c => c.Nome == NovoCliente.Nome);
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

                clienteExistente.Criancas.Add(criancaParaAgendar);
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
                agendamentoExistente.ValorPendente = NovoAgendamento.ValorPendente;
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
                    ValorPendente = NovoAgendamento.ValorPendente
                };
                _db.Agendamentos.Add(novo);
            }


            _db.SaveChanges();

            ListaCriancas.Clear();
            foreach (var c in clienteExistente.Criancas)
                ListaCriancas.Add(c);

            CarregarDadosDoBanco();
            AtualizarAgendamentos();
            AtualizarHorariosDisponiveis();
            FiltrarAgendamentos();
            ItemSelecionado = null;
            LimparCampos();
        }

  
        private void AtualizarAgendamentos()
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
                _db.Attach(ItemSelecionado);
                _db.Agendamentos.Remove(ItemSelecionado);
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

            NovoAgendamento = new Agendamento
            {
                Cliente = new Cliente(),
                Crianca = new Crianca(),
                Data = DateTime.Today
            };
            NovoCliente = new Cliente();
            NovoCliente.Telefone = string.Empty;
            ClienteSelecionado = null;
            ListaCriancas.Clear();
            OnPropertyChanged(nameof(NovoAgendamento));
            OnPropertyChanged(nameof(NovoCliente));
            OnPropertyChanged(nameof(ClienteSelecionado));
            OnPropertyChanged(nameof(ListaCriancas));
            AtualizarHorariosDisponiveis();
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
    }


}
