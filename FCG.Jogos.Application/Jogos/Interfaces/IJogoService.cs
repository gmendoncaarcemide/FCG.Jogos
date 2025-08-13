using FCG.Jogos.Application.Jogos.ViewModels;
using FCG.Jogos.Domain.Jogos.Entities;

namespace FCG.Jogos.Application.Jogos.Interfaces;

public interface IJogoService
{
    Task<JogoResponse> CriarAsync(CriarJogoRequest request);
    Task<JogoResponse?> ObterPorIdAsync(Guid id);
    Task<IEnumerable<JogoResponse>> ObterTodosAsync();
    Task<JogoResponse> AtualizarAsync(Guid id, AtualizarJogoRequest request);
    Task ExcluirAsync(Guid id);
    Task<IEnumerable<JogoResponse>> BuscarAsync(BuscarJogosRequest request);
    Task<IEnumerable<JogoResponse>> ObterJogosPopularesAsync(int quantidade = 10);
    Task<IEnumerable<JogoResponse>> ObterJogosRecomendadosAsync(string[] tags, int quantidade = 10);
} 