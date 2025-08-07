using Microsoft.Extensions.Logging;
using OrderMediatR.Infra.MessageBus;
using OrderMediatR.SyncWorker.Interfaces;
using OrderMediatR.SyncWorker.Messages;

namespace OrderMediatR.SyncWorker;

public class Worker : BackgroundService
{
    private readonly IConsumerMessageBus _bus;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<Worker> _logger;

    public Worker(
        IConsumerMessageBus bus,
        IServiceScopeFactory scopeFactory,
        ILogger<Worker> logger)
    {
        _bus = bus;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SyncWorker iniciado");

        try
        {
            var orderTask = ConsumeQueue("entity.changed.order", stoppingToken);
            var customerTask = ConsumeQueue("entity.changed.customer", stoppingToken);
            var productTask = ConsumeQueue("entity.changed.product", stoppingToken);

            await Task.WhenAll(orderTask, customerTask, productTask);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("SyncWorker cancelado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro crítico no SyncWorker");
            throw;
        }
        finally
        {
            _logger.LogInformation("SyncWorker finalizado");
        }
    }

    private async Task ConsumeQueue(string queueName, CancellationToken stoppingToken)
    {
        _logger.LogInformation("Iniciando consumo da fila: {QueueName}", queueName);

        try
        {
            await _bus.ConsumeAsync<BaseEntityMessage>(
                queue: queueName,
                onMessage: async (msg) => await ProcessMessageAsync(msg, stoppingToken),
                cancellationToken: stoppingToken
            );
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Consumo cancelado para fila: {QueueName}", queueName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro no consumo da fila: {QueueName}", queueName);
            throw;
        }
    }

    private async Task ProcessMessageAsync(BaseEntityMessage message, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dispatcher = scope.ServiceProvider.GetRequiredService<IMessageDispatcher>();
        await dispatcher.DispatchAsync(message, cancellationToken);
    }
}
