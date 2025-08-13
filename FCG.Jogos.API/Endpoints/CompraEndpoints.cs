using FCG.Jogos.Application.Jogos.Interfaces;
using FCG.Jogos.Application.Jogos.ViewModels;
using FCG.Jogos.Domain.Jogos.Entities;
using Microsoft.AspNetCore.Mvc;

namespace FCG.Jogos.API.Endpoints;

public static class CompraEndpoints
{
    public static void MapCompraEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/compras")
            .WithTags("Compras")
            .WithOpenApi();

        group.MapGet("/", async ([FromServices] ICompraService service) =>
        {
            try
            {
                var compras = await service.ObterTodosAsync();
                return Results.Ok(compras);
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        })
        .WithName("ObterTodasCompras")
        .WithSummary("Lista todas as compras")
        .Produces<IEnumerable<CompraResponse>>(200)
        .ProducesProblem(500);

        group.MapGet("/{id:guid}", async (Guid id, [FromServices] ICompraService service) =>
        {
            try
            {
                var compra = await service.ObterPorIdAsync(id);
                if (compra == null)
                    return Results.NotFound();

                return Results.Ok(compra);
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        })
        .WithName("ObterCompraPorId")
        .WithSummary("Obtém uma compra pelo ID")
        .Produces<CompraResponse>(200)
        .Produces(404)
        .ProducesProblem(500);

        group.MapPost("/", async ([FromBody] CompraRequest request, [FromServices] ICompraService service) =>
        {
            try
            {
                var compra = await service.CriarCompraAsync(request);
                return Results.CreatedAtRoute("ObterCompraPorId", new { id = compra.Id }, compra);
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        })
        .WithName("CriarCompra")
        .WithSummary("Cria uma nova compra")
        .Produces<CompraResponse>(201)
        .ProducesProblem(500);

        group.MapGet("/usuario/{usuarioId:guid}", async (Guid usuarioId, [FromServices] ICompraService service) =>
        {
            try
            {
                var compras = await service.ObterPorUsuarioAsync(usuarioId);
                return Results.Ok(compras);
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        })
        .WithName("ObterComprasPorUsuario")
        .WithSummary("Obtém compras de um usuário específico")
        .Produces<IEnumerable<CompraResponse>>(200)
        .ProducesProblem(500);

        group.MapGet("/jogo/{jogoId:guid}", async (Guid jogoId, [FromServices] ICompraService service) =>
        {
            try
            {
                var compras = await service.ObterPorJogoAsync(jogoId);
                return Results.Ok(compras);
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        })
        .WithName("ObterComprasPorJogo")
        .WithSummary("Obtém compras de um jogo específico")
        .Produces<IEnumerable<CompraResponse>>(200)
        .ProducesProblem(500);

        group.MapPut("/{id:guid}/status", async (Guid id, [FromBody] StatusCompra status, [FromServices] ICompraService service) =>
        {
            try
            {
                var compra = await service.AtualizarStatusAsync(id, status);
                return Results.Ok(compra);
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
        .WithName("AtualizarStatusCompra")
        .WithSummary("Atualiza o status de uma compra")
        .Produces<CompraResponse>(200)
        .Produces(404)
        .ProducesProblem(500);

        group.MapPost("/{id:guid}/cancelar", async (Guid id, [FromServices] ICompraService service) =>
        {
            try
            {
                await service.CancelarCompraAsync(id);
                return Results.NoContent();
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        })
        .WithName("CancelarCompra")
        .WithSummary("Cancela uma compra")
        .Produces(204)
        .ProducesProblem(500);

        group.MapGet("/{id:guid}/codigo-ativacao", async (Guid id, [FromServices] ICompraService service) =>
        {
            try
            {
                var codigo = await service.GerarCodigoAtivacaoAsync(id);
                return Results.Ok(new { CodigoAtivacao = codigo });
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        })
        .WithName("GerarCodigoAtivacao")
        .WithSummary("Gera código de ativação para uma compra")
        .Produces<object>(200)
        .ProducesProblem(500);
    }
}