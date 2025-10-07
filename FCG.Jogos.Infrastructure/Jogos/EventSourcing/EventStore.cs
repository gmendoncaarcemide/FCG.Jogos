using System.Text.Json;
using FCG.Jogos.Domain.Base;
using FCG.Jogos.Infrastructure.Jogos.EventSourcing;

namespace FCG.Jogos.Infrastructure.Jogos.EventSourcing;

public class EventStore : IEventStore
{
    private readonly JogosDbContext _context;

    public EventStore(JogosDbContext context)
    {
        _context = context;
    }

    public async Task AppendAsync(string aggregateType, Guid aggregateId, string eventType, object data, string? correlationId = null, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var evt = new StoredEvent
        {
            AggregateType = aggregateType,
            AggregateId = aggregateId,
            EventType = eventType,
            Data = json,
            OccurredOn = DateTimeOffset.UtcNow,
            CorrelationId = correlationId
        };
        _context.StoredEvents.Add(evt);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
