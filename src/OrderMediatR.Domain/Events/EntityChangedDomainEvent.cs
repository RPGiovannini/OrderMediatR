using MediatR;

namespace OrderMediatR.Domain.Events
{
    public class EntityChangedDomainEvent<T> : INotification
    {
        public T Entity { get; set; }
        public string ChangeType { get; set; }
        public DateTime OccurredAt { get; set; }
        
        public EntityChangedDomainEvent(T entity, string changeType)
        {
            Entity = entity;
            ChangeType = changeType;
            OccurredAt = DateTime.UtcNow;
        }
        
        public EntityChangedDomainEvent()
        {
        }
    }
}