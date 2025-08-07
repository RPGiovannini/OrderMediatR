using Microsoft.EntityFrameworkCore;
using OrderMediatR.Application.Interfaces;
using OrderMediatR.Domain.Entities;
using OrderMediatR.Infra.Context;

namespace OrderMediatR.Infra.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly WriteContext _writeContext;
    private readonly ReadContext _readContext;

    public OrderRepository(WriteContext writeContext, ReadContext readContext)
    {
        _writeContext = writeContext;
        _readContext = readContext;
    }

    public async Task<Order?> GetByIdAsync(Guid id)
    {
        return await _readContext.Orders
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<Order?> GetByIdWithDetailsAsync(Guid id)
    {
        return await _readContext.Orders
            .Include(o => o.Customer)
            .Include(o => o.DeliveryAddress)
            .Include(o => o.BillingAddress)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<List<Order>> GetOrdersAsync(int page, int pageSize)
    {
        return await _readContext.Orders
            .Include(o => o.Customer)
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task AddAsync(Order order)
    {
        await _writeContext.Orders.AddAsync(order);
        await _writeContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(Order order)
    {
        _writeContext.Orders.Update(order);
        await _writeContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var order = await _writeContext.Orders.FirstOrDefaultAsync(o => o.Id == id);
        if (order != null)
        {
            _writeContext.Orders.Remove(order);
            await _writeContext.SaveChangesAsync();
        }
    }
}