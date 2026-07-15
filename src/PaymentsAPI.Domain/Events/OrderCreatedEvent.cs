namespace PaymentsAPI.Domain.Events;

/// <summary>
/// Evento publicado quando um novo pedido é criado
/// Consumido pelo CatalogAPI para reservar estoque
/// </summary>
public class OrderCreatedEvent
{
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public Guid GameId { get; set; }
    public int Quantity { get; set; }
    public DateTime CreatedAt { get; set; }
}
