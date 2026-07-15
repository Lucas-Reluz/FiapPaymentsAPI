using MediatR;
using Microsoft.Extensions.Logging;
using PaymentsAPI.Application.Commands;
using PaymentsAPI.Application.DTOs;
using PaymentsAPI.Domain.Entities;
using PaymentsAPI.Domain.Events;
using PaymentsAPI.Domain.Interfaces;

namespace PaymentsAPI.Application.Handlers;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<CreateOrderCommandHandler> _logger;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        IEventPublisher eventPublisher,
        ILogger<CreateOrderCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<OrderResponse> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // Criar pedido
        var order = new Order(
            request.UserId,
            request.GameId,
            request.GameTitle,
            request.Quantity,
            request.UnitPrice
        );

        await _orderRepository.AddAsync(order);

        _logger.LogInformation("Pedido {OrderId} criado para usuário {UserId}", order.Id, order.UserId);

        // Publicar evento OrderCreatedEvent para o CatalogAPI
        var orderCreatedEvent = new OrderCreatedEvent
        {
            OrderId = order.Id,
            UserId = order.UserId,
            GameId = order.GameId,
            Quantity = order.Quantity,
            CreatedAt = order.CreatedAt
        };

        await _eventPublisher.PublishAsync(orderCreatedEvent, "order.exchange");

        _logger.LogInformation("Evento OrderCreatedEvent publicado para pedido {OrderId}", order.Id);

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
            UpdatedAt = order.UpdatedAt
        };
    }
}
