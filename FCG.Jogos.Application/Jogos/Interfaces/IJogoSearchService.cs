using FCG.Jogos.Application.Jogos.ViewModels;

namespace FCG.Jogos.Application.Jogos.Interfaces;

public interface IJogoSearchService
{
    Task<IEnumerable<JogoResponse>> SearchAsync(
        string? q,
        string? categoria,
        decimal? precoMin,
        decimal? precoMax,
        string[]? tags,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default);

    Task<IEnumerable<JogoResponse>> SuggestForUserAsync(Guid usuarioId, int quantidade = 10, CancellationToken ct = default);

    Task<PopularMetricsVm> GetPopularMetricsAsync(int top = 10, CancellationToken ct = default);
}

public class PopularMetricsVm
{
    public required IReadOnlyCollection<string> TopTags { get; init; }
    public required IReadOnlyCollection<string> TopPlataformas { get; init; }
    public required IReadOnlyCollection<string> TopCategorias { get; init; }
}
