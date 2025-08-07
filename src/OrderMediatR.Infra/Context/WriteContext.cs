using System.Reflection;
using Microsoft.EntityFrameworkCore;
using OrderMediatR.Common;
using OrderMediatR.Domain.Entities;

namespace OrderMediatR.Infra.Context
{
    public class WriteContext : DbContext
    {
        public WriteContext(DbContextOptions<WriteContext> options) : base(options)
        {
        }

        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<OutboxEvent> OutboxEvents { get; set; } 

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {

            var entitiesWithEvents = ChangeTracker
                .Entries()
                .Where(e => e.Entity is BaseEntity baseEntity && baseEntity.DomainEvents?.Any() == true)
                .Select(e => (BaseEntity)e.Entity)
                .ToList();

            foreach (var entity in entitiesWithEvents)
            {
                var events = entity.DomainEvents!.ToList();
                entity.ClearDomainEvents();

                foreach (var domainEvent in events)
                {
                    var outboxEvent = OutboxEvent.Create(domainEvent.GetType(), domainEvent);
                    await OutboxEvents.AddAsync(outboxEvent, cancellationToken);
                }
            }
            return await base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureValueObjects(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

        private static void ConfigureValueObjects(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.OwnsOne(c => c.Email, email =>
                {
                    email.Property(e => e.Value)
                        .HasColumnName("Email")
                        .HasMaxLength(255)
                        .IsRequired();
                });

                entity.OwnsOne(c => c.Phone, phone =>
                {
                    phone.Property(p => p.Value)
                        .HasColumnName("Phone")
                        .HasMaxLength(20)
                        .IsRequired();
                });
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.OwnsOne(p => p.Price, price =>
                {
                    price.Property(m => m.Amount)
                        .HasColumnName("Price")
                        .HasColumnType("decimal(18,2)")
                        .IsRequired();

                    price.Property(m => m.Currency)
                        .HasColumnName("Currency")
                        .HasMaxLength(3)
                        .IsRequired();
                });

                entity.OwnsOne(p => p.Sku, sku =>
                {
                    sku.Property(s => s.Value)
                        .HasColumnName("Sku")
                        .HasMaxLength(50)
                        .IsRequired();
                });
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.OwnsOne(o => o.OrderNumber, orderNumber =>
                {
                    orderNumber.Property(on => on.Value)
                        .HasColumnName("OrderNumber")
                        .HasMaxLength(50)
                        .IsRequired();
                });

                entity.OwnsOne(o => o.Subtotal, money =>
                {
                    money.Property(m => m.Amount)
                        .HasColumnName("Subtotal")
                        .HasColumnType("decimal(18,2)")
                        .IsRequired();
                    money.Property(m => m.Currency)
                        .HasColumnName("SubtotalCurrency")
                        .HasMaxLength(3)
                        .IsRequired();
                });

                entity.OwnsOne(o => o.TaxAmount, money =>
                {
                    money.Property(m => m.Amount)
                        .HasColumnName("TaxAmount")
                        .HasColumnType("decimal(18,2)")
                        .IsRequired();
                    money.Property(m => m.Currency)
                        .HasColumnName("TaxAmountCurrency")
                        .HasMaxLength(3)
                        .IsRequired();
                });

                entity.OwnsOne(o => o.ShippingAmount, money =>
                {
                    money.Property(m => m.Amount)
                        .HasColumnName("ShippingAmount")
                        .HasColumnType("decimal(18,2)")
                        .IsRequired();
                    money.Property(m => m.Currency)
                        .HasColumnName("ShippingAmountCurrency")
                        .HasMaxLength(3)
                        .IsRequired();
                });

                entity.OwnsOne(o => o.DiscountAmount, money =>
                {
                    money.Property(m => m.Amount)
                        .HasColumnName("DiscountAmount")
                        .HasColumnType("decimal(18,2)")
                        .IsRequired();
                    money.Property(m => m.Currency)
                        .HasColumnName("DiscountAmountCurrency")
                        .HasMaxLength(3)
                        .IsRequired();
                });

                entity.OwnsOne(o => o.TotalAmount, money =>
                {
                    money.Property(m => m.Amount)
                        .HasColumnName("TotalAmount")
                        .HasColumnType("decimal(18,2)")
                        .IsRequired();
                    money.Property(m => m.Currency)
                        .HasColumnName("TotalAmountCurrency")
                        .HasMaxLength(3)
                        .IsRequired();
                });
            });

            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.OwnsOne(oi => oi.UnitPrice, money =>
                {
                    money.Property(m => m.Amount)
                        .HasColumnName("UnitPrice")
                        .HasColumnType("decimal(18,2)")
                        .IsRequired();
                    money.Property(m => m.Currency)
                        .HasColumnName("UnitPriceCurrency")
                        .HasMaxLength(3)
                        .IsRequired();
                });

                entity.OwnsOne(oi => oi.TotalPrice, money =>
                {
                    money.Property(m => m.Amount)
                        .HasColumnName("TotalPrice")
                        .HasColumnType("decimal(18,2)")
                        .IsRequired();
                    money.Property(m => m.Currency)
                        .HasColumnName("TotalPriceCurrency")
                        .HasMaxLength(3)
                        .IsRequired();
                });

                entity.OwnsOne(oi => oi.DiscountAmount, money =>
                {
                    money.Property(m => m.Amount)
                        .HasColumnName("DiscountAmount")
                        .HasColumnType("decimal(18,2)");
                    money.Property(m => m.Currency)
                        .HasColumnName("DiscountAmountCurrency")
                        .HasMaxLength(3);
                });
            });
        }
    }
}