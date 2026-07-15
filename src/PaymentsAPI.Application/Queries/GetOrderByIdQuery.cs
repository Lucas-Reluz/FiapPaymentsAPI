using MediatR;
using PaymentsAPI.Application.DTOs;

namespace PaymentsAPI.Application.Queries;

public record GetOrderByIdQuery(Guid OrderId) : IRequest<OrderResponse?>;
