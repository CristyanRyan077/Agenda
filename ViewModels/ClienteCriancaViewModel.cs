using AgendaNovo.Migrations;
using AgendaNovo.Models;
using AgendaNovo.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
            crianca.Idade = 0;
            crianca.Genero = string.Empty;
            crianca.IdadeUnidade = string.Empty;
        }
    }
    public partial class ClienteCriancaViewModel : ObservableObject
    {
        [ObservableProperty] private ObservableCollection<ClienteCriancaView> listaClienteCrianca = new();
        [ObservableProperty] private ClienteCriancaView? clienteCriancaSelecionado;
        [ObservableProperty] private bool clienteExistenteDetectado;
        [ObservableProperty] private bool isInEditMode;
        [ObservableProperty] private string pesquisaText;
        public IEnumerable<IdadeUnidade> IdadesUnidadeDisponiveis => Enum.GetValues(typeof(IdadeUnidade)).Cast<IdadeUnidade>();


        private readonly AgendaViewModel _agenda;
        private readonly AgendaContext _db;

        public ClienteCriancaViewModel(AgendaViewModel agenda)
        {
            
            _agenda = agenda;
            _db = agenda.DbContext;
            CarregarClientesDoBanco();
            AtualizarListaClienteCrianca();
            LimparCamposClienteCrianca();

        }
        private void NotifyAll()
        {
            OnPropertyChanged(nameof(NovoCliente));
            OnPropertyChanged(nameof(CriancaSelecionada));
            OnPropertyChanged(nameof(ClienteCriancaSelecionado));
            OnPropertyChanged(nameof(ListaClienteCrianca));
            OnPropertyChanged(nameof(ListaCriancasDoCliente));
            OnPropertyChanged(nameof(ListaCriancas));
            OnPesquisaTextChanged(PesquisaText);
        }
        private void CarregarClientesDoBanco()
        {
            _todosClientes = _db.Clientes
         .Include(c => c.Criancas)
         .ToList()
         .SelectMany(cliente =>
         {
             if (cliente.Criancas != null && cliente.Criancas.Count > 0)
             {
                 return cliente.Criancas.Select(crianca => new ClienteCriancaView
                 {
                     ClienteId = cliente.Id,
                     NomeCliente = cliente.Nome,
                     Telefone = cliente.Telefone,
                     Email = cliente.Email,
                     CriancaId = crianca.Id,
                     NomeCrianca = crianca.Nome,
                     Genero = Enum.TryParse<Genero>(crianca.Genero, out var genero) ? genero : Genero.M,
                     Idade = crianca.Idade,
                     IdadeUnidade = Enum.TryParse<IdadeUnidade>(crianca.IdadeUnidade, out var unidade) ? unidade : IdadeUnidade.Anos
                 });
             }
             else
             {
                 return new List<ClienteCriancaView>
                 {
                    new ClienteCriancaView
                    {
                        ClienteId = cliente.Id,
                        NomeCliente = cliente.Nome,
                        Telefone = cliente.Telefone,
                        Email = cliente.Email
                    }
                 };
             }
         })
         .ToList();

            ListaClienteCrianca = new ObservableCollection<ClienteCriancaView>(_todosClientes);
        }
        public void DetectarClientePorCampos()
        {
            if (IsInEditMode)
                return;

            bool nomeVazio = string.IsNullOrWhiteSpace(NovoCliente.Nome);
            bool telVazio = string.IsNullOrWhiteSpace(NovoCliente.Telefone);
            bool emailVazio = string.IsNullOrWhiteSpace(NovoCliente.Email);
            if (nomeVazio && telVazio && emailVazio)
            {
                ClienteExistenteDetectado = false;
                NovoCliente.Id = 0;
                ListaCriancasDoCliente.Clear();
                return;
            }


            Cliente? clienteDetectado = null;

            if (!nomeVazio)
            {
                clienteDetectado = ListaClientes
                    .FirstOrDefault(c => c.Nome.Equals(NovoCliente.Nome, StringComparison.OrdinalIgnoreCase));
            }
            if (clienteDetectado is null && !telVazio)
            {
                clienteDetectado = ListaClientes
                    .FirstOrDefault(c => c.Telefone?.Equals(NovoCliente.Telefone, StringComparison.OrdinalIgnoreCase) == true);
            }
            if (clienteDetectado is null && !emailVazio)
            {
                clienteDetectado = ListaClientes
                    .FirstOrDefault(c => c.Email?.Equals(NovoCliente.Email, StringComparison.OrdinalIgnoreCase) == true);
            }

            if (clienteDetectado is not null)
            {
                // Preenche dados
                NovoCliente.Id = clienteDetectado.Id;
                NovoCliente.Nome = clienteDetectado.Nome;
                NovoCliente.Telefone = clienteDetectado.Telefone;
                NovoCliente.Email = clienteDetectado.Email;

                ListaCriancasDoCliente.Clear();
                foreach (var c in clienteDetectado.Criancas)
                    ListaCriancasDoCliente.Add(c);

                ClienteExistenteDetectado = true;
            }
            else
            {
                ClienteExistenteDetectado = false;
            }
        }



        // 🔁 Compartilhamento de propriedades da AgendaViewModel
        public Cliente NovoCliente => _agenda.NovoCliente;
        public ObservableCollection<Cliente> ListaClientes => _agenda.ListaClientes;

        private List<ClienteCriancaView> _todosClientes = new();

        public Crianca CriancaSelecionada
        {
            get => _agenda.CriancaSelecionada;
            set => _agenda.CriancaSelecionada = value;
        }

        public ObservableCollection<Crianca> ListaCriancas => _agenda.ListaCriancas;
        public ObservableCollection<Crianca> ListaCriancasDoCliente => _agenda.ListaCriancasDoCliente;




        public void AtualizarListaClienteCrianca()
        {
            ListaClienteCrianca.Clear();

            foreach (var cliente in ListaClientes)
            {
                if (cliente.Criancas != null && cliente.Criancas.Count > 0)
                {
                    foreach (var crianca in cliente.Criancas)
                    {
                        ListaClienteCrianca.Add(new ClienteCriancaView
                        {
                            ClienteId = cliente.Id,
                            NomeCliente = cliente.Nome,
                            Telefone = cliente.Telefone,
                            Email = cliente.Email,
                            CriancaId = crianca.Id,
                            NomeCrianca = crianca.Nome,
                            Genero = Enum.TryParse<Genero>(crianca.Genero, out var genero) ? genero : Genero.M,
                            Idade = crianca.Idade,
                            IdadeUnidade = Enum.TryParse<IdadeUnidade>(crianca.IdadeUnidade, out var unidade) ? unidade : IdadeUnidade.Anos
                        });
                    }
                }
                else
                {
                    // Cliente sem crianças
                    ListaClienteCrianca.Add(new ClienteCriancaView
                    {
                        ClienteId = cliente.Id,
                        NomeCliente = cliente.Nome,
                        Telefone = cliente.Telefone,
                        Email = cliente.Email,
                    });

                }

            }
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


            CarregarCriancasDoCliente(cliente);

            if (ClienteCriancaSelecionado.CriancaId is int criancaId)
            {
                var crianca = cliente.Criancas.FirstOrDefault(c => c.Id == criancaId);
                if (crianca is not null)
                {

                    CriancaSelecionada.Id = crianca.Id;
                    CriancaSelecionada.Nome = crianca.Nome;
                    CriancaSelecionada.Idade = crianca.Idade;
                    CriancaSelecionada.Genero = crianca.Genero;
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


            var clienteFoiEditado = NovoCliente.Id != 0 && ListaClientes.Any(c => c.Id == NovoCliente.Id);
            Cliente cliente;

            if (!clienteFoiEditado && ListaClientes.Any(c =>
                c.Nome.Equals(NovoCliente.Nome, StringComparison.OrdinalIgnoreCase)
                && c.Id != NovoCliente.Id))
            {
                MessageBox.Show("Já existe um cliente com esse nome.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }


            if (clienteFoiEditado)
            {
                // Atualiza cliente
                cliente = ListaClientes.First(c => c.Id == NovoCliente.Id);
                cliente.Nome = NovoCliente.Nome;
                cliente.Telefone = NovoCliente.Telefone;
                cliente.Email = NovoCliente.Email;
                _db.Clientes.Update(cliente);
                _db.SaveChanges();
            }
            else
            {
                cliente = new Cliente
                {
                    Nome = NovoCliente.Nome,
                    Telefone = NovoCliente.Telefone,
                    Email = NovoCliente.Email,
                    Criancas = new List<Crianca>()
                };

                
                _db.Clientes.Add(cliente);
                _db.SaveChanges();
                cliente = _db.Clientes.Include(c => c.Criancas).First(c => c.Id == cliente.Id);
                ListaClientes.Add(cliente);



            }

            // Verifica se há criança para salvar
            if (CriancaSelecionada != null && !string.IsNullOrWhiteSpace(CriancaSelecionada.Nome))
            {
                var crianca = cliente.Criancas.FirstOrDefault(c => c.Id == CriancaSelecionada.Id)
                           ?? cliente.Criancas.FirstOrDefault(c => c.Nome == CriancaSelecionada.Nome);

                if (crianca == null)
                {
                    crianca = new Crianca
                    {
                        Nome = CriancaSelecionada.Nome,
                        Idade = CriancaSelecionada.Idade,
                        Genero = CriancaSelecionada.Genero,
                        IdadeUnidade = CriancaSelecionada.IdadeUnidade,
                        ClienteId = cliente.Id,
                        Cliente = cliente
                    };
                    ListaCriancas.Add(crianca);
                    _db.Criancas.Add(crianca);
                }
                else
                {
                    crianca.Cliente = cliente;
                    crianca.Nome = CriancaSelecionada.Nome;
                    crianca.Idade = CriancaSelecionada.Idade;
                    crianca.Genero = CriancaSelecionada.Genero;
                    crianca.IdadeUnidade = CriancaSelecionada.IdadeUnidade;

                }
            }

            _db.SaveChanges();
            _db.Entry(cliente).Collection(c => c.Criancas).Load();

            AtualizarListaClienteCrianca();
            CarregarCriancasDoCliente(cliente);
            IsInEditMode = false;
            ClienteExistenteDetectado = false;
            NotifyAll();
            CarregarClientesDoBanco();
            LimparCamposClienteCrianca();
        }
        [RelayCommand]
        private void ExcluirClienteOuCriancaSelecionado()
        {
            var selecionado = ClienteCriancaSelecionado; // salva local
            if (selecionado is null)
                return;
            var cliente = ListaClientes.FirstOrDefault(c => c.Id == ClienteCriancaSelecionado.ClienteId);

            if (cliente is null)
                return;


            if (MessageBox.Show($"Deseja excluir o cliente '{cliente.Nome}' e todas as crianças vinculadas?", "Confirmação", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                var agendamentosDoCliente = _db.Agendamentos
                    .Where(a => a.ClienteId == cliente.Id)
                    .ToList();
                if (agendamentosDoCliente.Any())
                {
                    MessageBox.Show(
                        $"O cliente '{cliente.Nome}' possui agendamentos vinculados.\n" +
                        "Remova os agendamentos antes de excluir o cliente.",
                        "Não é possível excluir",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
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
                _db.Clientes.Remove(cliente);
                _db.SaveChanges();

                // 8) Atualiza suas collections
                ListaClientes.Remove(cliente);
                ListaCriancasDoCliente.Clear();
                AtualizarListaClienteCrianca();
                if (selecionado.CriancaId is int criancaId)
                {
                    var crianca = cliente.Criancas.FirstOrDefault(c => c.Id == criancaId);
                    if (crianca is not null)
                    {
                        cliente.Criancas.Remove(crianca);

                        var criancaDb = _db.Criancas.FirstOrDefault(c => c.Id == criancaId);
                        if (criancaDb is not null)
                            _db.Criancas.Remove(criancaDb);

                        var itemRemover = ListaClienteCrianca.FirstOrDefault(x => x.CriancaId == criancaId);
                        if (itemRemover is not null)
                            ListaClienteCrianca.Remove(itemRemover);
                        CriancaSelecionada = null;
                    }
                }
                _db.SaveChanges();
                AtualizarListaClienteCrianca();
                IsInEditMode = false;
                ClienteExistenteDetectado = false;
                NotifyAll();
                CarregarClientesDoBanco();
                return;

            }
        }
        partial void OnPesquisaTextChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                ListaClienteCrianca = new ObservableCollection<ClienteCriancaView>(_todosClientes);
                return;
            }

            var filtro = value.Trim().ToLower();

            var filtrados = _todosClientes.Where(c =>
                (!string.IsNullOrEmpty(c.NomeCliente) && c.NomeCliente.ToLower().Contains(filtro)) ||
                (!string.IsNullOrEmpty(c.Telefone) && c.Telefone.ToLower().Contains(filtro)) ||
                (!string.IsNullOrEmpty(c.NomeCrianca) && c.NomeCrianca.ToLower().Contains(filtro))
            );

            ListaClienteCrianca = new ObservableCollection<ClienteCriancaView>(filtrados);
        }

    }

}
