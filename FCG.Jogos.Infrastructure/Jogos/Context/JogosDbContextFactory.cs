using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FCG.Jogos.Infrastructure
{
    public class JogosDbContextFactory : IDesignTimeDbContextFactory<JogosDbContext>
    {
        public JogosDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<JogosDbContext>();
            optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=FCG_Jogos;Trusted_Connection=true;");

            return new JogosDbContext(optionsBuilder.Options);
        }
    }
}
