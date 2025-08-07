namespace OrderMediatR.SyncWorker.Messages
{
    public class BaseEntityMessage
    {
        public Guid EventId { get; set; }
        public Guid EntityId { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public string ChangeType { get; set; } = string.Empty;
        public DateTime OccurredAt { get; set; }
        public string Payload { get; set; } = string.Empty;
    }
}