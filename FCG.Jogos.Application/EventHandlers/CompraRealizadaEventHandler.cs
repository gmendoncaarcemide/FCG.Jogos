using FCG.Jogos.Application.Messaging.Events;
using FCG.Jogos.Application.Messaging.Interfaces;
using Microsoft.Extensions.Logging;

namespace FCG.Jogos.Application.EventHandlers;

public class CompraRealizadaEventHandler : IEventHandler<CompraRealizadaEvent>
{
    private readonly ILogger<CompraRealizadaEventHandler> _logger;
    private readonly IEventBus _eventBus;

    public CompraRealizadaEventHandler(
        ILogger<CompraRealizadaEventHandler> logger,
        IEventBus eventBus)
    {
        _logger = logger;
        _eventBus = eventBus;
    }

    public async Task HandleAsync(CompraRealizadaEvent @event)
    {
        _logger.LogInformation(
            "Processing CompraRealizadaEvent for Purchase {CompraId}, User {UsuarioId}",
            @event.CompraId, @event.UsuarioId);

        var notificacao = new NotificacaoEvent
        {
            UsuarioId = @event.UsuarioId,
            Titulo = "Compra Realizada com Sucesso",
            Mensagem = $"Sua compra no valor de R$ {@event.ValorPago:F2} foi concluída com sucesso!",
            Tipo = TipoNotificacao.CompraRealizada,
            Metadata = new Dictionary<string, string>
            {
                { "CompraId", @event.CompraId.ToString() },
                { "JogoId", @event.JogoId.ToString() },
                { "TransacaoId", @event.TransacaoId.ToString() }
            }
        };

        await _eventBus.PublishAsync(notificacao);

        _logger.LogInformation(
            "Notification sent for completed purchase {CompraId}",
            @event.CompraId);
    }
}
