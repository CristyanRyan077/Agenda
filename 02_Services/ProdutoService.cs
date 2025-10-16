using AgendaNovo.Helpers;
using AgendaNovo.Interfaces;
using AgendaNovo.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AgendaNovo.AgendaViewModel;

namespace AgendaNovo.Services
{
    public class ProdutoService :IProdutoService
    {
        private readonly AgendaContext _db;

        public ProdutoService(AgendaContext db) 
        {
            _db = db;
        }

        public List<Produto> GetAll()
        {
            return _db.Produtos.AsNoTracking().ToList();
        }

        public Produto? GetById(int id)
        {
            return _db.Produtos.Find(id);
        }

        public void Add(Produto produto)
        {
            _db.Produtos.Add(produto);
            _db.SaveChanges();
        }

        public void Update(Produto produto)
        {
            _db.Produtos.Update(produto);
            _db.SaveChanges();
        }

        public void Delete(int id)
        {
            var produto = _db.Produtos.Find(id);
            if (produto != null)
            {
                _db.Produtos.Remove(produto);
                _db.SaveChanges();
            }
        }
        
    }
}
