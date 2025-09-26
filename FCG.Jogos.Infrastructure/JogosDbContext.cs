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
            // Map as PostgreSQL arrays
            entity.Property(e => e.Tags).HasColumnType("text[]");
            entity.Property(e => e.Plataformas).HasColumnType("text[]");
            entity.Property(e => e.Categoria).HasConversion<int>();
            entity.Property(e => e.Classificacao).HasConversion<int>();
            // Date/time columns as timestamptz
            entity.Property(e => e.DataLancamento).HasColumnType("timestamptz");
            entity.Property(e => e.DataCriacao).HasColumnType("timestamptz");
            entity.Property(e => e.DataAtualizacao).HasColumnType("timestamptz");
            
            entity.HasIndex(e => e.Titulo);
            entity.HasIndex(e => e.Categoria);
        });

        modelBuilder.Entity<Compra>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PrecoPago).HasPrecision(18, 2);
            entity.Property(e => e.CodigoAtivacao).HasMaxLength(50);
            // Date/time columns as timestamptz
            entity.Property(e => e.DataCompra).HasColumnType("timestamptz");
            entity.Property(e => e.DataAtivacao).HasColumnType("timestamptz");

            // Correctly tie FK to the Jogo.Vendas navigation to avoid duplicate relationship
            entity.HasOne(e => e.Jogo)
                .WithMany(j => j.Vendas)
                .HasForeignKey(e => e.JogoId)
                .OnDelete(DeleteBehavior.Restrict);

            // We don't have an entity for Usuario; keep scalar UsuarioId but ignore navigation
            entity.Ignore(e => e.Usuario);
        });
    }
}