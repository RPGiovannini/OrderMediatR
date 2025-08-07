namespace OrderMediatR.Infra.MessageBus
{
    public interface IPublisherMessageBus
    {
        Task PublishAsync<T>(string queue, T message, CancellationToken cancellationToken = default);
    }
}