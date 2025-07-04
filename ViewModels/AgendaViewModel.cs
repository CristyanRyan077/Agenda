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

namespace AgendaNovo
{
    public partial class AgendaViewModel : ObservableObject
    {
        //Agendamento
        [ObservableProperty] private Agendamento novoAgendamento = new();
        [ObservableProperty] private ObservableCollection<Agendamento> listaAgendamentos = new();
        [ObservableProperty] private ObservableCollection<Agendamento> agendamentosFiltrados = new();
        [ObservableProperty] private decimal valorPacote;
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

        private readonly List<string> _horariosFixos = new()
        {
            "8:00", "9:00", "10:00", "13:00", "14:00", "15:00", "16:00", "17:00", "18:00"
        };

        private readonly Dictionary<string, decimal> _pacotesFixos = new()
{
        { "Infantil pct01", 150m },
        { "Gestante pct01", 200m },
        { "Aniversario pct01", 450m }
        };

<<<<<<< HEAD
        public ObservableCollection<string> UnidadesIdade { get; } = new()
        {
            "meses",
            "anos"
        };

=======
>>>>>>> 339a18d4c7781204c8c80dd673286d34e4ee8714
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
                ListaCriancas.Clear();
                foreach (var crianca in value.Criancas)
                {
                    ListaCriancas.Add(crianca);
                }

                // Se houver apenas uma criança, seleciona automaticamente
                if (value.Criancas.Count == 1)
                {
<<<<<<< HEAD
                    var crianca = value.Criancas.First();
                    NovoAgendamento.Crianca = new Crianca
                    {
                        Nome = crianca.Nome,
                        Idade = crianca.Idade,
                        Genero = crianca.Genero
                    };
=======
                    NovoAgendamento.Crianca = value.Criancas.First();
>>>>>>> 339a18d4c7781204c8c80dd673286d34e4ee8714
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


        public AgendaViewModel()
        {
            AtualizarHorariosDisponiveis();
            DiaAtual = DateTime.Today.DayOfWeek;
            NovoCliente = new Cliente();
            NovoAgendamento = new Agendamento
            {
                Cliente = NovoCliente,
                Crianca = new Crianca(),
            };
            ListaAgendamentos = new ObservableCollection<Agendamento>();
            ListaClientes = new ObservableCollection<Cliente>();
            agendamentosFiltrados = new ObservableCollection<Agendamento>();
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

                listaCriancas.Clear();
                foreach (var crianca in cliente.Criancas)
                    listaCriancas.Add(crianca);
            }
        }
        public void PreencherCampoCrianca(string? nomeDigitado, Action<Crianca> preencher)
        {
            if (string.IsNullOrWhiteSpace(nomeDigitado))
                return;

            var crianca = listaCriancas.FirstOrDefault(c =>
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

        [RelayCommand] private void Agendar()
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
                ListaClientes.Add(clienteExistente);
<<<<<<< HEAD
            }
            else
=======
            }
            else
            {
                clienteExistente.Criancas ??= new List<Crianca>();
            }
            if (NovoAgendamento.Crianca != null &&
            !string.IsNullOrWhiteSpace(NovoAgendamento.Crianca.Nome))
            {
                var criancaExistente = clienteExistente.Criancas
                .FirstOrDefault(c => c.Nome == NovoAgendamento.Crianca.Nome);
                if (criancaExistente == null)
                {
                    clienteExistente.Criancas.Add(NovoAgendamento.Crianca);
                }
                else
                {
                    NovoAgendamento.Crianca = criancaExistente;
                }
            }

            var novo = new Agendamento
>>>>>>> 339a18d4c7781204c8c80dd673286d34e4ee8714
            {
                clienteExistente.Criancas ??= new List<Crianca>();
            }
            if (NovoAgendamento.Crianca != null &&
            !string.IsNullOrWhiteSpace(NovoAgendamento.Crianca.Nome))
            {
                var criancaExistente = clienteExistente.Criancas
                    .FirstOrDefault(c => c.Nome == NovoAgendamento.Crianca.Nome);

                if (criancaExistente == null)
                {
<<<<<<< HEAD
                    // Adiciona à lista do cliente (salva o "cadastro")
                    clienteExistente.Criancas.Add(new Crianca
                    {
                        Nome = NovoAgendamento.Crianca.Nome,
                        Idade = NovoAgendamento.Crianca.Idade,
                        Genero = NovoAgendamento.Crianca.Genero
                    });
                }
=======
                    Nome = NovoCliente.Nome,
                    Telefone = NovoCliente.Telefone,
                },
                Crianca = NovoAgendamento.Crianca,
                Pacote = NovoAgendamento.Pacote,
                Horario = NovoAgendamento.Horario,
                Data = NovoAgendamento.Data.Date,
                Tema = NovoAgendamento.Tema,
                Valor = NovoAgendamento.Valor
            };
>>>>>>> 339a18d4c7781204c8c80dd673286d34e4ee8714

                // SEMPRE cria uma nova instância da criança para o agendamento
                NovoAgendamento.Crianca = new Crianca
                {
                    Nome = NovoAgendamento.Crianca.Nome,
                    Idade = NovoAgendamento.Crianca.Idade,
                    Genero = NovoAgendamento.Crianca.Genero
                };


<<<<<<< HEAD
                var novo = new Agendamento
                {
                    Cliente = new Cliente
                    {
                        Nome = NovoCliente.Nome,
                        Telefone = NovoCliente.Telefone,
                    },
                    Crianca = new Crianca
                    {
                        Nome = NovoAgendamento.Crianca.Nome,
                        Idade = NovoAgendamento.Crianca.Idade,
                        Genero = NovoAgendamento.Crianca.Genero
                    },
                    Pacote = NovoAgendamento.Pacote,
                    Horario = NovoAgendamento.Horario,
                    Data = NovoAgendamento.Data.Date,
                    Tema = NovoAgendamento.Tema,
                    Valor = NovoAgendamento.Valor,
                    ValorPendente = NovoAgendamento.ValorPendente
                };
=======
            NovoAgendamento = new Agendamento { Crianca = new Crianca(), Data = NovoAgendamento.Data.Date };
            NovoCliente = new Cliente();
            AtualizarAgendamentos();
            AtualizarHorariosDisponiveis();
>>>>>>> 339a18d4c7781204c8c80dd673286d34e4ee8714

                ListaAgendamentos.Add(novo);


                NovoAgendamento = new Agendamento { Crianca = new Crianca(), Data = NovoAgendamento.Data.Date };
                NovoCliente = new Cliente();
                AtualizarAgendamentos();
                AtualizarHorariosDisponiveis();
            }
        }
<<<<<<< HEAD

=======
>>>>>>> 339a18d4c7781204c8c80dd673286d34e4ee8714
  
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
            ListaAgendamentos.Remove(NovoAgendamento);
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
