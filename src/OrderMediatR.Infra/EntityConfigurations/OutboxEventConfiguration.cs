using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderMediatR.Domain.Entities;

namespace OrderMediatR.Infra.EntityConfigurations
{
    public class OutboxEventConfiguration : BaseEntityConfiguration<OutboxEvent>
    {
        public override void Configure(EntityTypeBuilder<OutboxEvent> builder)
        {
            base.Configure(builder);
            
            builder.ToTable("OutboxEvents");
            
            builder.Property(e => e.EventType)
                .HasMaxLength(200)
                .IsRequired();
                
            builder.Property(e => e.EventData)
                .IsRequired();
                
            builder.Property(e => e.OccurredAt)
                .IsRequired();
                
            builder.Property(e => e.ProcessedAt);
            
            builder.Property(e => e.IsProcessed)
                .IsRequired()
                .HasDefaultValue(false);
                
            builder.Property(e => e.Error)
                .HasMaxLength(1000);
                
            builder.Property(e => e.RetryCount)
                .IsRequired()
                .HasDefaultValue(0);
            
            // Índices críticos para performance
            builder.HasIndex(e => new { e.IsProcessed, e.OccurredAt })
                .HasDatabaseName("IX_OutboxEvents_IsProcessed_OccurredAt");
            
            builder.HasIndex(e => e.EventType)
                .HasDatabaseName("IX_OutboxEvents_EventType");
                
            builder.HasIndex(e => e.RetryCount)
                .HasDatabaseName("IX_OutboxEvents_RetryCount");
        }
    }
}