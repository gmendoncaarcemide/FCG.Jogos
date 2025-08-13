using FCG.Jogos.Domain.Jogos.Entities;
using Microsoft.EntityFrameworkCore;

namespace FCG.Jogos.Infrastructure;

public class JogosDbContext : DbContext
{
    public JogosDbContext(DbContextOptions<JogosDbContext> options) : base(options)
    {
    }

    public DbSet<Jogo> Jogos { get; set; }
    public DbSet<Compra> Compras { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Jogo>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Titulo).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Descricao).HasMaxLength(2000);
            entity.Property(e => e.Preco).HasPrecision(18, 2);
            entity.Property(e => e.Tags).HasMaxLength(500);
            entity.Property(e => e.Plataformas).HasMaxLength(200);
            entity.Property(e => e.Categoria).HasMaxLength(100);
            
            entity.HasIndex(e => e.Titulo);
            entity.HasIndex(e => e.Categoria);
        });

        modelBuilder.Entity<Compra>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PrecoPago).HasPrecision(18, 2);
            entity.Property(e => e.CodigoAtivacao).HasMaxLength(50);
            
            entity.HasOne<Jogo>()
                .WithMany()
                .HasForeignKey(e => e.JogoId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
} 