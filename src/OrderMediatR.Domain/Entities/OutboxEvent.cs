using System.Text.Json;
using MediatR;
using OrderMediatR.Common;

namespace OrderMediatR.Domain.Entities
{
    public class OutboxEvent : BaseEntity
    {
        public string EventType { get; private set; }
        public string EventData { get; private set; }
        public DateTime OccurredAt { get; private set; }
        public DateTime? ProcessedAt { get; private set; }
        public bool IsProcessed { get; private set; }
        public string? Error { get; private set; }
        public int RetryCount { get; private set; }

        protected OutboxEvent() { }

        private OutboxEvent(string eventType, string eventData, DateTime occurredAt)
        {
            EventType = eventType;
            EventData = eventData;
            OccurredAt = occurredAt;
            IsProcessed = false;
            RetryCount = 0;
        }

        public static OutboxEvent Create(Type eventType, INotification domainEvent)
        {
            string eventTypeName;
            
            if (eventType.IsGenericType)
            {
                var typeName = eventType.Name.Split('`')[0];
                var genericArgs = eventType.GetGenericArguments();
                var argNames = genericArgs.Select(t => t.Name).ToArray();
                eventTypeName = $"{typeName}<{string.Join(",", argNames)}>";
            }
            else
            {
                eventTypeName = eventType.FullName ?? eventType.Name;
            }
                
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false,
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never
            };
            
            var manualJson = new
            {
                entity = domainEvent.GetType().GetProperty("Entity")?.GetValue(domainEvent),
                changeType = domainEvent.GetType().GetProperty("ChangeType")?.GetValue(domainEvent),
                occurredAt = domainEvent.GetType().GetProperty("OccurredAt")?.GetValue(domainEvent)
            };
            
            var json = JsonSerializer.Serialize(manualJson, jsonOptions);
                
            return new OutboxEvent(
                eventTypeName,
                json,
                DateTime.UtcNow
            );
        }

        public void MarkAsProcessed()
        {
            IsProcessed = true;
            ProcessedAt = DateTime.UtcNow;
            Error = null;
            SetUpdatedAt();
        }

        public void MarkAsFailed(string error)
        {
            Error = error;
            RetryCount++;
            SetUpdatedAt();
        }

        public bool ShouldRetry() => RetryCount < 5;
    }
}