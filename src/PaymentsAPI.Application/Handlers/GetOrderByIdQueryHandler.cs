using MediatR;
using PaymentsAPI.Application.DTOs;
using PaymentsAPI.Application.Queries;
using PaymentsAPI.Domain.Interfaces;

namespace PaymentsAPI.Application.Handlers;

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderResponse?>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrderByIdQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<OrderResponse?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId);

        if (order == null)
            return null;

        return new OrderResponse
        {
            Id = order.Id,
            UserId = order.UserId,
            GameId = order.GameId,
            GameTitle = order.GameTitle,
            Quantity = order.Quantity,
            UnitPrice = order.UnitPrice,
            TotalPrice = order.TotalPrice,
            Status = order.Status.ToString(),
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            Payment = order.Payment != null ? new PaymentResponse
            {
                Id = order.Payment.Id,
                OrderId = order.Payment.OrderId,
                Amount = order.Payment.Amount,
                PaymentMethod = order.Payment.PaymentMethod,
                Status = order.Payment.Status.ToString(),
                CreatedAt = order.Payment.CreatedAt,
                ProcessedAt = order.Payment.ProcessedAt,
                FailureReason = order.Payment.FailureReason
            } : null
        };
    }
}
