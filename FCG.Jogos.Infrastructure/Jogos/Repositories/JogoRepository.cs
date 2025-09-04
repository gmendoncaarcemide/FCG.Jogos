using FCG.Jogos.Domain.Jogos.Entities;
using FCG.Jogos.Domain.Jogos.Interfaces;
using FCG.Jogos.Infrastructure.Base;
using Microsoft.EntityFrameworkCore;

namespace FCG.Jogos.Infrastructure.Jogos.Repositories;

public class JogoRepository : Repository<Jogo>, IJogoRepository
{
    public JogoRepository(JogosDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Jogo>> BuscarPorTituloAsync(string titulo)
    {
        return await _dbSet
            .Where(j => j.Titulo.Contains(titulo) && j.Ativo)
            .ToListAsync();
    }

    public async Task<IEnumerable<Jogo>> BuscarPorCategoriaAsync(CategoriaJogo categoria)
    {
        return await _dbSet
            .Where(j => j.Categoria == categoria && j.Ativo)
            .ToListAsync();
    }

    public async Task<IEnumerable<Jogo>> BuscarPorPrecoAsync(decimal precoMin, decimal precoMax)
    {
        return await _dbSet
            .Where(j => j.Preco >= precoMin && j.Preco <= precoMax && j.Ativo)
            .ToListAsync();
    }

    public async Task<IEnumerable<Jogo>> ObterJogosPopularesAsync(int quantidade = 10)
    {
        return await _dbSet
            .Where(j => j.Ativo)
            .OrderByDescending(j => j.AvaliacaoMedia)
            .Take(quantidade)
            .ToListAsync();
    }

    public async Task<IEnumerable<Jogo>> ObterJogosRecomendadosAsync(string[] tags, int quantidade = 10)
    {
        return await _dbSet
            .Where(j => j.Ativo && tags.Any(tag => j.Tags.Contains(tag)))
            .OrderByDescending(j => j.AvaliacaoMedia)
            .Take(quantidade)
            .ToListAsync();
    }
} 