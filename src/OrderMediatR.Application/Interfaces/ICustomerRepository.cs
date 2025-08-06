using OrderMediatR.Domain.Entities;

namespace OrderMediatR.Application.Interfaces;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(Guid id);
    Task<Customer?> GetByIdWithAddressesAsync(Guid id);
    Task<List<Customer>> GetCustomersAsync(
        string? searchTerm,
        int page,
        int pageSize,
        string? sortBy,
        bool isDescending
    );
    Task<int> GetTotalCountAsync(string? searchTerm);
    Task AddAsync(Customer customer);
    Task UpdateAsync(Customer customer);
    Task DeleteAsync(Guid id);
}