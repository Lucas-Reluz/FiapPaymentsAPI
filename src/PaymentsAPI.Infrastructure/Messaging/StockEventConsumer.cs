using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PaymentsAPI.Domain.Entities;
using PaymentsAPI.Domain.Enums;
using PaymentsAPI.Domain.Events;
using PaymentsAPI.Domain.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PaymentsAPI.Infrastructure.Messaging;

public class StockEventConsumer : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<StockEventConsumer> _logger;
    private IConnection? _connection;
    private IChannel? _channel;

    public StockEventConsumer(
        IConfiguration configuration,
        IServiceProvider serviceProvider,
        ILogger<StockEventConsumer> logger)
    {
        _configuration = configuration;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var rabbitMqSettings = _configuration.GetSection("RabbitMQ");
            var hostName = rabbitMqSettings["Host"] ?? "localhost";
            var port = int.Parse(rabbitMqSettings["Port"] ?? "5672");
            var userName = rabbitMqSettings["UserName"] ?? "guest";
            var password = rabbitMqSettings["Password"] ?? "guest";
            var queueName = rabbitMqSettings["CatalogStockQueue"] ?? "payments.catalog.stock.queue";
            var exchangeName = rabbitMqSettings["CatalogExchange"] ?? "catalog.exchange";

            var factory = new ConnectionFactory
            {
                HostName = hostName,
                Port = port,
                UserName = userName,
                Password = password
            };

            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();
            await _channel.ExchangeDeclareAsync(exchangeName, ExchangeType.Fanout, true, false);
            await _channel.QueueDeclareAsync(queueName, true, false, false);
            await _channel.QueueBindAsync(queueName, exchangeName, string.Empty);

            _logger.LogInformation("StockEventConsumer conectado à fila {QueueName}", queueName);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);

                    _logger.LogInformation("Mensagem recebida do CatalogAPI: {Message}", message);

                    using var jsonDocument = JsonDocument.Parse(message);
                    var root = jsonDocument.RootElement;

                    if (root.TryGetProperty("RequestedQuantity", out _) ||
                        root.TryGetProperty("AvailableStock", out _))
                    {
                        var stockInsufficientEvent = JsonSerializer.Deserialize<StockInsufficientEvent>(message);
                        if (stockInsufficientEvent != null && stockInsufficientEvent.OrderId != Guid.Empty)
                        {
                            await ProcessStockInsufficientEventAsync(stockInsufficientEvent);
                            await _channel.BasicAckAsync(ea.DeliveryTag, false);
                            return;
                        }
                    }

                    if (root.TryGetProperty("Quantity", out _))
                    {
                        var stockReservedEvent = JsonSerializer.Deserialize<StockReservedEvent>(message);
                        if (stockReservedEvent != null && stockReservedEvent.OrderId != Guid.Empty)
                        {
                            await ProcessStockReservedEventAsync(stockReservedEvent);
                            await _channel.BasicAckAsync(ea.DeliveryTag, false);
                            return;
                        }
                    }

                    _logger.LogWarning("Mensagem não reconhecida: {Message}", message);
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao processar mensagem do CatalogAPI");
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, true);
                }
            };

            await _channel.BasicConsumeAsync(queueName, false, consumer, stoppingToken);

            _logger.LogInformation("StockEventConsumer iniciado e escutando fila {QueueName}", queueName);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao iniciar StockEventConsumer");
        }
    }

    private async Task ProcessStockReservedEventAsync(StockReservedEvent @event)
    {
        using var scope = _serviceProvider.CreateScope();
        var orderRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
        var paymentRepository = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();

        _logger.LogInformation("Estoque reservado para pedido {OrderId}", @event.OrderId);

        var order = await orderRepository.GetByIdAsync(@event.OrderId);
        if (order == null)
        {
            _logger.LogWarning("Pedido {OrderId} não encontrado", @event.OrderId);
            return;
        }

        if (order.Status != OrderStatus.Pending)
        {
            _logger.LogInformation("Ignorando evento de estoque repetido para pedido {OrderId} com status {Status}",
                @event.OrderId, order.Status);
            return;
        }
        order.ConfirmStock();
        await orderRepository.UpdateAsync(order);

        _logger.LogInformation("Pedido {OrderId} atualizado para AwaitingPayment", @event.OrderId);
        var payment = new Payment(order.Id, order.TotalPrice, "CreditCard");
        await paymentRepository.AddAsync(payment);

        _logger.LogInformation("Pagamento {PaymentId} criado para pedido {OrderId}", payment.Id, order.Id);
    }

    private async Task ProcessStockInsufficientEventAsync(StockInsufficientEvent @event)
    {
        using var scope = _serviceProvider.CreateScope();
        var orderRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
        var eventPublisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();

        _logger.LogWarning("Estoque insuficiente para pedido {OrderId}. Solicitado: {Requested}, Disponível: {Available}",
            @event.OrderId, @event.RequestedQuantity, @event.AvailableStock);

        var order = await orderRepository.GetByIdAsync(@event.OrderId);
        if (order == null)
        {
            _logger.LogWarning("Pedido {OrderId} não encontrado", @event.OrderId);
            return;
        }

        if (order.Status != OrderStatus.Pending)
        {
            _logger.LogWarning("Ignorando cancelamento de estoque para pedido {OrderId} com status {Status}",
                @event.OrderId, order.Status);
            return;
        }
        order.Cancel();
        await orderRepository.UpdateAsync(order);

        _logger.LogInformation("Pedido {OrderId} cancelado por estoque insuficiente", @event.OrderId);
        var orderCancelledEvent = new OrderCancelledEvent
        {
            OrderId = order.Id,
            UserId = order.UserId,
            GameId = order.GameId,
            GameTitle = order.GameTitle,
            CancellationReason = $"Estoque insuficiente. Solicitado: {@event.RequestedQuantity}, Disponível: {@event.AvailableStock}",
            CancelledAt = DateTime.UtcNow
        };

        await eventPublisher.PublishAsync(orderCancelledEvent, "payment.exchange");

        _logger.LogInformation("Evento OrderCancelledEvent publicado para pedido {OrderId}", order.Id);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("StockEventConsumer parando");

        if (_channel != null)
            await _channel.CloseAsync();

        if (_connection != null)
            await _connection.CloseAsync();

        await base.StopAsync(cancellationToken);

        _logger.LogInformation("StockEventConsumer finalizado");
    }
}
