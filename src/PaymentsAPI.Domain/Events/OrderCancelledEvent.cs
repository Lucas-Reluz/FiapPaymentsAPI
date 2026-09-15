namespace PaymentsAPI.Domain.Events;
public class OrderCancelledEvent
{
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public Guid GameId { get; set; }
    public string GameTitle { get; set; } = string.Empty;
    public string CancellationReason { get; set; } = string.Empty;
    public DateTime CancelledAt { get; set; }
}
