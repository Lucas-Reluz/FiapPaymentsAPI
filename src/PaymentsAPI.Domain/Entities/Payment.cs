using PaymentsAPI.Domain.Enums;

namespace PaymentsAPI.Domain.Entities;

public class Payment
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public decimal Amount { get; private set; }
    public string PaymentMethod { get; private set; } = string.Empty;
    public PaymentStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public string? FailureReason { get; private set; }
    public Order Order { get; private set; } = null!;
    private Payment() { }
    public Payment(Guid orderId, decimal amount, string paymentMethod)
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
        Amount = amount;
        PaymentMethod = paymentMethod;
        Status = PaymentStatus.Processing;
        CreatedAt = DateTime.UtcNow;
    }
    public void Complete()
    {
        if (Status != PaymentStatus.Processing)
            throw new InvalidOperationException($"Não é possível completar pagamento com status {Status}");

        Status = PaymentStatus.Completed;
        ProcessedAt = DateTime.UtcNow;
    }
    public void Fail(string reason)
    {
        if (Status != PaymentStatus.Processing)
            throw new InvalidOperationException($"Não é possível falhar pagamento com status {Status}");

        Status = PaymentStatus.Failed;
        ProcessedAt = DateTime.UtcNow;
        FailureReason = reason;
    }
}
