using FCG.Jogos.Domain.Jogos.Entities;
using FCG.Jogos.Domain.Jogos.Interfaces;

namespace FCG.Jogos.Infrastructure.Jogos.Search;

internal class NoOpJogoSearchProvider : IJogoSearchProvider
{
    public Task IndexAsync(Jogo jogo, CancellationToken ct = default) => Task.CompletedTask;
    public Task DeleteAsync(Guid jogoId, CancellationToken ct = default) => Task.CompletedTask;

    public Task<IReadOnlyCollection<Jogo>> SearchAsync(string? query, string? categoria, decimal? precoMin, decimal? precoMax, string[]? tags, int page, int pageSize, CancellationToken ct = default)
        => Task.FromResult((IReadOnlyCollection<Jogo>)Array.Empty<Jogo>());

    public Task<IReadOnlyCollection<Jogo>> SuggestForUserAsync(Guid usuarioId, int quantidade = 10, CancellationToken ct = default)
        => Task.FromResult((IReadOnlyCollection<Jogo>)Array.Empty<Jogo>());

    public Task<PopularMetricsResponse> GetPopularMetricsAsync(int top = 10, CancellationToken ct = default)
        => Task.FromResult(new PopularMetricsResponse
        {
            TopTags = Array.Empty<string>(),
            TopPlataformas = Array.Empty<string>(),
            TopCategorias = Array.Empty<string>()
        });
}
