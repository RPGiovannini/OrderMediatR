using OrderMediatR.Domain.Entities;

namespace OrderMediatR.Application.Interfaces;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id);
    Task<List<Product>> GetProductsAsync(
        string? searchTerm,
        string? category,
        string? brand,
        decimal? minPrice,
        decimal? maxPrice,
        bool? isAvailable,
        int page,
        int pageSize,
        string? sortBy,
        bool isDescending
    );
    Task<int> GetTotalCountAsync(
        string? searchTerm,
        string? category,
        string? brand,
        decimal? minPrice,
        decimal? maxPrice,
        bool? isAvailable
    );
    Task AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(Guid id);
}