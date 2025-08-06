using MediatR;
using OrderMediatR.Application.Interfaces;
using OrderMediatR.Domain.Entities;

namespace OrderMediatR.Application.Features.Products.GetProducts
{
    public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, GetProductsQueryResponse>
    {
        private readonly IProductRepository _productRepository;

        public GetProductsQueryHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<GetProductsQueryResponse> Handle(GetProductsQuery request, CancellationToken cancellationToken)
        {
            var products = await _productRepository.GetProductsAsync(
                request.SearchTerm,
                request.Category,
                request.Brand,
                request.MinPrice,
                request.MaxPrice,
                request.IsAvailable,
                request.Page,
                request.PageSize,
                request.SortBy,
                request.IsDescending
            );

            var totalCount = await _productRepository.GetTotalCountAsync(
                request.SearchTerm,
                request.Category,
                request.Brand,
                request.MinPrice,
                request.MaxPrice,
                request.IsAvailable
            );

            var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

            return new GetProductsQueryResponse
            {
                Products = products.Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Sku = p.Sku.Value,
                    Price = p.Price.Amount,
                    StockQuantity = p.StockQuantity,
                    Category = p.Category,
                    Brand = p.Brand,
                    Weight = p.Weight,
                    ImageUrl = p.ImageUrl,
                    IsAvailable = p.IsAvailable,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                }).ToList(),
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = totalPages,
                HasNextPage = request.Page < totalPages,
                HasPreviousPage = request.Page > 1
            };
        }
    }


}