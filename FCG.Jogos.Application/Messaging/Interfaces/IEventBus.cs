using FCG.Jogos.Application.Messaging.Events;

namespace FCG.Jogos.Application.Messaging.Interfaces;

public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent @event, string? routingKey = null) where TEvent : IntegrationEvent;
    void Subscribe<TEvent, THandler>() 
        where TEvent : IntegrationEvent 
        where THandler : IEventHandler<TEvent>;
}
