using System;

namespace FCG.Jogos.Infrastructure.Jogos.EventSourcing;

public class StoredEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string AggregateType { get; set; } = string.Empty;
    public Guid AggregateId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Data { get; set; } = string.Empty; // JSON
    public DateTimeOffset OccurredOn { get; set; } = DateTimeOffset.UtcNow;
    public string? CorrelationId { get; set; }
}
