using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AgendaNovo
{
    public class AgendaContextFactory : IDesignTimeDbContextFactory<AgendaContext>
    {
        public AgendaContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AgendaContext>();
            optionsBuilder.UseSqlServer("Data Source=2857AL17;Initial Catalog=AgendaDB;Integrated Security=True;Trust Server Certificate=True;");

            return new AgendaContext(optionsBuilder.Options);
        }
    }
}
