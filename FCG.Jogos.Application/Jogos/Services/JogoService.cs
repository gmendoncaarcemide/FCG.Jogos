using FCG.Jogos.Application.Jogos.Interfaces;
using FCG.Jogos.Application.Jogos.ViewModels;
using FCG.Jogos.Domain.Jogos.Entities;
using FCG.Jogos.Domain.Jogos.Interfaces;

namespace FCG.Jogos.Application.Jogos.Services;

public class JogoService : IJogoService
{
    private readonly IJogoRepository _jogoRepository;

    public JogoService(IJogoRepository jogoRepository)
    {
        _jogoRepository = jogoRepository;
    }

    public async Task<JogoResponse> CriarAsync(CriarJogoRequest request)
    {
        var jogo = new Jogo
        {
            Titulo = request.Titulo,
            Descricao = request.Descricao,
            Desenvolvedor = "Desenvolvedor Padrão", // Valor padrão
            Editora = "Editora Padrão", // Valor padrão
            DataLancamento = DateTimeOffset.UtcNow,
            Preco = request.Preco,
            Estoque = request.Estoque,
            Tags = request.Tags,
            Plataformas = request.Plataformas,
            Categoria = request.Categoria,
            AvaliacaoMedia = 0,
            NumeroAvaliacoes = 0
        };

        var jogoCriado = await _jogoRepository.AdicionarAsync(jogo);
        return MapearParaResponse(jogoCriado);
    }

    public async Task<JogoResponse?> ObterPorIdAsync(Guid id)
    {
        var jogo = await _jogoRepository.ObterPorIdAsync(id);
        return jogo != null ? MapearParaResponse(jogo) : null;
    }

    public async Task<IEnumerable<JogoResponse>> ObterTodosAsync()
    {
        var jogos = await _jogoRepository.ObterTodosAsync();
        return jogos.Select(MapearParaResponse);
    }

    public async Task<JogoResponse> AtualizarAsync(Guid id, AtualizarJogoRequest request)
    {
        var jogo = await _jogoRepository.ObterPorIdAsync(id);
        if (jogo == null)
            throw new InvalidOperationException("Jogo não encontrado");

        if (request.Titulo != null) jogo.Titulo = request.Titulo;
        if (request.Descricao != null) jogo.Descricao = request.Descricao;
        if (request.Preco.HasValue) jogo.Preco = request.Preco.Value;
        if (request.Estoque.HasValue) jogo.Estoque = request.Estoque.Value;
        if (request.Tags != null) jogo.Tags = request.Tags;
        if (request.Plataformas != null) jogo.Plataformas = request.Plataformas;
        if (request.Categoria.HasValue) jogo.Categoria = request.Categoria.Value;

        var jogoAtualizado = await _jogoRepository.AtualizarAsync(jogo);
        return MapearParaResponse(jogoAtualizado);
    }

    public async Task ExcluirAsync(Guid id)
    {
        await _jogoRepository.ExcluirAsync(id);
    }

    public async Task<IEnumerable<JogoResponse>> BuscarAsync(BuscarJogosRequest request)
    {
        IEnumerable<Jogo> jogos;

        if (!string.IsNullOrEmpty(request.Titulo))
            jogos = await _jogoRepository.BuscarPorTituloAsync(request.Titulo);
        else if (request.Categoria.HasValue)
            jogos = await _jogoRepository.BuscarPorCategoriaAsync(request.Categoria.Value);
        else if (request.PrecoMin.HasValue && request.PrecoMax.HasValue)
            jogos = await _jogoRepository.BuscarPorPrecoAsync(request.PrecoMin.Value, request.PrecoMax.Value);
        else
            jogos = await _jogoRepository.ObterTodosAsync();

        return jogos.Select(MapearParaResponse);
    }

    public async Task<IEnumerable<JogoResponse>> ObterJogosPopularesAsync(int quantidade = 10)
    {
        var jogos = await _jogoRepository.ObterJogosPopularesAsync(quantidade);
        return jogos.Select(MapearParaResponse);
    }

    public async Task<IEnumerable<JogoResponse>> ObterJogosRecomendadosAsync(string[] tags, int quantidade = 10)
    {
        var jogos = await _jogoRepository.ObterJogosRecomendadosAsync(tags, quantidade);
        return jogos.Select(MapearParaResponse);
    }

    private static JogoResponse MapearParaResponse(Jogo jogo)
    {
        return new JogoResponse
        {
            Id = jogo.Id,
            Titulo = jogo.Titulo,
            Descricao = jogo.Descricao,
            Preco = jogo.Preco,
            Estoque = jogo.Estoque,
            Tags = jogo.Tags,
            Plataformas = jogo.Plataformas,
            Categoria = jogo.Categoria,
            Avaliacao = jogo.AvaliacaoMedia,
            Vendas = 0, // Será calculado dinamicamente
            DataCriacao = jogo.DataCriacao.UtcDateTime,
            DataAtualizacao = jogo.DataAtualizacao?.UtcDateTime
        };
    }
}