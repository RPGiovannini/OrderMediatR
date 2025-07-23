using MediatR;

namespace OrderMediatR.Application.Features.Products.CreateProduct
{
    public class CreateProductCommand : IRequest<CreateProductCommandResponse>
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string Category { get; set; } = string.Empty;
        public string? Brand { get; set; }
        public decimal Weight { get; set; }
        public string? ImageUrl { get; set; }
    }
}