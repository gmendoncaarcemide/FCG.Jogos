using FCG.Jogos.Application.Jogos.Interfaces;
using FCG.Jogos.Application.Jogos.Services;
using FCG.Jogos.Domain.Jogos.Interfaces;
using FCG.Jogos.Domain.Jogos.Entities;
using FCG.Jogos.Infrastructure.Jogos.Repositories;
using FCG.Jogos.Infrastructure;
using FCG.Jogos.Infrastructure.Jogos.Search;
using Microsoft.EntityFrameworkCore;
using FCG.Jogos.Domain.Base;
using FCG.Jogos.Infrastructure.Jogos.EventSourcing;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddHttpClient();

// Database
builder.Services.AddDbContext<JogosDbContext>(options =>
{
    var config = builder.Configuration;
    var useInMemory = config.GetValue<bool>("UseInMemoryDatabase");
    var provider = config.GetValue<string>("DatabaseProvider");

    if (useInMemory || string.Equals(provider, "InMemory", StringComparison.OrdinalIgnoreCase))
    {
        options.UseInMemoryDatabase("JogosDb");
    }
    else
    {
        var conn = config.GetConnectionString("DefaultConnection") ?? string.Empty;
        // Se a connection string não tiver SSL configurado, forçar SSL para compatibilidade com Supabase
        if (!conn.Contains("Ssl Mode", StringComparison.OrdinalIgnoreCase))
        {
            conn = conn.TrimEnd(';') + ";Ssl Mode=Require;Trust Server Certificate=true";
        }
        options.UseNpgsql(conn, npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure();
        });
    }
});

// Elasticsearch
builder.Services.AddElasticsearch(builder.Configuration);

// Repositories
builder.Services.AddScoped<IJogoRepository, JogoRepository>();
builder.Services.AddScoped<ICompraRepository, CompraRepository>();

// Event Store
builder.Services.AddScoped<IEventStore, EventStore>();

// Services
builder.Services.AddScoped<IJogoService, JogoService>();
builder.Services.AddScoped<ICompraService, CompraService>();
builder.Services.AddScoped<IJogoSearchService, JogoSearchService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
    app.UseSwagger();
    app.UseSwaggerUI();
// }

app.UseHttpsRedirection();

// Map controllers
app.MapControllers();

// Redirect root to Swagger UI for convenience
app.MapGet("/", () => Results.Redirect("/swagger"));

// Auto-migrate database (do not block app start if DB is unreachable)
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<JogosDbContext>();
        var cfg = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var useInMemory = cfg.GetValue<bool>("UseInMemoryDatabase") ||
                          string.Equals(cfg.GetValue<string>("DatabaseProvider"), "InMemory", StringComparison.OrdinalIgnoreCase);

        if (useInMemory)
        {
            context.Database.EnsureCreated();
            logger.LogInformation("InMemory database ensured created.");

            // Seed básico para desenvolvimento
            if (!context.Jogos.Any())
            {
                var seed = new List<Jogo>
                {
                    new Jogo
                    {
                        Titulo = "Space Adventure",
                        Descricao = "Jogo de aventura espacial",
                        Desenvolvedor = "Studio A",
                        Editora = "Editora X",
                        DataLancamento = DateTimeOffset.UtcNow.AddDays(-30),
                        Preco = 99.90m,
                        ImagemUrl = null,
                        VideoUrl = null,
                        Tags = new List<string>{"espaco","aventura"},
                        Plataformas = new List<string>{"PC","Xbox"},
                        Categoria = CategoriaJogo.Aventura,
                        Classificacao = ClassificacaoIndicativa.Livre,
                        AvaliacaoMedia = 4,
                        NumeroAvaliacoes = 10,
                        Disponivel = true,
                        Estoque = 100
                    },
                    new Jogo
                    {
                        Titulo = "Racing Pro",
                        Descricao = "Corrida de alta velocidade",
                        Desenvolvedor = "Studio B",
                        Editora = "Editora Y",
                        DataLancamento = DateTimeOffset.UtcNow.AddDays(-10),
                        Preco = 149.90m,
                        ImagemUrl = null,
                        VideoUrl = null,
                        Tags = new List<string>{"corrida","carros"},
                        Plataformas = new List<string>{"PC","PlayStation"},
                        Categoria = CategoriaJogo.Corrida,
                        Classificacao = ClassificacaoIndicativa.Livre,
                        AvaliacaoMedia = 5,
                        NumeroAvaliacoes = 25,
                        Disponivel = true,
                        Estoque = 50
                    }
                };

                context.Jogos.AddRange(seed);
                context.SaveChanges();
                logger.LogInformation("InMemory database seeded with {Count} jogos.", seed.Count);
            }
        }
        else
        {
            context.Database.Migrate();
            logger.LogInformation("Database migration completed successfully.");
        }
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Database migration skipped: unable to connect or apply migrations at startup.");
    }
}

app.Run();