namespace FCG.Jogos.Application.Messaging.Events;

public class CompraRealizadaEvent : IntegrationEvent
{
    public Guid CompraId { get; set; }
    public Guid UsuarioId { get; set; }
    public Guid JogoId { get; set; }
    public Guid TransacaoId { get; set; }
    public decimal ValorPago { get; set; }
    public DateTime DataCompra { get; set; }
}
