using FCG.Jogos.Application.Jogos.Interfaces;
using FCG.Jogos.Application.Jogos.ViewModels;
using FCG.Jogos.Domain.Jogos.Entities;
using Microsoft.AspNetCore.Mvc;

namespace FCG.Jogos.API.Endpoints;

public static class JogoEndpoints
{
    public static void MapJogoEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/jogos")
            .WithTags("Jogos")
            .WithOpenApi();

        group.MapGet("/", async ([FromServices] IJogoService service) =>
        {
            try
            {
                var jogos = await service.ObterTodosAsync();
                return Results.Ok(jogos);
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        })
        .WithName("ObterTodosJogos")
        .WithSummary("Lista todos os jogos")
        .Produces<IEnumerable<JogoResponse>>(200)
        .ProducesProblem(500);

        group.MapGet("/{id:guid}", async (Guid id, [FromServices] IJogoService service) =>
        {
            try
            {
                var jogo = await service.ObterPorIdAsync(id);
                if (jogo == null)
                    return Results.NotFound();

                return Results.Ok(jogo);
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        })
        .WithName("ObterJogoPorId")
        .WithSummary("Obtém um jogo pelo ID")
        .Produces<JogoResponse>(200)
        .Produces(404)
        .ProducesProblem(500);

        group.MapPost("/", async ([FromBody] CriarJogoRequest request, [FromServices] IJogoService service) =>
        {
            try
            {
                var jogo = await service.CriarAsync(request);
                return Results.CreatedAtRoute("ObterJogoPorId", new { id = jogo.Id }, jogo);
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        })
        .WithName("CriarJogo")
        .WithSummary("Cria um novo jogo")
        .Produces<JogoResponse>(201)
        .ProducesProblem(500);

        group.MapPut("/{id:guid}", async (Guid id, [FromBody] AtualizarJogoRequest request, [FromServices] IJogoService service) =>
        {
            try
            {
                var jogo = await service.AtualizarAsync(id, request);
                return Results.Ok(jogo);
            }
            catch (InvalidOperationException ex)
            {
                return Results.NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        })
        .WithName("AtualizarJogo")
        .WithSummary("Atualiza um jogo existente")
        .Produces<JogoResponse>(200)
        .Produces(404)
        .ProducesProblem(500);

        group.MapDelete("/{id:guid}", async (Guid id, [FromServices] IJogoService service) =>
        {
            try
            {
                await service.ExcluirAsync(id);
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        })
        .WithName("ExcluirJogo")
        .WithSummary("Exclui um jogo")
        .Produces(204)
        .ProducesProblem(500);

        group.MapGet("/buscar", async ([FromQuery] string? titulo, [FromQuery] CategoriaJogo? categoria, 
            [FromQuery] decimal? precoMin, [FromQuery] decimal? precoMax, [FromServices] IJogoService service) =>
        {
            try
            {
                var request = new BuscarJogosRequest
                {
                    Titulo = titulo,
                    Categoria = categoria,
                    PrecoMin = precoMin,
                    PrecoMax = precoMax
                };

                var jogos = await service.BuscarAsync(request);
                return Results.Ok(jogos);
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        })
        .WithName("BuscarJogos")
        .WithSummary("Busca jogos por critérios")
        .Produces<IEnumerable<JogoResponse>>(200)
        .ProducesProblem(500);

        group.MapGet("/populares", async ([FromServices] IJogoService service, [FromQuery] int quantidade = 10) =>
        {
            try
            {
                var jogos = await service.ObterJogosPopularesAsync(quantidade);
                return Results.Ok(jogos);
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        })
        .WithName("ObterJogosPopulares")
        .WithSummary("Obtém os jogos mais populares")
        .Produces<IEnumerable<JogoResponse>>(200)
        .ProducesProblem(500);

        group.MapGet("/recomendados", async ([FromQuery] string[] tags, [FromServices] IJogoService service, [FromQuery] int quantidade = 10) =>
        {
            try
            {
                var jogos = await service.ObterJogosRecomendadosAsync(tags, quantidade);
                return Results.Ok(jogos);
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        })
        .WithName("ObterJogosRecomendados")
        .WithSummary("Obtém jogos recomendados baseado em tags")
        .Produces<IEnumerable<JogoResponse>>(200)
        .ProducesProblem(500);
    }
} 