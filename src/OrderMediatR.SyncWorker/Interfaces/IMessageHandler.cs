namespace OrderMediatR.SyncWorker.Interfaces
{
    public interface IMessageHandler<T>
    {
        Task HandleAsync(T message, CancellationToken cancellationToken);
    }
}