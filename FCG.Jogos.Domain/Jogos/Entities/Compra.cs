using FCG.Jogos.Domain.Base;
using System;

namespace FCG.Jogos.Domain.Jogos.Entities;

public class Compra : Entity
{
    public Guid UsuarioId { get; set; }
    public Guid JogoId { get; set; }
    public decimal PrecoPago { get; set; }
    public DateTimeOffset DataCompra { get; set; }
    public StatusCompra Status { get; set; }
    public string? CodigoAtivacao { get; set; }
    public DateTimeOffset? DataAtivacao { get; set; }
    public string? Observacoes { get; set; }
    
    // Propriedades de navegação
    public virtual Jogo Jogo { get; set; } = null!;
    public virtual Usuario Usuario { get; set; } = null!;
}

public enum StatusCompra
{
    Pendente = 1,
    Aprovada = 2,
    Cancelada = 3,
    Reembolsada = 4,
    Processando = 5,
    Ativada = 6
}

// Classe Usuario para referência
public class Usuario
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
} 