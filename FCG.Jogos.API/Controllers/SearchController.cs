using FCG.Jogos.Application.Jogos.Interfaces;
using FCG.Jogos.Application.Jogos.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace FCG.Jogos.API.Controllers;

[ApiController]
[Route("api/search")]
public class SearchController : ControllerBase
{
    private readonly IJogoSearchService _searchService;

    public SearchController(IJogoSearchService searchService)
    {
        _searchService = searchService;
    }

    // GET api/search/jogos?q=...&categoria=...&precoMin=...&precoMax=...&tags=tag1&tags=tag2&page=1&pageSize=20
    [HttpGet("jogos")]
    [ProducesResponseType(typeof(IEnumerable<JogoResponse>), 200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> SearchJogos(
        [FromQuery] string? q,
        [FromQuery] string? categoria,
        [FromQuery] decimal? precoMin,
        [FromQuery] decimal? precoMax,
        [FromQuery] string[]? tags,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        try
        {
            var result = await _searchService.SearchAsync(q, categoria, precoMin, precoMax, tags, page, pageSize, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return Problem(ex.Message);
        }
    }

    // GET api/search/sugestoes/{usuarioId}?quantidade=10
    [HttpGet("sugestoes/{usuarioId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<JogoResponse>), 200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> Sugestoes(Guid usuarioId, [FromQuery] int quantidade = 10, CancellationToken ct = default)
    {
        try
        {
            var result = await _searchService.SuggestForUserAsync(usuarioId, quantidade, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return Problem(ex.Message);
        }
    }

    // GET api/search/metrics/popular?top=10
    [HttpGet("metrics/popular")]
    [ProducesResponseType(typeof(PopularMetricsVm), 200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> PopularMetrics([FromQuery] int top = 10, CancellationToken ct = default)
    {
        try
        {
            var result = await _searchService.GetPopularMetricsAsync(top, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return Problem(ex.Message);
        }
    }
}
