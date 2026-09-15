using MediatR;
using Microsoft.Extensions.Logging;
using PaymentsAPI.Application.Commands;
using PaymentsAPI.Domain.Entities;
using PaymentsAPI.Domain.Events;
using PaymentsAPI.Domain.Interfaces;

namespace PaymentsAPI.Application.Handlers;

public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, bool>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<ProcessPaymentCommandHandler> _logger;

    public ProcessPaymentCommandHandler(
        IOrderRepository orderRepository,
        IEventPublisher eventPublisher,
        ILogger<ProcessPaymentCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<bool> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId);

        if (order == null)
        {
            _logger.LogWarning("Pedido {OrderId} não encontrado para processar pagamento", request.OrderId);
            return false;
        }
        if (order.Status != Domain.Enums.OrderStatus.AwaitingPayment)
        {
            _logger.LogWarning("Pedido {OrderId} não está em status AwaitingPayment. Status atual: {Status}", 
                order.Id, order.Status);
            return false;
        }
        var payment = order.Payment ?? new Payment(order.Id, order.TotalPrice, request.PaymentMethod);

        if (order.Payment == null)
        {
            order.AddPayment(payment);
        }

        _logger.LogInformation("Pagamento {PaymentId} processado para pedido {OrderId}", payment.Id, order.Id);
        var random = new Random();
        var isApproved = random.Next(100) < 80;

        if (isApproved)
        {
            payment.Complete();
            order.Confirm();
            await _orderRepository.UpdateAsync(order);

            _logger.LogInformation("Pagamento {PaymentId} aprovado para pedido {OrderId}", payment.Id, order.Id);
            var orderConfirmedEvent = new OrderConfirmedEvent
            {
                OrderId = order.Id,
                UserId = order.UserId,
                GameId = order.GameId,
                GameTitle = order.GameTitle,
                Quantity = order.Quantity,
                TotalPrice = order.TotalPrice,
                ConfirmedAt = DateTime.UtcNow
            };

            await _eventPublisher.PublishAsync(orderConfirmedEvent, "payment.exchange");

            _logger.LogInformation("Evento OrderConfirmedEvent publicado para pedido {OrderId}", order.Id);
            return true;
        }
        else
        {
            payment.Fail("Pagamento recusado pela operadora");
            order.Cancel();
            await _orderRepository.UpdateAsync(order);

            _logger.LogWarning("Pagamento {PaymentId} recusado para pedido {OrderId}", payment.Id, order.Id);
            var orderCancelledEvent = new OrderCancelledEvent
            {
                OrderId = order.Id,
                UserId = order.UserId,
                GameId = order.GameId,
                GameTitle = order.GameTitle,
                CancellationReason = "Pagamento recusado",
                CancelledAt = DateTime.UtcNow
            };

            await _eventPublisher.PublishAsync(orderCancelledEvent, "payment.exchange");

            _logger.LogInformation("Evento OrderCancelledEvent publicado para pedido {OrderId}", order.Id);
            return false;
        }
    }
}
