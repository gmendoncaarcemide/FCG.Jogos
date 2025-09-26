using FCG.Jogos.Domain.Base;

namespace FCG.Jogos.Domain.Jogos.Entities;

public class Jogo : Entity
{
    public required string Titulo { get; set; }
    public required string Descricao { get; set; }
    public required string Desenvolvedor { get; set; }
    public required string Editora { get; set; }
    public DateTimeOffset DataLancamento { get; set; }
    public decimal Preco { get; set; }
    public string? ImagemUrl { get; set; }
    public string? VideoUrl { get; set; }
    public List<string> Tags { get; set; } = new();
    public List<string> Plataformas { get; set; } = new();
    public CategoriaJogo Categoria { get; set; }
    public ClassificacaoIndicativa Classificacao { get; set; }
    public int AvaliacaoMedia { get; set; }
    public int NumeroAvaliacoes { get; set; }
    public bool Disponivel { get; set; } = true;
    public int Estoque { get; set; }
    
    // Propriedades de navegação
    public virtual ICollection<Compra> Vendas { get; set; } = new List<Compra>();
    public virtual ICollection<Avaliacao> Avaliacoes { get; set; } = new List<Avaliacao>();
}

public enum CategoriaJogo
{
    Acao = 1,
    Aventura = 2,
    Estrategia = 3,
    RPG = 4,
    Esporte = 5,
    Corrida = 6,
    Puzzle = 7,
    Simulacao = 8,
    Terror = 9,
    Outros = 10
}

public enum ClassificacaoIndicativa
{
    Desconhecida = 0,
    Livre = 1,
    Maior10 = 2,
    Maior12 = 3,
    Maior14 = 4,
    Maior16 = 5,
    Maior18 = 6
}

// Classe Avaliacao para referência
public class Avaliacao
{
    public Guid Id { get; set; }
    public Guid JogoId { get; set; }
    public Guid UsuarioId { get; set; }
    public int Nota { get; set; }
    public string? Comentario { get; set; }
    public DateTime DataAvaliacao { get; set; }
} 