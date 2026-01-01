using FCG.Jogos.Application.Messaging.Events;
using FCG.Jogos.Application.Messaging.Interfaces;
using FCG.Jogos.Application.Jogos.Interfaces;
using Microsoft.Extensions.Logging;

namespace FCG.Jogos.Application.EventHandlers;

public class PagamentoAprovadoEventHandler : IEventHandler<PagamentoAprovadoEvent>
{
    private readonly ILogger<PagamentoAprovadoEventHandler> _logger;
    private readonly ICompraService _compraService;
    private readonly IEventBus _eventBus;

    public PagamentoAprovadoEventHandler(
        ILogger<PagamentoAprovadoEventHandler> logger,
        ICompraService compraService,
        IEventBus eventBus)
    {
        _logger = logger;
        _compraService = compraService;
        _eventBus = eventBus;
    }

    public async Task HandleAsync(PagamentoAprovadoEvent @event)
    {
        _logger.LogInformation(
            "Processing approved payment for User {UsuarioId}, Game {JogoId}, Transaction {TransacaoId}",
            @event.UsuarioId, @event.JogoId, @event.TransacaoId);

        try
        {
            var compraId = await _compraService.RegistrarCompraAsync(
                @event.UsuarioId,
                @event.JogoId,
                @event.TransacaoId,
                @event.Valor);

            var compraRealizadaEvent = new CompraRealizadaEvent
            {
                CompraId = compraId,
                UsuarioId = @event.UsuarioId,
                JogoId = @event.JogoId,
                TransacaoId = @event.TransacaoId,
                ValorPago = @event.Valor,
                DataCompra = DateTime.UtcNow
            };

            await _eventBus.PublishAsync(compraRealizadaEvent);

            _logger.LogInformation(
                "Purchase {CompraId} registered successfully for Transaction {TransacaoId}",
                compraId, @event.TransacaoId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error processing approved payment for Transaction {TransacaoId}",
                @event.TransacaoId);
            throw;
        }
    }
}
