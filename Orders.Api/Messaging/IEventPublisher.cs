namespace Orders.Api.Messaging;

public interface IEventPublisher
{
    Task PublishAsync<T>(string exchange, string routingKey, T message);
}