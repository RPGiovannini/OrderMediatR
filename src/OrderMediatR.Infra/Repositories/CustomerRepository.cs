using Microsoft.EntityFrameworkCore;
using OrderMediatR.Application.Interfaces;
using OrderMediatR.Domain.Entities;
using OrderMediatR.Infra.Context;

namespace OrderMediatR.Infra.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly OrderMediatRContext _context;

    public CustomerRepository(OrderMediatRContext context)
    {
        _context = context;
    }

    public async Task<Customer?> GetByIdAsync(Guid id)
    {
        return await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Customer?> GetByIdWithAddressesAsync(Guid id)
    {
        return await _context.Customers
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
        var query = _context.Customers.AsQueryable();

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
        var query = _context.Customers.AsQueryable();

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
        await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Customer customer)
    {
        _context.Customers.Update(customer);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var customer = await GetByIdAsync(id);
        if (customer != null)
        {
            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
        }
    }
}