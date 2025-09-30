using FCG.Jogos.Domain.Jogos.Entities;

namespace FCG.Jogos.Domain.Jogos.Interfaces;

public interface IJogoSearchProvider
{
    Task IndexAsync(Jogo jogo, CancellationToken ct = default);
    Task DeleteAsync(Guid jogoId, CancellationToken ct = default);

    Task<IReadOnlyCollection<Jogo>> SearchAsync(string? query,
        string? categoria,
        decimal? precoMin,
        decimal? precoMax,
        string[]? tags,
        int page,
        int pageSize,
        CancellationToken ct = default);

    Task<IReadOnlyCollection<Jogo>> SuggestForUserAsync(Guid usuarioId, int quantidade = 10, CancellationToken ct = default);

    Task<PopularMetricsResponse> GetPopularMetricsAsync(int top = 10, CancellationToken ct = default);
}

public class PopularMetricsResponse
{
    public required IReadOnlyCollection<string> TopTags { get; init; }
    public required IReadOnlyCollection<string> TopPlataformas { get; init; }
    public required IReadOnlyCollection<string> TopCategorias { get; init; }
}
