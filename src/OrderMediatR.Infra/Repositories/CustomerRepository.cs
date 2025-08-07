using Microsoft.EntityFrameworkCore;
using OrderMediatR.Application.Interfaces;
using OrderMediatR.Domain.Entities;
using OrderMediatR.Infra.Context;

namespace OrderMediatR.Infra.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly WriteContext _writeContext;
    private readonly ReadContext _readContext;

    public CustomerRepository(WriteContext writeContext, ReadContext readContext)
    {
        _writeContext = writeContext;
        _readContext = readContext;
    }

    public async Task<Customer?> GetByIdAsync(Guid id)
    {
        return await _readContext.Customers
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Customer?> GetByIdWithAddressesAsync(Guid id)
    {
        return await _readContext.Customers
            .Include(c => c.Addresses)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<List<Customer>> GetCustomersAsync(
        string? searchTerm,
        int page,
        int pageSize,
        string? sortBy,
        bool isDescending)
    {
        var query = _readContext.Customers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(c =>
                c.FirstName.Contains(searchTerm) ||
                c.LastName.Contains(searchTerm) ||
                c.Email.Value.Contains(searchTerm));
        }

        query = sortBy?.ToLower() switch
        {
            "firstname" => isDescending ? query.OrderByDescending(c => c.FirstName) : query.OrderBy(c => c.FirstName),
            "lastname" => isDescending ? query.OrderByDescending(c => c.LastName) : query.OrderBy(c => c.LastName),
            "email" => isDescending ? query.OrderByDescending(c => c.Email.Value) : query.OrderBy(c => c.Email.Value),
            "createdat" => isDescending ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt),
            _ => query.OrderBy(c => c.FirstName)
        };

        return await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetTotalCountAsync(string? searchTerm)
    {
        var query = _readContext.Customers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(c =>
                c.FirstName.Contains(searchTerm) ||
                c.LastName.Contains(searchTerm) ||
                c.Email.Value.Contains(searchTerm));
        }

        return await query.CountAsync();
    }

    public async Task AddAsync(Customer customer)
    {
        await _writeContext.Customers.AddAsync(customer);
        await _writeContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(Customer customer)
    {
        _writeContext.Customers.Update(customer);
        await _writeContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var customer = await _writeContext.Customers.FirstOrDefaultAsync(c => c.Id == id);
        if (customer != null)
        {
            _writeContext.Customers.Remove(customer);
            await _writeContext.SaveChangesAsync();
        }
    }
}