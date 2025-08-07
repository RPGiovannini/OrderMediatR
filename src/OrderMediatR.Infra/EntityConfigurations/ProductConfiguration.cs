using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderMediatR.Domain.Entities;

namespace OrderMediatR.Infra.EntityConfigurations
{
    public class ProductConfiguration : BaseEntityConfiguration<Product>
    {
        public override void Configure(EntityTypeBuilder<Product> builder)
        {
            base.Configure(builder);
            
            builder.ToTable("Products");
            
            builder.Property(e => e.Name)
                .HasMaxLength(200)
                .IsRequired();
                
            builder.Property(e => e.Description)
                .HasMaxLength(1000)
                .IsRequired();
                
            builder.Property(e => e.Category)
                .HasMaxLength(100)
                .IsRequired();
                
            builder.Property(e => e.Brand)
                .HasMaxLength(100);
                
            builder.Property(e => e.Weight)
                .HasColumnType("decimal(10,3)")
                .IsRequired();
                
            builder.Property(e => e.ImageUrl)
                .HasMaxLength(500);
                
            builder.Property(e => e.IsAvailable)
                .IsRequired()
                .HasDefaultValue(true);
                
            builder.Property(e => e.StockQuantity)
                .IsRequired();
            
            // Relacionamentos
            builder.HasMany(e => e.OrderItems)
                .WithOne(oi => oi.Product)
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // Índices
            builder.HasIndex(e => e.Name)
                .HasDatabaseName("IX_Products_Name");
                
            builder.HasIndex(e => e.Category)
                .HasDatabaseName("IX_Products_Category");
                
            builder.HasIndex(e => e.IsAvailable)
                .HasDatabaseName("IX_Products_IsAvailable");
                
            builder.HasIndex(e => e.StockQuantity)
                .HasDatabaseName("IX_Products_StockQuantity");
        }
    }
}