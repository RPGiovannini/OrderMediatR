using Microsoft.EntityFrameworkCore;
using OrderMediatR.Domain.Entities;

namespace OrderMediatR.Domain.Context;

public interface IOrderMediatRContext
{
    DbSet<Order> Orders { get; set; }
    DbSet<OrderItem> OrderItems { get; set; }
    DbSet<Customer> Customers { get; set; }
    DbSet<Product> Products { get; set; }
    DbSet<Payment> Payments { get; set; }
    DbSet<Address> Addresses { get; set; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}