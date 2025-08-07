using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderMediatR.Domain.Entities;
using OrderMediatR.Infra.EntityConfigurations.ValueObjects;

namespace OrderMediatR.Infra.EntityConfigurations
{
    public class OrderItemConfiguration : BaseEntityConfiguration<OrderItem>
    {
        public override void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            base.Configure(builder);
            
            builder.ToTable("OrderItems");
            
            builder.Property(e => e.Quantity)
                .IsRequired();
                
            builder.Property(e => e.OrderId)
                .IsRequired();
                
            builder.Property(e => e.ProductId)
                .IsRequired();

            builder.OwnsOne(oi => oi.UnitPrice, price => BaseValueObjectConfiguration.ConfigureMoney(price, "UnitPrice"));
            builder.OwnsOne(oi => oi.TotalPrice, total => BaseValueObjectConfiguration.ConfigureMoney(total, "TotalPrice"));
            builder.OwnsOne(oi => oi.DiscountAmount, discount => BaseValueObjectConfiguration.ConfigureOptionalMoney(discount, "DiscountAmount"));
            
            // Relacionamentos
            builder.HasOne(e => e.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
                
            builder.HasOne(e => e.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // Índices
            builder.HasIndex(e => e.OrderId)
                .HasDatabaseName("IX_OrderItems_OrderId");
                
            builder.HasIndex(e => e.ProductId)
                .HasDatabaseName("IX_OrderItems_ProductId");
        }
    }
}
