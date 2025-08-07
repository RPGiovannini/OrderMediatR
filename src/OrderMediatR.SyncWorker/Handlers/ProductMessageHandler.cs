using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderMediatR.Domain.Entities;
using OrderMediatR.Domain.ValueObjects;
using OrderMediatR.Infra.Context;
using OrderMediatR.SyncWorker.Interfaces;
using OrderMediatR.SyncWorker.Messages;

namespace OrderMediatR.SyncWorker.Handlers
{
    public class ProductMessageHandler : IMessageHandler<ProductMessage>
    {
        private readonly ReadContext _readContext;
        private readonly ILogger<ProductMessageHandler> _logger;

        public ProductMessageHandler(
            ReadContext readContext,
            ILogger<ProductMessageHandler> logger)
        {
            _readContext = readContext;
            _logger = logger;
        }

        public async Task HandleAsync(ProductMessage message, CancellationToken cancellationToken)
        {
            var existing = await _readContext.Products
                .FirstOrDefaultAsync(e => e.Id == message.Id, cancellationToken);

            if (existing == null)
            {
                await HandleCreate(message, cancellationToken);
            }
            else
            {
                await HandleUpdate(message, existing, cancellationToken);
            }
        }

        private async Task HandleCreate(ProductMessage message, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Criando novo produto via sync: {ProductId}", message.Id);

            var product = Product.FromSync(
                id: message.Id,
                name: message.Name,
                description: message.Description,
                sku: new Sku(message.Sku),
                price: new Money(message.Price, message.Currency),
                stockQuantity: message.StockQuantity,
                category: message.Category,
                brand: message.Brand,
                weight: message.Weight,
                imageUrl: message.ImageUrl,
                isAvailable: message.IsAvailable,
                createdAt: message.CreatedAt,
                updatedAt: message.UpdatedAt,
                isActive: message.IsActive
            );

            await _readContext.Products.AddAsync(product, cancellationToken);
            await _readContext.SaveChangesAsync(cancellationToken);
            
            _logger.LogDebug("Produto criado via sync: {ProductId}", message.Id);
        }

        private async Task HandleUpdate(ProductMessage message, Product existing, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Atualizando produto via sync: {ProductId}", message.Id);

            existing.UpdateFromSync(
                name: message.Name,
                description: message.Description,
                price: new Money(message.Price, message.Currency),
                stockQuantity: message.StockQuantity,
                category: message.Category,
                brand: message.Brand,
                weight: message.Weight,
                imageUrl: message.ImageUrl,
                isAvailable: message.IsAvailable,
                updatedAt: message.UpdatedAt,
                isActive: message.IsActive
            );

            await _readContext.SaveChangesAsync(cancellationToken);
            
            _logger.LogDebug("Produto atualizado via sync: {ProductId}", message.Id);
        }
    }
}