using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FCG.Jogos.Infrastructure
{
    public class JogosDbContextFactory : IDesignTimeDbContextFactory<JogosDbContext>
    {
        public JogosDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<JogosDbContext>();
            // Usa o mesmo provider do runtime (PostgreSQL)
            optionsBuilder.UseNpgsql("Host=aws-1-us-east-2.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.elcvczlnnzbgcpsbowkg;Password=Fiap@1234");

            return new JogosDbContext(optionsBuilder.Options);
        }
    }
}
