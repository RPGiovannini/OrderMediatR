namespace OrderMediatR.Infra.MessageBus
{
    public interface IConsumerMessageBus
    {
        Task ConsumeAsync<T>(string queue, Func<T, Task> onMessage, CancellationToken cancellationToken = default);
    }
}