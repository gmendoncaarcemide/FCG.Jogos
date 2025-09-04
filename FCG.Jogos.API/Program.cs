using FCG.Jogos.Application.Jogos.Interfaces;
using FCG.Jogos.Application.Jogos.Services;
using FCG.Jogos.Domain.Jogos.Interfaces;
using FCG.Jogos.Infrastructure.Jogos.Repositories;
using FCG.Jogos.Infrastructure;
using FCG.Jogos.API.Endpoints;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<JogosDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories
builder.Services.AddScoped<IJogoRepository, JogoRepository>();
builder.Services.AddScoped<ICompraRepository, CompraRepository>();

// Services
builder.Services.AddScoped<IJogoService, JogoService>();
builder.Services.AddScoped<ICompraService, CompraService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Map endpoints
app.MapJogoEndpoints();
app.MapCompraEndpoints();

// Auto-migrate database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<JogosDbContext>();
    context.Database.Migrate();
}

app.Run(); 