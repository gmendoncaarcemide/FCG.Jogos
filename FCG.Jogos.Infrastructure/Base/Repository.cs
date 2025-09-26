using FCG.Jogos.Domain.Base;
using Microsoft.EntityFrameworkCore;

namespace FCG.Jogos.Infrastructure.Base;

public abstract class Repository<T> : IRepository<T> where T : Entity
{
    protected readonly JogosDbContext _context;
    protected readonly DbSet<T> _dbSet;

    protected Repository(JogosDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> ObterPorIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<IEnumerable<T>> ObterTodosAsync()
    {
        return await _dbSet.Where(e => e.Ativo).ToListAsync();
    }

    public virtual async Task<T> AdicionarAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task<T> AtualizarAsync(T entity)
    {
        entity.DataAtualizacao = DateTimeOffset.UtcNow;
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task<bool> ExcluirAsync(Guid id)
    {
        var entity = await ObterPorIdAsync(id);
        if (entity != null)
        {
            entity.Ativo = false;
            entity.DataAtualizacao = DateTimeOffset.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
        return false;
    }
}