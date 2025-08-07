using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderMediatR.Domain.Entities;

namespace OrderMediatR.Infra.EntityConfigurations
{
    public class CustomerConfiguration : BaseEntityConfiguration<Customer>
    {
        public override void Configure(EntityTypeBuilder<Customer> builder)
        {
            base.Configure(builder);
            
            builder.ToTable("Customers");
            
            builder.Property(e => e.FirstName)
                .HasMaxLength(100)
                .IsRequired();
                
            builder.Property(e => e.LastName)
                .HasMaxLength(100)
                .IsRequired();
                
            builder.Property(e => e.DocumentNumber)
                .HasMaxLength(20);
                
            builder.Property(e => e.DateOfBirth);
            
            // Relacionamentos
            builder.HasMany(e => e.Addresses)
                .WithOne(a => a.Customer)
                .HasForeignKey(a => a.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
                
            builder.HasMany(e => e.Orders)
                .WithOne(o => o.Customer)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // Índices
            builder.HasIndex(e => e.DocumentNumber)
                .HasDatabaseName("IX_Customers_DocumentNumber")
                .IsUnique()
                .HasFilter("[DocumentNumber] IS NOT NULL");
                
            builder.HasIndex(e => new { e.FirstName, e.LastName })
                .HasDatabaseName("IX_Customers_FullName");
        }
    }
}