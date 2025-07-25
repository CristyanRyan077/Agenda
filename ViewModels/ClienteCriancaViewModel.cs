﻿using AgendaNovo.Interfaces;
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
        [ObservableProperty]
        private ObservableCollection<ClienteCriancaView> listaClienteCrianca = new();

        [ObservableProperty] private ClienteCriancaView? clienteCriancaSelecionado;
        [ObservableProperty] private bool clienteExistenteDetectado;
        [ObservableProperty] private bool isInEditMode;
        [ObservableProperty] private string pesquisaText;
        [ObservableProperty] private ObservableCollection<Crianca> listaCriancas = new();
        [ObservableProperty] private ObservableCollection<Cliente> listaClientes = new();
        [ObservableProperty] private ObservableCollection<Crianca> listaCriancasDoCliente = new();
        [ObservableProperty] private Cliente novoCliente = new();
                [ObservableProperty] private Crianca? criancaSelecionada = new();
        private List<ClienteCriancaView> _todosClientes = new();
        public IEnumerable<IdadeUnidade> IdadesUnidadeDisponiveis => Enum.GetValues(typeof(IdadeUnidade)).Cast<IdadeUnidade>();
        public IEnumerable<Genero> GenerosLista => Enum.GetValues(typeof(Genero)).Cast<Genero>();


        private readonly AgendaViewModel _agenda;
        private readonly IClienteService _clienteService;
        private readonly ICriancaService _criancaService;


        public ClienteCriancaViewModel(IClienteService clienteService,
        ICriancaService criancaService)
        {
            _clienteService = clienteService;
            _criancaService = criancaService;
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
                        Genero = Enum.TryParse<Genero>(crianca.Genero, out var g) ? g : Genero.M,
                        Idade = crianca.Idade,
                        IdadeUnidade = Enum.TryParse<IdadeUnidade>(crianca.IdadeUnidade, out var u) ? u : IdadeUnidade.Anos
                    });
                }
                else
                {
                    return new[] {
                    new ClienteCriancaView {
                        ClienteId = cliente.Id,
                        NomeCliente = cliente.Nome,
                        Telefone = cliente.Telefone,
                        Email = cliente.Email
                    }
                };
                }
            }).ToList();
            ListaClientes.Clear();
            foreach (var cli in todos)
                ListaClientes.Add(cli);

            ListaClienteCrianca = new ObservableCollection<ClienteCriancaView>(_todosClientes);
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



     




        public void AtualizarListaClienteCrianca()
        {
            listaClienteCrianca.Clear();

            foreach (var item in _todosClientes)
                listaClienteCrianca.Add(item);
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



            Cliente cliente;
            /*if (!clienteFoiEditado && ListaClientes.Any(c =>
                c.Nome.Equals(NovoCliente.Nome, StringComparison.OrdinalIgnoreCase)
                && c.Id != NovoCliente.Id))
            {
                MessageBox.Show("Já existe um cliente com esse nome.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            } */


            if (NovoCliente.Id != 0)
            {
                cliente = _clienteService.GetById(NovoCliente.Id)!;
                cliente.Nome = NovoCliente.Nome;
                cliente.Telefone = NovoCliente.Telefone;
                cliente.Email = NovoCliente.Email;
                _clienteService.Update(cliente);
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
                cliente = _clienteService.Add(cliente);



            }

            // Verifica se há criança para salvar
            if (!string.IsNullOrWhiteSpace(CriancaSelecionada.Nome))
            {
                var crianca = CriancaSelecionada.Id != 0
                ? _criancaService.GetById(CriancaSelecionada.Id)!
                : new Crianca { ClienteId = cliente.Id };

                crianca.Nome = CriancaSelecionada.Nome;
                crianca.Idade = CriancaSelecionada.Idade;
                crianca.Genero = CriancaSelecionada.Genero;
                crianca.IdadeUnidade = CriancaSelecionada.IdadeUnidade;

                _criancaService.AddOrUpdate(crianca);
            }
           
            CarregarClientesDoBanco();
            AtualizarListaClienteCrianca();

            LimparInputsClienteCrianca();
            NotifyAll();


        }
        private void LimparInputsClienteCrianca()
        {
            NovoCliente.Reset();
            CriancaSelecionada.Reset();
            ClienteExistenteDetectado = false;
            IsInEditMode = false;
            // NÃO limpa ListaCriancas nem ListaCriancasDoCliente
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
                if (MessageBox.Show($"Excluir cliente {clienteCriancaSelecionado.NomeCliente} e crianças?",
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
               
                AtualizarListaClienteCrianca();
                IsInEditMode = false;
                ClienteExistenteDetectado = false;
                NotifyAll();
                CarregarClientesDoBanco();
                LimparCamposClienteCrianca();
                return;

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
