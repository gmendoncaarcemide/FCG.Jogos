using System.ComponentModel.DataAnnotations;
using FCG.Jogos.Domain.Jogos.Entities;

namespace FCG.Jogos.Application.Jogos.ViewModels;

public class CompraRequest
{
    [Required(ErrorMessage = "ID do usuário é obrigatório")]
    public Guid UsuarioId { get; set; }

    [Required(ErrorMessage = "ID do jogo é obrigatório")]
    public Guid JogoId { get; set; }

    [Required(ErrorMessage = "Preço é obrigatório")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Preço deve ser maior que zero")]
    public decimal PrecoPago { get; set; }

    public string? Observacoes { get; set; }
}

public class CompraResponse
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public Guid JogoId { get; set; }
    public decimal PrecoPago { get; set; }
    public DateTime DataCompra { get; set; }
    public StatusCompra Status { get; set; }
    public string? CodigoAtivacao { get; set; }
    public DateTime? DataAtivacao { get; set; }
    public string? Observacoes { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
} 