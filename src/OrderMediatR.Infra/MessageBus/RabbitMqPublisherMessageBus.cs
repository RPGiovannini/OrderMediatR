using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace OrderMediatR.Infra.MessageBus
{
    public class RabbitMqPublisherMessageBus : IPublisherMessageBus, IDisposable
    {
        private readonly RabbitMqSettings _settings;
        private readonly ILogger<RabbitMqPublisherMessageBus> _logger;
        private readonly ConnectionFactory _factory;
        private IConnection? _connection;
        private IChannel? _channel;

        public RabbitMqPublisherMessageBus(
            IOptions<RabbitMqSettings> settings,
            ILogger<RabbitMqPublisherMessageBus> logger)
        {
            _settings = settings.Value;
            _logger = logger;
            
            _factory = new ConnectionFactory
            {
                HostName = _settings.Host,
                Port = _settings.Port,
                UserName = _settings.Username,
                Password = _settings.Password,
                VirtualHost = _settings.VirtualHost
            };
        }

        public async Task PublishAsync<T>(string queue, T message, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Iniciando publicação na fila: {Queue}", queue);
            
            var maxRetries = _settings.ConnectionRetryCount;
            
            for (int retry = 0; retry < maxRetries; retry++)
            {
                try
                {
                    _logger.LogInformation("Tentativa {Retry}/{MaxRetries} de publicação", retry + 1, maxRetries);
                    
                    await EnsureConnectionAsync();
                    
                    if (_channel == null)
                        throw new InvalidOperationException("Canal RabbitMQ não foi inicializado");

                    _logger.LogInformation("Declarando fila: {Queue}", queue);
                    await _channel.QueueDeclareAsync(
                        queue: queue,
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null,
                        cancellationToken: cancellationToken);

                    var json = JsonSerializer.Serialize(message, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });
                    
                    var body = Encoding.UTF8.GetBytes(json);
                    _logger.LogInformation("Mensagem serializada: {Json}", json);

                    var properties = new BasicProperties
                    {
                        Persistent = true,
                        MessageId = Guid.NewGuid().ToString(),
                        Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
                        ContentType = "application/json",
                        DeliveryMode = DeliveryModes.Persistent
                    };

                    _logger.LogInformation("Publicando mensagem na fila: {Queue}", queue);
                    await _channel.BasicPublishAsync(
                        exchange: "",
                        routingKey: queue,
                        mandatory: true,
                        basicProperties: properties,
                        body: body,
                        cancellationToken: cancellationToken);

                    _logger.LogInformation("Mensagem publicada com sucesso na fila: {Queue}, MessageId: {MessageId}", 
                        queue, properties.MessageId);
                    return;
                }
                catch (Exception ex) when (retry < maxRetries - 1)
                {
                    _logger.LogWarning(ex, "Falha na publicação (tentativa {Retry}/{MaxRetries})", 
                        retry + 1, maxRetries);
                    
                    await DisposeConnectionAsync();
                    await Task.Delay(_settings.ConnectionRetryDelay, cancellationToken);
                }
            }

            _logger.LogError("Falha ao publicar mensagem após {MaxRetries} tentativas", maxRetries);
            throw new InvalidOperationException($"Falha ao publicar mensagem após {maxRetries} tentativas");
        }

        private async Task EnsureConnectionAsync()
        {
            if (_connection?.IsOpen == true && _channel?.IsOpen == true)
                return;

            await DisposeConnectionAsync();

            _connection = await _factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();
            
            _logger.LogDebug("Conexão RabbitMQ estabelecida");
        }

        private async Task DisposeConnectionAsync()
        {
            try
            {
                if (_channel?.IsOpen == true)
                {
                    await _channel.CloseAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao fechar canal RabbitMQ");
            }
            finally
            {
                _channel?.Dispose();
                _channel = null;
            }

            try
            {
                if (_connection?.IsOpen == true)
                {
                    await _connection.CloseAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao fechar conexão RabbitMQ");
            }
            finally
            {
                _connection?.Dispose();
                _connection = null;
            }
        }

        public void Dispose()
        {
            DisposeConnectionAsync().GetAwaiter().GetResult();
        }
    }
}