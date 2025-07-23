using MediatR;
using OrderMediatR.Domain.Entities;
using OrderMediatR.Domain.ValueObjects;

namespace OrderMediatR.Application.Features.Products.CreateProduct
{
    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, CreateProductCommandResponse>
    {
        private readonly IProductRepository _productRepository;

        public CreateProductCommandHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<CreateProductCommandResponse> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var price = Money.Create(request.Price);
            var sku = Sku.Create(request.Sku);

            var product = Product.Create(
                request.Name,
                request.Description,
                sku,
                price,
                request.StockQuantity,
                request.Category,
                request.Brand,
                request.Weight,
                request.ImageUrl
            );

            await _productRepository.AddAsync(product);

            return new CreateProductCommandResponse
            {
                Id = product.Id,
                Name = product.Name,
                Sku = product.Sku.Value,
                Price = product.Price.Value,
                StockQuantity = product.StockQuantity,
                IsAvailable = product.IsAvailable,
                CreatedAt = product.CreatedAt
            };
        }
    }

    public interface IProductRepository
    {
        Task AddAsync(Product product);
    }
}