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
    private readonly IPaymentRepository _paymentRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<ProcessPaymentCommandHandler> _logger;

    public ProcessPaymentCommandHandler(
        IOrderRepository orderRepository,
        IPaymentRepository paymentRepository,
        IEventPublisher eventPublisher,
        ILogger<ProcessPaymentCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _paymentRepository = paymentRepository;
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

        // Verificar se o pedido está aguardando pagamento
        if (order.Status != Domain.Enums.OrderStatus.AwaitingPayment)
        {
            _logger.LogWarning("Pedido {OrderId} não está em status AwaitingPayment. Status atual: {Status}", 
                order.Id, order.Status);
            return false;
        }

        // Verificar se já existe pagamento para este pedido
        var existingPayment = await _paymentRepository.GetByOrderIdAsync(order.Id);
        if (existingPayment != null)
        {
            _logger.LogWarning("Pedido {OrderId} já possui pagamento {PaymentId}", order.Id, existingPayment.Id);
            return false;
        }

        // Criar pagamento
        var payment = new Payment(order.Id, order.TotalPrice, request.PaymentMethod);
        await _paymentRepository.AddAsync(payment);
        order.AddPayment(payment);

        _logger.LogInformation("Pagamento {PaymentId} criado para pedido {OrderId}", payment.Id, order.Id);

        // Simular processamento de pagamento (80% de aprovação)
        var random = new Random();
        var isApproved = random.Next(100) < 80;

        if (isApproved)
        {
            // Pagamento aprovado
            payment.Complete();
            order.Confirm();
            await _paymentRepository.UpdateAsync(payment);
            await _orderRepository.UpdateAsync(order);

            _logger.LogInformation("Pagamento {PaymentId} aprovado para pedido {OrderId}", payment.Id, order.Id);

            // Publicar evento OrderConfirmedEvent
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
            // Pagamento recusado
            payment.Fail("Pagamento recusado pela operadora");
            order.Cancel();
            await _paymentRepository.UpdateAsync(payment);
            await _orderRepository.UpdateAsync(order);

            _logger.LogWarning("Pagamento {PaymentId} recusado para pedido {OrderId}", payment.Id, order.Id);

            // Publicar evento OrderCancelledEvent
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
