using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PaymentsAPI.Domain.Interfaces;
using RabbitMQ.Client;

namespace PaymentsAPI.Infrastructure.Messaging;

public class RabbitMqPublisher : IEventPublisher
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<RabbitMqPublisher> _logger;

    public RabbitMqPublisher(IConfiguration configuration, ILogger<RabbitMqPublisher> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task PublishAsync<T>(T @event, string exchange) where T : class
    {
        var rabbitMqSettings = _configuration.GetSection("RabbitMQ");
        var hostName = rabbitMqSettings["Host"] ?? "localhost";
        var port = int.Parse(rabbitMqSettings["Port"] ?? "5672");
        var userName = rabbitMqSettings["UserName"] ?? "guest";
        var password = rabbitMqSettings["Password"] ?? "guest";

        var factory = new ConnectionFactory
        {
            HostName = hostName,
            Port = port,
            UserName = userName,
            Password = password
        };

        await using var connection = await factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();
        await channel.ExchangeDeclareAsync(exchange, ExchangeType.Fanout, durable: true, autoDelete: false);
        var message = JsonSerializer.Serialize(@event);
        var body = Encoding.UTF8.GetBytes(message);
        await channel.BasicPublishAsync(exchange, string.Empty, body);

        _logger.LogInformation("Evento {EventType} publicado com sucesso em {Exchange}", 
            typeof(T).Name, exchange);
    }
}
