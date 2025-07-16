using AgendaNovo.Migrations;
using AgendaNovo.Models;
using AgendaNovo.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        private readonly AgendaViewModel _agenda;
        private readonly AgendaContext _db;

        public ClienteCriancaViewModel(AgendaViewModel agenda)
        {
            _agenda = agenda;
            _db = agenda.DbContext;

            AtualizarListaClienteCrianca();
            LimparCamposClienteCrianca();

        }

    

        // 🔁 Compartilhamento de propriedades da AgendaViewModel
        public Cliente NovoCliente => _agenda.NovoCliente;
        public ObservableCollection<Cliente> ListaClientes => _agenda.ListaClientes;

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
                            Genero = crianca.Genero,
                            Idade = $"{crianca.Idade} {crianca.IdadeUnidade}"
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

            var cliente = ListaClientes.FirstOrDefault(c => c.Id == ClienteCriancaSelecionado.ClienteId);
            if (cliente is null)
                return;

            NovoCliente.Id = cliente.Id;
            NovoCliente.Nome = cliente.Nome;
            NovoCliente.Telefone = cliente.Telefone;
            NovoCliente.Email = cliente.Email;

            ListaCriancasDoCliente.Clear();
            foreach (var c in cliente.Criancas)
            {
                ListaCriancasDoCliente.Add(c);
            }

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
            OnPropertyChanged(nameof(NovoCliente));
            OnPropertyChanged(nameof(ClienteCriancaSelecionado));
            OnPropertyChanged(nameof(ListaClienteCrianca));
            OnPropertyChanged(nameof(ListaCriancasDoCliente));
            OnPropertyChanged(nameof(ListaCriancas));
        }
        [RelayCommand]
        private void SalvarClienteCrianca()
        {
            if (string.IsNullOrWhiteSpace(NovoCliente.Nome))
                return;
            Cliente cliente;

            var clienteFoiEditado = NovoCliente.Id != 0 && ListaClientes.Any(c => c.Id == NovoCliente.Id);

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
                        ClienteId = cliente.Id
                    };
                    cliente.Criancas.Add(crianca);
                    ListaCriancas.Add(crianca);
                    _db.Criancas.Add(crianca);
                }
                else
                {
                    crianca.Nome = CriancaSelecionada.Nome;
                    crianca.Idade = CriancaSelecionada.Idade;
                    crianca.Genero = CriancaSelecionada.Genero;
                    crianca.IdadeUnidade = CriancaSelecionada.IdadeUnidade;

                }
            }

            _db.SaveChanges();
            _db.Entry(cliente).Collection(c => c.Criancas).Load();
            AtualizarListaClienteCrianca();
            LimparCamposClienteCrianca();
        }
        [RelayCommand]
        private void ExcluirClienteOuCriancaSelecionado()
        {
            if (ClienteCriancaSelecionado is null)
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

                if (ClienteCriancaSelecionado.CriancaId is int criancaId)
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
                _db.Clientes.Remove(cliente);
                ListaClientes.Remove(cliente);
                _db.SaveChanges();
                AtualizarListaClienteCrianca();
                return;
            }
        }

    }
}
