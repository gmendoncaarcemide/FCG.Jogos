using FCG.Jogos.Domain.Jogos.Entities;

namespace FCG.Jogos.Domain.Jogos.Interfaces;

public interface ICompraRepository
{
    Task<Compra?> ObterPorIdAsync(Guid id);
    Task<IEnumerable<Compra>> ObterTodosAsync();
    Task<IEnumerable<Compra>> ObterPorUsuarioAsync(Guid usuarioId);
    Task<IEnumerable<Compra>> ObterPorJogoAsync(Guid jogoId);
    Task<IEnumerable<Compra>> ObterPorStatusAsync(StatusCompra status);
    Task<Compra> AdicionarAsync(Compra compra);
    Task<Compra> AtualizarAsync(Compra compra);
    Task<bool> ExcluirAsync(Guid id); // Alterado de Task para Task<bool>
} 