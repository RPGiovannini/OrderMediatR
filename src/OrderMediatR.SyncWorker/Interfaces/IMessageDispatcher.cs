using OrderMediatR.SyncWorker.Messages;

namespace OrderMediatR.SyncWorker.Interfaces
{
    public interface IMessageDispatcher
    {
        Task DispatchAsync(BaseEntityMessage envelope, CancellationToken cancellationToken);
    }
}