using FCG.Jogos.Application.Jogos.Interfaces;
using FCG.Jogos.Application.Jogos.ViewModels;
using FCG.Jogos.Domain.Jogos.Entities;
using Microsoft.AspNetCore.Mvc;

namespace FCG.Jogos.API.Controllers;

[ApiController]
[Route("api/compras")]
public class CompraController : ControllerBase
{
    private readonly ICompraService _service;

    public CompraController(ICompraService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CompraResponse>), 200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var compras = await _service.ObterTodosAsync();
            return Ok(compras);
        }
        catch (Exception ex)
        {
            return Problem(ex.Message);
        }
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CompraResponse), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var compra = await _service.ObterPorIdAsync(id);
            if (compra == null) return NotFound();
            return Ok(compra);
        }
        catch (Exception ex)
        {
            return Problem(ex.Message);
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(CompraResponse), 201)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> Create([FromBody] CompraRequest request)
    {
        try
        {
            var compra = await _service.CriarCompraAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = compra.Id }, compra);
        }
        catch (Exception ex)
        {
            return Problem(ex.Message);
        }
    }

    [HttpGet("usuario/{usuarioId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<CompraResponse>), 200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetByUsuario(Guid usuarioId)
    {
        try
        {
            var compras = await _service.ObterPorUsuarioAsync(usuarioId);
            return Ok(compras);
        }
        catch (Exception ex)
        {
            return Problem(ex.Message);
        }
    }

    [HttpGet("jogo/{jogoId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<CompraResponse>), 200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetByJogo(Guid jogoId)
    {
        try
        {
            var compras = await _service.ObterPorJogoAsync(jogoId);
            return Ok(compras);
        }
        catch (Exception ex)
        {
            return Problem(ex.Message);
        }
    }

    [HttpPut("{id:guid}/status")]
    [ProducesResponseType(typeof(CompraResponse), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> AtualizarStatus(Guid id, [FromBody] StatusCompra status)
    {
        try
        {
            var compra = await _service.AtualizarStatusAsync(id, status);
            return Ok(compra);
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

    [HttpPost("{id:guid}/cancelar")]
    [ProducesResponseType(204)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> Cancelar(Guid id)
    {
        try
        {
            await _service.CancelarCompraAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return Problem(ex.Message);
        }
    }

    [HttpGet("{id:guid}/codigo-ativacao")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GerarCodigoAtivacao(Guid id)
    {
        try
        {
            var codigo = await _service.GerarCodigoAtivacaoAsync(id);
            return Ok(new { CodigoAtivacao = codigo });
        }
        catch (Exception ex)
        {
            return Problem(ex.Message);
        }
    }
}
