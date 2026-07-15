using MediatR;
using PaymentsAPI.Application.DTOs;

namespace PaymentsAPI.Application.Commands;

public record CreateOrderCommand(
    Guid UserId,
    Guid GameId,
    string GameTitle,
    int Quantity,
    decimal UnitPrice
) : IRequest<OrderResponse>;
