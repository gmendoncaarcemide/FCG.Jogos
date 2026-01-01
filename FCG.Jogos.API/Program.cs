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
using FCG.Jogos.Application.Messaging.Extensions;
using FCG.Jogos.Application.Messaging.Interfaces;
using FCG.Jogos.Application.Messaging.Events;
using FCG.Jogos.Application.EventHandlers;

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

// RabbitMQ
builder.Services.AddRabbitMQMessaging(builder.Configuration);

builder.Services.AddScoped<IEventHandler<PagamentoAprovadoEvent>, PagamentoAprovadoEventHandler>();
builder.Services.AddScoped<IEventHandler<CompraRealizadaEvent>, CompraRealizadaEventHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
    app.UseSwagger();
    app.UseSwaggerUI();
// }

// app.UseHttpsRedirection(); // Disabled for local development with Docker

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
        logger.LogInformation("Applying database migrations...");
        context.Database.Migrate();
        logger.LogInformation("Database migrations applied successfully.");
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Database migration skipped: unable to connect or apply migrations at startup.");
    }

    // Subscribe to RabbitMQ events
    try
    {
        var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();
        eventBus.Subscribe<PagamentoAprovadoEvent, PagamentoAprovadoEventHandler>();
        eventBus.Subscribe<CompraRealizadaEvent, CompraRealizadaEventHandler>();
        logger.LogInformation("RabbitMQ event subscriptions registered successfully.");
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Failed to register RabbitMQ event subscriptions.");
    }
}

app.Run();