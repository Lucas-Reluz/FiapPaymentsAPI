namespace PaymentsAPI.Domain.Events;
public class OrderCreatedEvent
{
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public Guid GameId { get; set; }
    public int Quantity { get; set; }
    public DateTime CreatedAt { get; set; }
}
