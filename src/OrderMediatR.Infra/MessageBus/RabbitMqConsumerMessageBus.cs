using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace OrderMediatR.Infra.MessageBus
{
    public class RabbitMqConsumerMessageBus : IConsumerMessageBus, IDisposable
    {
        private readonly RabbitMqSettings _settings;
        private readonly ILogger<RabbitMqConsumerMessageBus> _logger;
        private readonly ConnectionFactory _factory;
        private IConnection? _connection;
        private IChannel? _channel;

        public RabbitMqConsumerMessageBus(
            IOptions<RabbitMqSettings> settings,
            ILogger<RabbitMqConsumerMessageBus> logger)
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

        public async Task ConsumeAsync<T>(string queue, Func<T, Task> onMessage, CancellationToken cancellationToken = default)
        {
            await EnsureConnectionAsync();
            
            if (_channel == null)
                throw new InvalidOperationException("Canal RabbitMQ não foi inicializado");

            await _channel.QueueDeclareAsync(
                queue: queue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: cancellationToken);

            // Configurar QoS para processar uma mensagem por vez
            await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            
            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);
                    
                    var message = JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });

                    if (message != null)
                    {
                        await onMessage(message);
                        
                        // Confirmar processamento apenas após sucesso
                        await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
                        
                        _logger.LogDebug("Mensagem processada com sucesso. Queue: {Queue}, MessageId: {MessageId}", 
                            queue, ea.BasicProperties.MessageId);
                    }
                    else
                    {
                        _logger.LogWarning("Mensagem deserializada como null. Queue: {Queue}", queue);
                        await _channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao processar mensagem. Queue: {Queue}, MessageId: {MessageId}", 
                        queue, ea.BasicProperties.MessageId);
                    
                    // Rejeitar mensagem e reenviar para fila (retry)
                    await _channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
                }
            };

            await _channel.BasicConsumeAsync(
                queue: queue,
                autoAck: false, 
                consumer: consumer,
                cancellationToken: cancellationToken);

            _logger.LogInformation("Consumidor iniciado para fila: {Queue}", queue);

            // Manter consumidor ativo
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                
                // Verificar se conexão ainda está ativa
                if (_connection?.IsOpen != true || _channel?.IsOpen != true)
                {
                    _logger.LogWarning("Conexão perdida, tentando reconectar...");
                    await EnsureConnectionAsync();
                }
            }
        }

        private async Task EnsureConnectionAsync()
        {
            if (_connection?.IsOpen == true && _channel?.IsOpen == true)
                return;

            await DisposeConnectionAsync();

            var maxRetries = _settings.ConnectionRetryCount;
            
            for (int retry = 0; retry < maxRetries; retry++)
            {
                try
                {
                    _connection = await _factory.CreateConnectionAsync();
                    _channel = await _connection.CreateChannelAsync();
                    
                    _logger.LogDebug("Conexão RabbitMQ estabelecida para consumo");
                    return;
                }
                catch (Exception ex) when (retry < maxRetries - 1)
                {
                    _logger.LogWarning(ex, "Falha na conexão (tentativa {Retry}/{MaxRetries})", 
                        retry + 1, maxRetries);
                    
                    await Task.Delay(_settings.ConnectionRetryDelay);
                }
            }

            throw new InvalidOperationException($"Falha ao conectar após {maxRetries} tentativas");
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