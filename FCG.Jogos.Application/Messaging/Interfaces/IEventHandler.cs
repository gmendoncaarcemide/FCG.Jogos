using FCG.Jogos.Application.Messaging.Events;

namespace FCG.Jogos.Application.Messaging.Interfaces;

public interface IEventHandler<in TEvent> where TEvent : IntegrationEvent
{
    Task HandleAsync(TEvent @event);
}
