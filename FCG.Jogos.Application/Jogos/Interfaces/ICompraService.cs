using FCG.Jogos.Application.Jogos.ViewModels;
using FCG.Jogos.Domain.Jogos.Entities;

namespace FCG.Jogos.Application.Jogos.Interfaces;

public interface ICompraService
{
    Task<CompraResponse> CriarCompraAsync(CompraRequest request);
    Task<CompraResponse?> ObterPorIdAsync(Guid id);
    Task<IEnumerable<CompraResponse>> ObterTodosAsync();
    Task<IEnumerable<CompraResponse>> ObterPorUsuarioAsync(Guid usuarioId);
    Task<IEnumerable<CompraResponse>> ObterPorJogoAsync(Guid jogoId);
    Task<CompraResponse> AtualizarStatusAsync(Guid id, StatusCompra status);
    Task CancelarCompraAsync(Guid id);
    Task<string> GerarCodigoAtivacaoAsync(Guid compraId);
    Task<Guid> RegistrarCompraAsync(Guid usuarioId, Guid jogoId, Guid transacaoId, decimal valorPago);
}
