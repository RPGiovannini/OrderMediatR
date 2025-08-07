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
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}