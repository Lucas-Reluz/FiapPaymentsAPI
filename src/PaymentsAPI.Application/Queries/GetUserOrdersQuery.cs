using MediatR;
using PaymentsAPI.Application.DTOs;

namespace PaymentsAPI.Application.Queries;

public record GetUserOrdersQuery(
    Guid UserId,
    int Page = 1,
    int PageSize = 20
) : IRequest<OrdersListResponse>;
