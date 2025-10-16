using AgendaNovo.Interfaces;
using AgendaNovo.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgendaNovo.ViewModels
{
    public partial class AdicionarProdutoVM : ObservableObject
    {
        private readonly IProdutoService _produtoService;
        private readonly IPagamentoService _pagamentoService;
        private readonly Func<Task> _onSuccess;

        public int AgendamentoId { get; }
        public ObservableCollection<Produto> Produtos { get; } = new();

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AdicionarCommand))]
        private Produto? produtoSelecionado;
        partial void OnProdutoSelecionadoChanged(Produto? value)
        {
            if (value != null)
                ValorUnitario = value.Valor;
            OnPropertyChanged(nameof(Total));
        }
        public Func<ProdutoLinhaVM, Task> OnAddToCart { get; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AdicionarCommand))]
        private int quantidade = 1;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AdicionarCommand))]
        private decimal valorUnitario;

        partial void OnQuantidadeChanged(int value)
        {
            OnPropertyChanged(nameof(Total));
        }
                

        public decimal Total => Quantidade * ValorUnitario;

        public IRelayCommand AdicionarCommand { get; }
        public IRelayCommand FecharCommand { get; }

        public AdicionarProdutoVM(
            IProdutoService produtoService,
            Func<ProdutoLinhaVM,Task> onAddToCart)
        {
            _produtoService = produtoService;
            OnAddToCart = onAddToCart ?? (_ => Task.CompletedTask);

            AdicionarCommand = new AsyncRelayCommand(AdicionarAsync, PodeAdicionar);
            FecharCommand = new RelayCommand(Fechar);

            CarregarProdutos();
        }

        private void CarregarProdutos()
        {
            Produtos.Clear();
            foreach (var p in _produtoService.GetAll())
                Produtos.Add(p);
        }

        private bool PodeAdicionar() =>
            ProdutoSelecionado != null && Quantidade > 0 && ValorUnitario > 0;

        private async Task AdicionarAsync()
        {
            if (!PodeAdicionar()) return;
            var unit = ValorUnitario > 0 ? ValorUnitario : ProdutoSelecionado.Valor;
            var linha = new ProdutoLinhaVM
            {
                ProdutoId = ProdutoSelecionado.Id,
                Nome = ProdutoSelecionado.Nome,
                Quantidade = Quantidade,
                ValorUnitario = unit
            };

            if (OnAddToCart != null) await OnAddToCart(linha);
        }

        private void Fechar()
        {
            _ = _onSuccess?.Invoke();
        }
        public class ProdutoLinhaVM : ObservableObject
        {
            public int ProdutoId { get; set; }
            public string Nome { get; set; } = "";
            public int Quantidade { get; set; } = 1;
            public decimal ValorUnitario { get; set; }
            public decimal Total => Quantidade * ValorUnitario;
        }
    }
}
