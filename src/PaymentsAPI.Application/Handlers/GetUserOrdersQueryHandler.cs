using MediatR;
using PaymentsAPI.Application.DTOs;
using PaymentsAPI.Application.Queries;
using PaymentsAPI.Domain.Interfaces;

namespace PaymentsAPI.Application.Handlers;

public class GetUserOrdersQueryHandler : IRequestHandler<GetUserOrdersQuery, OrdersListResponse>
{
    private readonly IOrderRepository _orderRepository;

    public GetUserOrdersQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<OrdersListResponse> Handle(GetUserOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await _orderRepository.GetByUserIdAsync(request.UserId, request.Page, request.PageSize);
        var totalCount = await _orderRepository.CountByUserIdAsync(request.UserId);

        var orderResponses = orders.Select(order => new OrderResponse
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
        });

        return new OrdersListResponse
        {
            Orders = orderResponses,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
