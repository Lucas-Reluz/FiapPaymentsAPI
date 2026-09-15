using PaymentsAPI.Domain.Enums;

namespace PaymentsAPI.Domain.Entities;

public class Order
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid GameId { get; private set; }
    public string GameTitle { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal TotalPrice { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public Payment? Payment { get; private set; }
    private Order() { }
    public Order(Guid userId, Guid gameId, string gameTitle, int quantity, decimal unitPrice)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        GameId = gameId;
        GameTitle = gameTitle;
        Quantity = quantity;
        UnitPrice = unitPrice;
        TotalPrice = quantity * unitPrice;
        Status = OrderStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }
    public void ConfirmStock()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException($"Não é possível confirmar estoque do pedido com status {Status}");

        Status = OrderStatus.AwaitingPayment;
        UpdatedAt = DateTime.UtcNow;
    }
    public void Cancel()
    {
        if (Status == OrderStatus.Confirmed)
            throw new InvalidOperationException("Não é possível cancelar pedido já confirmado");

        Status = OrderStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }
    public void Confirm()
    {
        if (Status != OrderStatus.AwaitingPayment)
            throw new InvalidOperationException($"Não é possível confirmar pedido com status {Status}");

        Status = OrderStatus.Confirmed;
        UpdatedAt = DateTime.UtcNow;
    }
    public void AddPayment(Payment payment)
    {
        if (Payment != null)
            throw new InvalidOperationException("Pedido já possui um pagamento associado");

        Payment = payment;
    }
}
