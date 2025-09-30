using FCG.Jogos.Application.Jogos.Interfaces;
using FCG.Jogos.Application.Jogos.ViewModels;
using FCG.Jogos.Domain.Jogos.Entities;
using FCG.Jogos.Domain.Jogos.Interfaces;

namespace FCG.Jogos.Application.Jogos.Services;

public class JogoSearchService : IJogoSearchService
{
    private readonly IJogoSearchProvider _provider;
    private readonly ICompraRepository _compraRepository;

    public JogoSearchService(IJogoSearchProvider provider, ICompraRepository compraRepository)
    {
        _provider = provider;
        _compraRepository = compraRepository;
    }

    public async Task<IEnumerable<JogoResponse>> SearchAsync(string? q, string? categoria, decimal? precoMin, decimal? precoMax, string[]? tags, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        var jogos = await _provider.SearchAsync(q, categoria, precoMin, precoMax, tags, page, pageSize, ct);
        return jogos.Select(Map);
    }

    public async Task<IEnumerable<JogoResponse>> SuggestForUserAsync(Guid usuarioId, int quantidade = 10, CancellationToken ct = default)
    {
        // Tenta inferir preferências do usuário a partir do histórico de compras (tags e categorias)
        var compras = await _compraRepository.ObterPorUsuarioAsync(usuarioId);
        var topTags = compras
            .SelectMany(c => c.Jogo.Tags)
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .GroupBy(t => t)
            .OrderByDescending(g => g.Count())
            .Take(5)
            .Select(g => g.Key)
            .ToArray();

        string? categoria = null;
        var topCategoria = compras
            .Select(c => c.Jogo.Categoria)
            .GroupBy(c => c)
            .OrderByDescending(g => g.Count())
            .Select(g => (int)g.Key)
            .FirstOrDefault();
        if (topCategoria != 0)
            categoria = topCategoria.ToString();

        // Busca jogos alinhados às preferências
        var jogosPreferidos = await _provider.SearchAsync(null, categoria, null, null, topTags, 1, quantidade, ct);
        if (jogosPreferidos.Count > 0)
            return jogosPreferidos.Select(Map);

        // Fallback para sugestão padrão baseada em avaliação
        var jogosFallback = await _provider.SuggestForUserAsync(usuarioId, quantidade, ct);
        return jogosFallback.Select(Map);
    }

    public async Task<PopularMetricsVm> GetPopularMetricsAsync(int top = 10, CancellationToken ct = default)
    {
        var resp = await _provider.GetPopularMetricsAsync(top, ct);
        return new PopularMetricsVm
        {
            TopTags = resp.TopTags,
            TopPlataformas = resp.TopPlataformas,
            TopCategorias = resp.TopCategorias
        };
    }

    private static JogoResponse Map(Jogo jogo)
    {
        return new JogoResponse
        {
            Id = jogo.Id,
            Titulo = jogo.Titulo,
            Descricao = jogo.Descricao,
            Preco = jogo.Preco,
            Estoque = jogo.Estoque,
            Tags = jogo.Tags,
            Plataformas = jogo.Plataformas,
            Categoria = jogo.Categoria,
            Avaliacao = jogo.AvaliacaoMedia,
            Vendas = 0,
            DataCriacao = jogo.DataCriacao.UtcDateTime,
            DataAtualizacao = jogo.DataAtualizacao?.UtcDateTime
        };
    }
}
