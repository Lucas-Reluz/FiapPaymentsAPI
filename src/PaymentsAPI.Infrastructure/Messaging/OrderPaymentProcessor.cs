using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PaymentsAPI.Domain.Enums;
using PaymentsAPI.Domain.Events;
using PaymentsAPI.Domain.Interfaces;

namespace PaymentsAPI.Infrastructure.Messaging;

public class OrderPaymentProcessor : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OrderPaymentProcessor> _logger;
    private readonly TimeSpan _processingInterval = TimeSpan.FromSeconds(10);

    public OrderPaymentProcessor(
        IServiceProvider serviceProvider,
        ILogger<OrderPaymentProcessor> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OrderPaymentProcessor iniciado");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingPaymentsAsync();
                await Task.Delay(_processingInterval, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar pagamentos pendentes");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        _logger.LogInformation("OrderPaymentProcessor finalizado");
    }

    private async Task ProcessPendingPaymentsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var orderRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
        var paymentRepository = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();
        var eventPublisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();

        _logger.LogDebug("Verificando pagamentos pendentes...");
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("OrderPaymentProcessor parando");
        return base.StopAsync(cancellationToken);
    }
}
