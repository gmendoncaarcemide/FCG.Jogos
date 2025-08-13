using FCG.Jogos.Domain.Jogos.Entities;

namespace FCG.Jogos.Domain.Jogos.Interfaces;

public interface IJogoRepository
{
    Task<Jogo?> ObterPorIdAsync(Guid id);
    Task<IEnumerable<Jogo>> ObterTodosAsync();
    Task<IEnumerable<Jogo>> BuscarPorTituloAsync(string titulo);
    Task<IEnumerable<Jogo>> BuscarPorCategoriaAsync(CategoriaJogo categoria);
    Task<IEnumerable<Jogo>> BuscarPorPrecoAsync(decimal precoMin, decimal precoMax);
    Task<IEnumerable<Jogo>> ObterJogosPopularesAsync(int quantidade);
    Task<IEnumerable<Jogo>> ObterJogosRecomendadosAsync(string[] tags, int quantidade);
    Task<Jogo> AdicionarAsync(Jogo jogo);
    Task<Jogo> AtualizarAsync(Jogo jogo);
    Task<bool> ExcluirAsync(Guid id); // Alterado de Task para Task<bool>
} 