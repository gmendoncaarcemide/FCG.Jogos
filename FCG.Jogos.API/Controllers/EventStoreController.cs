using System.ComponentModel.DataAnnotations;
using FCG.Jogos.Infrastructure;
using FCG.Jogos.Infrastructure.Jogos.EventSourcing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FCG.Jogos.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventStoreController : ControllerBase
{
    private readonly JogosDbContext _context;

    public EventStoreController(JogosDbContext context)
    {
        _context = context;
    }

    // GET: /api/eventstore
    [HttpGet]
    public async Task<ActionResult<PagedResult<StoredEvent>>> Get(
        [FromQuery] string? aggregateType,
        [FromQuery] Guid? aggregateId,
        [FromQuery] string? eventType,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery][Range(1, 1000)] int pageSize = 50,
        [FromQuery][Range(1, int.MaxValue)] int page = 1)
    {
        var query = _context.StoredEvents.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(aggregateType))
            query = query.Where(e => e.AggregateType == aggregateType);
        if (aggregateId.HasValue)
            query = query.Where(e => e.AggregateId == aggregateId.Value);
        if (!string.IsNullOrWhiteSpace(eventType))
            query = query.Where(e => e.EventType == eventType);
        if (from.HasValue)
            query = query.Where(e => e.OccurredOn >= from.Value);
        if (to.HasValue)
            query = query.Where(e => e.OccurredOn <= to.Value);

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(e => e.OccurredOn)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var result = new PagedResult<StoredEvent>
        {
            Page = page,
            PageSize = pageSize,
            Total = total,
            Items = items
        };
        return Ok(result);
    }
}

public class PagedResult<T>
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int Total { get; set; }
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
}
