using MediatR;

namespace OrderMediatR.Common
{
    public abstract class BaseEntity
    {
        private List<INotification>? _domainEvents;
        public IReadOnlyCollection<INotification>? DomainEvents => _domainEvents?.AsReadOnly();
        
        public Guid Id { get; protected set; }
        public DateTime CreatedAt { get; protected set; }
        public DateTime? UpdatedAt { get; protected set; }
        public bool IsActive { get; protected set; } = true;
        
        protected BaseEntity() 
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
        }
        
        protected BaseEntity(Guid id) 
        {
            Id = id == Guid.Empty ? Guid.NewGuid() : id;
            CreatedAt = DateTime.UtcNow;
        }
        
        public void AddDomainEvent(INotification eventItem)
        {
            _domainEvents ??= new List<INotification>();
            _domainEvents.Add(eventItem);
        }
        
        public void ClearDomainEvents() => _domainEvents?.Clear();
        
        protected void SetUpdatedAt() => UpdatedAt = DateTime.UtcNow;
    }
}