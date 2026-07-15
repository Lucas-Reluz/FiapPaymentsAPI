namespace PaymentsAPI.Domain.Interfaces;

public interface IEventPublisher
{
    Task PublishAsync<T>(T @event, string exchange) where T : class;
}
