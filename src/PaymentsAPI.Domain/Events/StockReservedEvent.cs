namespace PaymentsAPI.Domain.Events;
public class StockReservedEvent
{
    public Guid OrderId { get; set; }
    public Guid GameId { get; set; }
    public string GameTitle { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public DateTime ReservedAt { get; set; }
}
