using System.ComponentModel.DataAnnotations;
using FCG.Jogos.Domain.Jogos.Entities;

namespace FCG.Jogos.Application.Jogos.ViewModels;

public class CriarJogoRequest
{
    [Required(ErrorMessage = "Título é obrigatório")]
    [StringLength(200, ErrorMessage = "Título deve ter no máximo 200 caracteres")]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Descrição é obrigatória")]
    [StringLength(2000, ErrorMessage = "Descrição deve ter no máximo 2000 caracteres")]
    public string Descricao { get; set; } = string.Empty;

    [Required(ErrorMessage = "Preço é obrigatório")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Preço deve ser maior que zero")]
    public decimal Preco { get; set; }

    [Required(ErrorMessage = "Estoque é obrigatório")]
    [Range(0, int.MaxValue, ErrorMessage = "Estoque deve ser maior ou igual a zero")]
    public int Estoque { get; set; }

    public List<string> Tags { get; set; } = new();
    public List<string> Plataformas { get; set; } = new();
    public CategoriaJogo Categoria { get; set; }
}

public class AtualizarJogoRequest
{
    [StringLength(200, ErrorMessage = "Título deve ter no máximo 200 caracteres")]
    public string? Titulo { get; set; }

    [StringLength(2000, ErrorMessage = "Descrição deve ter no máximo 2000 caracteres")]
    public string? Descricao { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Preço deve ser maior que zero")]
    public decimal? Preco { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Estoque deve ser maior ou igual a zero")]
    public int? Estoque { get; set; }

    public List<string>? Tags { get; set; }
    public List<string>? Plataformas { get; set; }
    public CategoriaJogo? Categoria { get; set; }
}

public class JogoResponse
{
    public Guid Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public int Estoque { get; set; }
    public List<string> Tags { get; set; } = new();
    public List<string> Plataformas { get; set; } = new();
    public CategoriaJogo Categoria { get; set; }
    public decimal Avaliacao { get; set; }
    public int Vendas { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
}

public class BuscarJogosRequest     
{
    public string? Titulo { get; set; }
    public CategoriaJogo? Categoria { get; set; }
    public decimal? PrecoMin { get; set; }
    public decimal? PrecoMax { get; set; }
}