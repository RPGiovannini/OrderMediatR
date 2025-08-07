using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderMediatR.Domain.Entities;
using OrderMediatR.Infra.EntityConfigurations.ValueObjects;

namespace OrderMediatR.Infra.EntityConfigurations
{
    public class OrderConfiguration : BaseEntityConfiguration<Order>
    {
        public override void Configure(EntityTypeBuilder<Order> builder)
        {
            base.Configure(builder);
            
            builder.ToTable("Orders");
            
            builder.Property(e => e.Status)
                .IsRequired()
                .HasConversion<int>();
                
            builder.Property(e => e.Notes)
                .HasMaxLength(1000);
                
            builder.Property(e => e.EstimatedDeliveryDate);
            builder.Property(e => e.ShippedDate);
            builder.Property(e => e.DeliveredDate);
            builder.Property(e => e.CancelledAt);
            
            builder.Property(e => e.CustomerId)
                .IsRequired();
                
            builder.Property(e => e.DeliveryAddressId);
            builder.Property(e => e.BillingAddressId);

            builder.OwnsOne(o => o.OrderNumber, orderNumber => BaseValueObjectConfiguration.ConfigureOrderNumber(orderNumber));
            builder.OwnsOne(o => o.Subtotal, subtotal => BaseValueObjectConfiguration.ConfigureMoney(subtotal, "Subtotal"));
            builder.OwnsOne(o => o.TaxAmount, tax => BaseValueObjectConfiguration.ConfigureMoney(tax, "TaxAmount"));
            builder.OwnsOne(o => o.ShippingAmount, shipping => BaseValueObjectConfiguration.ConfigureMoney(shipping, "ShippingAmount"));
            builder.OwnsOne(o => o.DiscountAmount, discount => BaseValueObjectConfiguration.ConfigureMoney(discount, "DiscountAmount"));
            builder.OwnsOne(o => o.TotalAmount, total => BaseValueObjectConfiguration.ConfigureMoney(total, "TotalAmount"));
            
            // Relacionamentos
            builder.HasOne(e => e.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
                
            builder.HasOne(e => e.DeliveryAddress)
                .WithMany()
                .HasForeignKey(e => e.DeliveryAddressId)
                .OnDelete(DeleteBehavior.NoAction);
                
            builder.HasOne(e => e.BillingAddress)
                .WithMany()
                .HasForeignKey(e => e.BillingAddressId)
                .OnDelete(DeleteBehavior.NoAction);
                
            builder.HasMany(e => e.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
                
            builder.HasMany(e => e.Payments)
                .WithOne(p => p.Order)
                .HasForeignKey(p => p.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Índices
            builder.HasIndex(e => e.CustomerId)
                .HasDatabaseName("IX_Orders_CustomerId");
                
            builder.HasIndex(e => e.Status)
                .HasDatabaseName("IX_Orders_Status");
                
            builder.HasIndex(e => e.CreatedAt)
                .HasDatabaseName("IX_Orders_CreatedAt");
        }
    }
}