using MediatR;

namespace PaymentsAPI.Application.Commands;

public record ProcessPaymentCommand(
    Guid OrderId,
    string PaymentMethod
) : IRequest<bool>;
