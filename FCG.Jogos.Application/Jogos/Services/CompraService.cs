using FCG.Jogos.Application.Jogos.Interfaces;
using FCG.Jogos.Application.Jogos.ViewModels;
using FCG.Jogos.Domain.Jogos.Entities;
using FCG.Jogos.Domain.Jogos.Interfaces;

namespace FCG.Jogos.Application.Jogos.Services;

public class CompraService : ICompraService
{
    private readonly ICompraRepository _compraRepository;
    private readonly IJogoRepository _jogoRepository;

    public CompraService(ICompraRepository compraRepository, IJogoRepository jogoRepository)
    {
        _compraRepository = compraRepository;
        _jogoRepository = jogoRepository;
    }

    public async Task<CompraResponse> CriarCompraAsync(CompraRequest request)
    {
        var jogo = await _jogoRepository.ObterPorIdAsync(request.JogoId);
        if (jogo == null)
            throw new InvalidOperationException("Jogo não encontrado");

        if (jogo.Estoque <= 0)
            throw new InvalidOperationException("Jogo sem estoque disponível");

        var compra = new Compra
        {
            UsuarioId = request.UsuarioId,
            JogoId = request.JogoId,
            PrecoPago = request.PrecoPago,
            DataCompra = DateTimeOffset.UtcNow,
            Status = StatusCompra.Pendente
        };

        var compraCriada = await _compraRepository.AdicionarAsync(compra);

        // Atualiza o estoque do jogo
        jogo.Estoque--;
        await _jogoRepository.AtualizarAsync(jogo);

        return MapearParaResponse(compraCriada);
    }

    public async Task<CompraResponse?> ObterPorIdAsync(Guid id)
    {
        var compra = await _compraRepository.ObterPorIdAsync(id);
        return compra != null ? MapearParaResponse(compra) : null;
    }

    public async Task<IEnumerable<CompraResponse>> ObterPorUsuarioAsync(Guid usuarioId)
    {
        var compras = await _compraRepository.ObterPorUsuarioAsync(usuarioId);
        return compras.Select(MapearParaResponse);
    }

    public async Task<IEnumerable<CompraResponse>> ObterPorJogoAsync(Guid jogoId)
    {
        var compras = await _compraRepository.ObterPorJogoAsync(jogoId);
        return compras.Select(MapearParaResponse);
    }

    public async Task<IEnumerable<CompraResponse>> ObterTodosAsync()
    {
        var compras = await _compraRepository.ObterTodosAsync();
        return compras.Select(MapearParaResponse);
    }

    public async Task<CompraResponse> AtualizarStatusAsync(Guid id, StatusCompra status)
    {
        var compra = await _compraRepository.ObterPorIdAsync(id);
        if (compra == null)
            throw new InvalidOperationException("Compra não encontrada");

        compra.Status = status;
        var compraAtualizada = await _compraRepository.AtualizarAsync(compra);
        return MapearParaResponse(compraAtualizada);
    }

    public async Task CancelarCompraAsync(Guid id)
    {
        var compra = await _compraRepository.ObterPorIdAsync(id);
        if (compra == null)
            throw new InvalidOperationException("Compra não encontrada");

        if (compra.Status == StatusCompra.Cancelada)
            throw new InvalidOperationException("Compra já está cancelada");

        compra.Status = StatusCompra.Cancelada;
        await _compraRepository.AtualizarAsync(compra);

        // Restaura o estoque do jogo
        var jogo = await _jogoRepository.ObterPorIdAsync(compra.JogoId);
        if (jogo != null)
        {
            jogo.Estoque++;
            await _jogoRepository.AtualizarAsync(jogo);
        }
    }

    public async Task<string> GerarCodigoAtivacaoAsync(Guid compraId)
    {
        var compra = await _compraRepository.ObterPorIdAsync(compraId);
        if (compra == null)
            throw new InvalidOperationException("Compra não encontrada");

        if (compra.Status != StatusCompra.Aprovada)
            throw new InvalidOperationException("Apenas compras aprovadas podem gerar código de ativação");

        var codigoAtivacao = Guid.NewGuid().ToString("N").Substring(0, 16).ToUpper();
        compra.CodigoAtivacao = codigoAtivacao;
        compra.DataAtivacao = DateTimeOffset.UtcNow;

        await _compraRepository.AtualizarAsync(compra);
        return codigoAtivacao;
    }

    private static CompraResponse MapearParaResponse(Compra compra)
    {
        return new CompraResponse
        {
            Id = compra.Id,
            UsuarioId = compra.UsuarioId,
            JogoId = compra.JogoId,
            PrecoPago = compra.PrecoPago,
            DataCompra = compra.DataCompra.UtcDateTime,
            Status = compra.Status,
            CodigoAtivacao = compra.CodigoAtivacao,
            DataCriacao = compra.DataCriacao.UtcDateTime,
            DataAtualizacao = compra.DataAtualizacao?.UtcDateTime
        };
    }
}