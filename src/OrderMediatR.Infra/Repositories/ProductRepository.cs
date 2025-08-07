using Microsoft.EntityFrameworkCore;
using OrderMediatR.Application.Interfaces;
using OrderMediatR.Domain.Entities;
using OrderMediatR.Infra.Context;

namespace OrderMediatR.Infra.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly WriteContext _writeContext;
    private readonly ReadContext _readContext;

    public ProductRepository(WriteContext writeContext, ReadContext readContext)
    {
        _writeContext = writeContext;
        _readContext = readContext;
    }

    public async Task<Product?> GetByIdAsync(Guid id)
    {
        return await _readContext.Products
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<List<Product>> GetProductsAsync(
        string? searchTerm,
        string? category,
        string? brand,
        decimal? minPrice,
        decimal? maxPrice,
        bool? isAvailable,
        int page,
        int pageSize,
        string? sortBy,
        bool isDescending)
    {
        var query = _readContext.Products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(p =>
                p.Name.Contains(searchTerm) ||
                p.Description.Contains(searchTerm) ||
                p.Sku.Value.Contains(searchTerm));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(p => p.Category == category);
        }

        if (!string.IsNullOrWhiteSpace(brand))
        {
            query = query.Where(p => p.Brand == brand);
        }

        if (minPrice.HasValue)
        {
            query = query.Where(p => p.Price.Amount >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(p => p.Price.Amount <= maxPrice.Value);
        }

        if (isAvailable.HasValue)
        {
            query = query.Where(p => p.IsAvailable == isAvailable.Value);
        }

        query = sortBy?.ToLower() switch
        {
            "name" => isDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            "price" => isDescending ? query.OrderByDescending(p => p.Price.Amount) : query.OrderBy(p => p.Price.Amount),
            "category" => isDescending ? query.OrderByDescending(p => p.Category) : query.OrderBy(p => p.Category),
            "createdat" => isDescending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),
            _ => query.OrderBy(p => p.Name)
        };

        return await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetTotalCountAsync(
        string? searchTerm,
        string? category,
        string? brand,
        decimal? minPrice,
        decimal? maxPrice,
        bool? isAvailable)
    {
        var query = _readContext.Products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(p =>
                p.Name.Contains(searchTerm) ||
                p.Description.Contains(searchTerm) ||
                p.Sku.Value.Contains(searchTerm));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(p => p.Category == category);
        }

        if (!string.IsNullOrWhiteSpace(brand))
        {
            query = query.Where(p => p.Brand == brand);
        }

        if (minPrice.HasValue)
        {
            query = query.Where(p => p.Price.Amount >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(p => p.Price.Amount <= maxPrice.Value);
        }

        if (isAvailable.HasValue)
        {
            query = query.Where(p => p.IsAvailable == isAvailable.Value);
        }

        return await query.CountAsync();
    }

    public async Task AddAsync(Product product)
    {
        await _writeContext.Products.AddAsync(product);
        await _writeContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(Product product)
    {
        _writeContext.Products.Update(product);
        await _writeContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var product = await _writeContext.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (product != null)
        {
            _writeContext.Products.Remove(product);
            await _writeContext.SaveChangesAsync();
        }
    }
}