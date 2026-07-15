namespace PaymentsAPI.Domain.Events;

/// <summary>
/// Evento publicado quando o pedido é cancelado
/// Consumido pelo NotificationsAPI
/// </summary>
public class OrderCancelledEvent
{
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public Guid GameId { get; set; }
    public string GameTitle { get; set; } = string.Empty;
    public string CancellationReason { get; set; } = string.Empty;
    public DateTime CancelledAt { get; set; }
}
