using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrderMediatR.Domain.Entities;
using OrderMediatR.Domain.Events;
using OrderMediatR.Domain.ValueObjects;
using OrderMediatR.Infra.Context;
using OrderMediatR.Infra.MessageBus;

namespace OrderMediatR.Infra.BackgroundServices
{
    public class OutboxProcessor : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OutboxProcessor> _logger;
        private readonly TimeSpan _processingInterval = TimeSpan.FromSeconds(5);
        private readonly TimeSpan _errorDelay = TimeSpan.FromSeconds(10);
        private readonly int _batchSize = 50;

        public OutboxProcessor(
            IServiceProvider serviceProvider,
            ILogger<OutboxProcessor> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("OutboxProcessor iniciado");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessPendingEvents(stoppingToken);
                    await Task.Delay(_processingInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Cancelamento normal
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro no OutboxProcessor");
                    await Task.Delay(_errorDelay, stoppingToken);
                }
            }

            _logger.LogInformation("OutboxProcessor finalizado");
        }

        private async Task ProcessPendingEvents(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<WriteContext>();
            var messageBus = scope.ServiceProvider.GetRequiredService<IPublisherMessageBus>();

            var pendingEvents = await context.OutboxEvents
                .Where(e => !e.IsProcessed && e.RetryCount < 5)
                .OrderBy(e => e.OccurredAt)
                .Take(_batchSize)
                .ToListAsync(cancellationToken);

            if (!pendingEvents.Any())
                return;

            _logger.LogDebug("Processando {Count} eventos pendentes", pendingEvents.Count);

            foreach (var outboxEvent in pendingEvents)
            {
                try
                {
                    await ProcessEvent(outboxEvent, messageBus, cancellationToken);
                    outboxEvent.MarkAsProcessed();
                    
                    _logger.LogDebug("Evento processado com sucesso: {EventId} - {EventType}", 
                        outboxEvent.Id, outboxEvent.EventType);
                }
                catch (Exception ex)
                {
                    outboxEvent.MarkAsFailed(ex.Message);
                    _logger.LogWarning(ex, "Falha ao processar evento: {EventId} - {EventType} (Tentativa {RetryCount})", 
                        outboxEvent.Id, outboxEvent.EventType, outboxEvent.RetryCount);
                }
            }

            var processedCount = pendingEvents.Count(e => e.IsProcessed);
            var failedCount = pendingEvents.Count - processedCount;

            await context.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Lote processado: {ProcessedCount} sucessos, {FailedCount} falhas", 
                processedCount, failedCount);
        }

        private async Task ProcessEvent(OutboxEvent outboxEvent, IPublisherMessageBus messageBus, CancellationToken cancellationToken)
        {
            var eventTypeName = outboxEvent.EventType;
            
            _logger.LogInformation("Processando evento: {EventType}", eventTypeName);
            _logger.LogInformation("EventData: {EventData}", outboxEvent.EventData);
            
            try
            {
            
                if (eventTypeName == "MediatR.INotification" || eventTypeName.Contains("INotification"))
                {
                    await ProcessLegacyEvent(outboxEvent, messageBus, cancellationToken);
                    return;
                }
                
                if (eventTypeName.Contains("EntityChangedDomainEvent<Order>"))
                {
                    var orderEvent = JsonSerializer.Deserialize<EntityChangedDomainEvent<Order>>(outboxEvent.EventData);
                    _logger.LogInformation("OrderEvent deserializado: {OrderEvent}", orderEvent?.GetType().Name ?? "NULL");
                    if (orderEvent != null)
                        await PublishEntityEvent(orderEvent, messageBus, cancellationToken);
                }
                else if (eventTypeName.Contains("EntityChangedDomainEvent<Customer>"))
                {
                    // Deserializar o JSON manual que criamos
                    using var jsonDoc = JsonDocument.Parse(outboxEvent.EventData);
                    var root = jsonDoc.RootElement;
                    
                    if (root.TryGetProperty("entity", out var entityElement) && 
                        root.TryGetProperty("changeType", out var changeTypeElement) &&
                        root.TryGetProperty("occurredAt", out var occurredAtElement))
                    {
                        // Deserializar as propriedades individuais
                        var entity = entityElement;
                        var id = Guid.Parse(entity.GetProperty("id").GetString() ?? "");
                        var firstName = entity.GetProperty("firstName").GetString() ?? "";
                        var lastName = entity.GetProperty("lastName").GetString() ?? "";
                        var emailValue = entity.GetProperty("email").GetProperty("value").GetString() ?? "";
                        var phoneValue = entity.GetProperty("phone").GetProperty("value").GetString() ?? "";
                        var documentNumber = entity.GetProperty("documentNumber").GetString();
                        var dateOfBirth = entity.GetProperty("dateOfBirth").GetDateTime();
                        var createdAt = entity.GetProperty("createdAt").GetDateTime();
                        var updatedAt = entity.GetProperty("updatedAt").GetDateTime();
                        var isActive = entity.GetProperty("isActive").GetBoolean();
                        
                        // Criar o Customer usando FromSync (sem validações)
                        var email = new Email(emailValue);
                        var phone = new Phone(phoneValue);
                        var customer = Customer.FromSync(id, firstName, lastName, email, phone, documentNumber, dateOfBirth, createdAt, updatedAt, isActive);
                        
                        var changeType = changeTypeElement.GetString() ?? "Created";
                        
                        var customerEvent = new EntityChangedDomainEvent<Customer>(customer, changeType);
                        _logger.LogInformation("CustomerEvent deserializado manualmente: {CustomerEvent}", customerEvent?.GetType().Name ?? "NULL");
                        if (customerEvent != null)
                            await PublishEntityEvent(customerEvent, messageBus, cancellationToken);
                    }
                }
                else if (eventTypeName.Contains("EntityChangedDomainEvent<Product>"))
                {
                    var productEvent = JsonSerializer.Deserialize<EntityChangedDomainEvent<Product>>(outboxEvent.EventData);
                    _logger.LogInformation("ProductEvent deserializado: {ProductEvent}", productEvent?.GetType().Name ?? "NULL");
                    if (productEvent != null)
                        await PublishEntityEvent(productEvent, messageBus, cancellationToken);
                }
                else
                {
                    _logger.LogWarning("Tipo de evento não suportado: {EventType}", outboxEvent.EventType);
                    throw new NotSupportedException($"Tipo de evento não suportado: {outboxEvent.EventType}");
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Erro na deserialização do evento: {EventType} - {EventData}", eventTypeName, outboxEvent.EventData);
                throw;
            }
        }

        private async Task ProcessLegacyEvent(OutboxEvent outboxEvent, IPublisherMessageBus messageBus, CancellationToken cancellationToken)
        {
            // Tentar deserializar como JsonDocument para analisar o conteúdo
            using var jsonDoc = JsonDocument.Parse(outboxEvent.EventData);
            var root = jsonDoc.RootElement;
            
            // Verificar se tem propriedade Entity e qual é o tipo
            if (root.TryGetProperty("Entity", out var entityElement))
            {
                if (entityElement.TryGetProperty("Id", out var idElement) && idElement.ValueKind == JsonValueKind.String)
                {
                    // Tentar deserializar baseado no conteúdo
                    if (outboxEvent.EventData.Contains("\"FirstName\"") || outboxEvent.EventData.Contains("\"LastName\""))
                    {
                        var customerEvent = JsonSerializer.Deserialize<EntityChangedDomainEvent<Customer>>(outboxEvent.EventData);
                        if (customerEvent != null)
                        {
                            await PublishEntityEvent(customerEvent, messageBus, cancellationToken);
                            return;
                        }
                    }
                    else if (outboxEvent.EventData.Contains("\"OrderNumber\"") || outboxEvent.EventData.Contains("\"TotalAmount\""))
                    {
                        var orderEvent = JsonSerializer.Deserialize<EntityChangedDomainEvent<Order>>(outboxEvent.EventData);
                        if (orderEvent != null)
                        {
                            await PublishEntityEvent(orderEvent, messageBus, cancellationToken);
                            return;
                        }
                    }
                    else if (outboxEvent.EventData.Contains("\"Sku\"") || outboxEvent.EventData.Contains("\"Price\""))
                    {
                        var productEvent = JsonSerializer.Deserialize<EntityChangedDomainEvent<Product>>(outboxEvent.EventData);
                        if (productEvent != null)
                        {
                            await PublishEntityEvent(productEvent, messageBus, cancellationToken);
                            return;
                        }
                    }
                }
            }
            
            _logger.LogWarning("Não foi possível determinar o tipo do evento legado: {EventData}", outboxEvent.EventData);
            throw new NotSupportedException($"Não foi possível determinar o tipo do evento legado");
        }

        private async Task PublishEntityEvent<T>(EntityChangedDomainEvent<T> domainEvent, IPublisherMessageBus messageBus, CancellationToken cancellationToken)
        {
            _logger.LogInformation("PublishEntityEvent chamado - Tipo: {EventType}, Evento: {Event}, Entidade: {Entity}", 
                typeof(T).Name, 
                domainEvent?.GetType().Name ?? "NULL", 
                domainEvent?.Entity?.GetType().Name ?? "NULL");
            
            if (domainEvent == null)
            {
                _logger.LogWarning("Evento é null, pulando publicação");
                return;
            }
            
            if (domainEvent.Entity == null)
            {
                _logger.LogWarning("Entidade dentro do evento é null, pulando publicação");
                return;
            }
            
            var entityType = typeof(T).Name;
            var queueName = $"entity.changed.{entityType.ToLowerInvariant()}";

            // Converter a entidade para o formato de mensagem apropriado
            object messagePayload;
            if (typeof(T) == typeof(Customer))
            {
                var customer = domainEvent.Entity as Customer;
                messagePayload = new
                {
                    Id = customer.Id,
                    FirstName = customer.FirstName,
                    LastName = customer.LastName,
                    Email = customer.Email.Value,
                    Phone = customer.Phone.Value,
                    DocumentNumber = customer.DocumentNumber,
                    DateOfBirth = customer.DateOfBirth,
                    CreatedAt = customer.CreatedAt,
                    UpdatedAt = customer.UpdatedAt,
                    IsActive = customer.IsActive
                };
            }
            else if (typeof(T) == typeof(Order))
            {
                var order = domainEvent.Entity as Order;
                messagePayload = new
                {
                    Id = order.Id,
                    OrderNumber = order.OrderNumber.Value,
                    CustomerId = order.CustomerId,
                    TotalAmount = order.TotalAmount.Amount,
                    Currency = order.TotalAmount.Currency,
                    Status = order.Status,
                    CreatedAt = order.CreatedAt,
                    UpdatedAt = order.UpdatedAt
                };
            }
            else if (typeof(T) == typeof(Product))
            {
                var product = domainEvent.Entity as Product;
                messagePayload = new
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    Sku = product.Sku.Value,
                    Price = product.Price.Amount,
                    Currency = product.Price.Currency,
                    StockQuantity = product.StockQuantity,
                    CreatedAt = product.CreatedAt,
                    UpdatedAt = product.UpdatedAt,
                    IsActive = product.IsActive
                };
            }
            else
            {
                messagePayload = domainEvent.Entity;
            }

            var payload = new
            {
                EventId = Guid.NewGuid(),
                EntityId = GetEntityId(domainEvent.Entity),
                EntityType = entityType,
                ChangeType = domainEvent.ChangeType,
                OccurredAt = domainEvent.OccurredAt,
                Payload = JsonSerializer.Serialize(messagePayload, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                })
            };

            _logger.LogInformation("Tentando publicar evento na fila: {Queue}", queueName);
            _logger.LogInformation("Payload: {Payload}", JsonSerializer.Serialize(payload));
            
            try
            {
                await messageBus.PublishAsync(queueName, payload, cancellationToken);
                _logger.LogInformation("Evento publicado com sucesso: {EntityType} - {ChangeType} - Queue: {Queue}", 
                    entityType, domainEvent.ChangeType, queueName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao publicar evento: {EntityType} - {ChangeType} - Queue: {Queue}", 
                    entityType, domainEvent.ChangeType, queueName);
                throw;
            }
        }

        private static Guid GetEntityId(object entity)
        {
            if (entity == null)
                throw new InvalidOperationException("Entidade é null");
                
            // Usar reflexão para obter o Id da entidade
            var idProperty = entity.GetType().GetProperty("Id");
            if (idProperty?.GetValue(entity) is Guid id)
                return id;
                
            throw new InvalidOperationException($"Entidade {entity.GetType().Name} não possui propriedade Id válida");
        }
    }
}