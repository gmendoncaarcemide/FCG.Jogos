using FCG.Jogos.Application.Jogos.Interfaces;
using FCG.Jogos.Application.Jogos.ViewModels;
using FCG.Jogos.Domain.Jogos.Entities;
using Microsoft.AspNetCore.Mvc;

namespace FCG.Jogos.API.Controllers;

[ApiController]
[Route("api/jogos")]
public class JogoController : ControllerBase
{
    private readonly IJogoService _service;

    public JogoController(IJogoService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<JogoResponse>), 200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var jogos = await _service.ObterTodosAsync();
            return Ok(jogos);
        }
        catch (Exception ex)
        {
            return Problem(ex.Message);
        }
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(JogoResponse), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var jogo = await _service.ObterPorIdAsync(id);
            if (jogo == null) return NotFound();
            return Ok(jogo);
        }
        catch (Exception ex)
        {
            return Problem(ex.Message);
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(JogoResponse), 201)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> Create([FromBody] CriarJogoRequest request)
    {
        try
        {
            var jogo = await _service.CriarAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = jogo.Id }, jogo);
        }
        catch (Exception ex)
        {
            return Problem(ex.Message);
        }
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(JogoResponse), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> Update(Guid id, [FromBody] AtualizarJogoRequest request)
    {
        try
        {
            var jogo = await _service.AtualizarAsync(id, request);
            return Ok(jogo);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return Problem(ex.Message);
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _service.ExcluirAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return Problem(ex.Message);
        }
    }

    [HttpGet("buscar")]
    [ProducesResponseType(typeof(IEnumerable<JogoResponse>), 200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> Buscar([FromQuery] string? titulo, [FromQuery] CategoriaJogo? categoria,
        [FromQuery] decimal? precoMin, [FromQuery] decimal? precoMax)
    {
        try
        {
            var request = new BuscarJogosRequest
            {
                Titulo = titulo,
                Categoria = categoria,
                PrecoMin = precoMin,
                PrecoMax = precoMax
            };
            var jogos = await _service.BuscarAsync(request);
            return Ok(jogos);
        }
        catch (Exception ex)
        {
            return Problem(ex.Message);
        }
    }

    [HttpGet("populares")]
    [ProducesResponseType(typeof(IEnumerable<JogoResponse>), 200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> Populares([FromQuery] int quantidade = 10)
    {
        try
        {
            var jogos = await _service.ObterJogosPopularesAsync(quantidade);
            return Ok(jogos);
        }
        catch (Exception ex)
        {
            return Problem(ex.Message);
        }
    }

    [HttpGet("recomendados")]
    [ProducesResponseType(typeof(IEnumerable<JogoResponse>), 200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> Recomendados([FromQuery] string[] tags, [FromQuery] int quantidade = 10)
    {
        try
        {
            var jogos = await _service.ObterJogosRecomendadosAsync(tags, quantidade);
            return Ok(jogos);
        }
        catch (Exception ex)
        {
            return Problem(ex.Message);
        }
    }
}
