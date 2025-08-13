using FCG.Jogos.Domain.Jogos.Entities;
using FCG.Jogos.Domain.Jogos.Interfaces;
using FCG.Jogos.Infrastructure.Base;
using Microsoft.EntityFrameworkCore;

namespace FCG.Jogos.Infrastructure.Jogos.Repositories;

public class CompraRepository : Repository<Compra>, ICompraRepository
{
    public CompraRepository(JogosDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Compra>> ObterPorUsuarioAsync(Guid usuarioId)
    {
        return await _dbSet
            .Where(c => c.UsuarioId == usuarioId && c.Ativo)
            .Include(c => c.Jogo)
            .ToListAsync();
    }

    public async Task<IEnumerable<Compra>> ObterPorJogoAsync(Guid jogoId)
    {
        return await _dbSet
            .Where(c => c.JogoId == jogoId && c.Ativo)
            .Include(c => c.Usuario)
            .ToListAsync();
    }

    public async Task<IEnumerable<Compra>> ObterPorStatusAsync(StatusCompra status)
    {
        return await _dbSet
            .Where(c => c.Status == status && c.Ativo)
            .Include(c => c.Jogo)
            .Include(c => c.Usuario)
            .ToListAsync();
    }
} 