using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrderMediatR.SyncWorker.Interfaces;
using OrderMediatR.SyncWorker.Messages;

namespace OrderMediatR.SyncWorker.Services
{
    public class MessageDispatcher : IMessageDispatcher
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MessageDispatcher> _logger;

        public MessageDispatcher(
            IServiceProvider serviceProvider,
            ILogger<MessageDispatcher> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task DispatchAsync(BaseEntityMessage envelope, CancellationToken cancellationToken)
        {
            if (envelope == null)
            {
                _logger.LogWarning("Mensagem recebida é null");
                return;
            }

            _logger.LogDebug("Processando mensagem: {EntityType} - {ChangeType} - {EntityId}", 
                envelope.EntityType, envelope.ChangeType, envelope.EntityId);

            using var scope = _serviceProvider.CreateScope();
            
            switch (envelope.EntityType)
            {
                case "Order":
                    var orderMessage = JsonSerializer.Deserialize<OrderMessage>(envelope.Payload, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });
                    if (orderMessage != null)
                    {
                        var orderHandler = scope.ServiceProvider.GetRequiredService<IMessageHandler<OrderMessage>>();
                        await orderHandler.HandleAsync(orderMessage, cancellationToken);
                    }
                    break;

                case "Customer":
                    var customerMessage = JsonSerializer.Deserialize<CustomerMessage>(envelope.Payload, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });
                    if (customerMessage != null)
                    {
                        var customerHandler = scope.ServiceProvider.GetRequiredService<IMessageHandler<CustomerMessage>>();
                        await customerHandler.HandleAsync(customerMessage, cancellationToken);
                    }
                    break;

                case "Product":
                    var productMessage = JsonSerializer.Deserialize<ProductMessage>(envelope.Payload, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });
                    if (productMessage != null)
                    {
                        var productHandler = scope.ServiceProvider.GetRequiredService<IMessageHandler<ProductMessage>>();
                        await productHandler.HandleAsync(productMessage, cancellationToken);
                    }
                    break;
                    
                default:
                    _logger.LogWarning("Tipo de entidade não suportado: {EntityType}", envelope.EntityType);
                    break;
            }

            _logger.LogDebug("Mensagem processada com sucesso: {EntityType} - {EntityId}", 
                envelope.EntityType, envelope.EntityId);
        }
    }
}