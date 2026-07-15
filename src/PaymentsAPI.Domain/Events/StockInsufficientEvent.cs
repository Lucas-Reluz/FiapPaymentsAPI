namespace PaymentsAPI.Domain.Events;

/// <summary>
/// Evento consumido do CatalogAPI quando não há estoque suficiente
/// </summary>
public class StockInsufficientEvent
{
    public Guid OrderId { get; set; }
    public Guid GameId { get; set; }
    public string GameTitle { get; set; } = string.Empty;
    public int RequestedQuantity { get; set; }
    public int AvailableStock { get; set; }
    public DateTime OccurredAt { get; set; }
}
