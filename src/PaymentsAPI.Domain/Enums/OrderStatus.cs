namespace PaymentsAPI.Domain.Enums;

public enum OrderStatus
{
    Pending = 0,
    AwaitingPayment = 1,
    Confirmed = 2,
    Cancelled = 3
}
