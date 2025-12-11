using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace Orders.Api.Messaging;

public class RabbitMqEventPublisher : IEventPublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMqEventPublisher(IConfiguration configuration)
    {
        var factory = new ConnectionFactory
        {
            HostName = configuration.GetValue<string>("RabbitMQ:Host") ?? "localhost",
            Port = configuration.GetValue<int?>("RabbitMQ:Port") ?? 5672,
            UserName = configuration.GetValue<string>("RabbitMQ:User") ?? "guest",
            Password = configuration.GetValue<string>("RabbitMQ:Password") ?? "guest"
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
    }

    public Task PublishAsync<T>(string exchange, string routingKey, T message)
    {
        // declaramos exchange tipo topic (idempotente)
        _channel.ExchangeDeclare(exchange: exchange, type: ExchangeType.Topic, durable: true);

        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        var properties = _channel.CreateBasicProperties();
        properties.ContentType = "application/json";
        properties.DeliveryMode = 2; // persistente

        _channel.BasicPublish(
            exchange: exchange,
            routingKey: routingKey,
            basicProperties: properties,
            body: body);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        if (_channel.IsOpen)
            _channel.Close();

        _channel.Dispose();
        _connection.Dispose();
    }
}

