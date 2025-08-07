using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderMediatR.Domain.Entities;
using OrderMediatR.Domain.ValueObjects;
using OrderMediatR.Infra.Context;
using OrderMediatR.SyncWorker.Interfaces;
using OrderMediatR.SyncWorker.Messages;

namespace OrderMediatR.SyncWorker.Handlers
{
    public class OrderMessageHandler : IMessageHandler<OrderMessage>
    {
        private readonly ReadContext _readContext;
        private readonly ILogger<OrderMessageHandler> _logger;

        public OrderMessageHandler(
            ReadContext readContext,
            ILogger<OrderMessageHandler> logger)
        {
            _readContext = readContext;
            _logger = logger;
        }

        public async Task HandleAsync(OrderMessage message, CancellationToken cancellationToken)
        {
            var existing = await _readContext.Orders
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

        private async Task HandleCreate(OrderMessage message, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Criando novo pedido via sync: {OrderId}", message.Id);

            var order = Order.FromSync(
                id: message.Id,
                customerId: message.CustomerId,
                orderNumber: OrderNumber.FromValue(message.OrderNumber),
                status: message.Status,
                subtotal: new Money(message.SubtotalAmount, message.SubtotalCurrency),
                taxAmount: new Money(message.TaxAmount, message.TaxAmountCurrency),
                shippingAmount: new Money(message.ShippingAmount, message.ShippingAmountCurrency),
                discountAmount: new Money(message.DiscountAmount, message.DiscountAmountCurrency),
                totalAmount: new Money(message.TotalAmount, message.TotalAmountCurrency),
                createdAt: message.CreatedAt,
                updatedAt: message.UpdatedAt,
                isActive: message.IsActive
            );

            await _readContext.Orders.AddAsync(order, cancellationToken);
            await _readContext.SaveChangesAsync(cancellationToken);
            
            _logger.LogDebug("Pedido criado via sync: {OrderId}", message.Id);
        }

        private async Task HandleUpdate(OrderMessage message, Order existing, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Atualizando pedido via sync: {OrderId}", message.Id);

            existing.UpdateFromSync(
                status: message.Status,
                subtotal: new Money(message.SubtotalAmount, message.SubtotalCurrency),
                taxAmount: new Money(message.TaxAmount, message.TaxAmountCurrency),
                shippingAmount: new Money(message.ShippingAmount, message.ShippingAmountCurrency),
                discountAmount: new Money(message.DiscountAmount, message.DiscountAmountCurrency),
                totalAmount: new Money(message.TotalAmount, message.TotalAmountCurrency),
                notes: message.Notes,
                estimatedDeliveryDate: message.EstimatedDeliveryDate,
                shippedDate: message.ShippedDate,
                deliveredDate: message.DeliveredDate,
                cancelledAt: message.CancelledAt,
                updatedAt: message.UpdatedAt,
                isActive: message.IsActive
            );

            await _readContext.SaveChangesAsync(cancellationToken);
            
            _logger.LogDebug("Pedido atualizado via sync: {OrderId}", message.Id);
        }
    }
}