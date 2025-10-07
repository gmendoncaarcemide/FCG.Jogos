namespace FCG.Jogos.Domain.Base;

public interface IEventStore
{
    Task AppendAsync(string aggregateType, Guid aggregateId, string eventType, object data, string? correlationId = null, CancellationToken cancellationToken = default);
}
