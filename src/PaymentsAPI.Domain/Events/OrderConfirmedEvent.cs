namespace PaymentsAPI.Domain.Events;

/// <summary>
/// Evento publicado quando o pedido é confirmado (pagamento aprovado)
/// Consumido pelo NotificationsAPI
/// </summary>
public class OrderConfirmedEvent
{
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public Guid GameId { get; set; }
    public string GameTitle { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime ConfirmedAt { get; set; }
}
