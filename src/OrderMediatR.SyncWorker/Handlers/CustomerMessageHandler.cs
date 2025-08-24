using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OrderMediatR.Domain.Entities;
using OrderMediatR.Domain.ValueObjects;
using OrderMediatR.Infra.Context;
using OrderMediatR.SyncWorker.Interfaces;
using OrderMediatR.SyncWorker.Messages;

namespace OrderMediatR.SyncWorker.Handlers
{
    public class CustomerMessageHandler : IMessageHandler<CustomerMessage>
    {
        private readonly ReadContext _readContext;
        private readonly ILogger<CustomerMessageHandler> _logger;

        public CustomerMessageHandler(
            ReadContext readContext,
            ILogger<CustomerMessageHandler> logger)
        {
            _readContext = readContext;
            _logger = logger;
        }

        public async Task HandleAsync(CustomerMessage message, CancellationToken cancellationToken)
        {
            var existing = await _readContext.Customers
                .FirstOrDefaultAsync(e => e.Id == message.Id, cancellationToken);

            if (existing == null)
            {
                await HandleCreate(message, cancellationToken);
                return;
            }
            await HandleUpdate(message, existing, cancellationToken);
        }

        private async Task HandleCreate(CustomerMessage message, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Criando novo cliente via sync: {CustomerId}", message.Id);

            var customer = Customer.FromSync(
                id: message.Id,
                firstName: message.FirstName,
                lastName: message.LastName,
                email: new Email(message.Email),
                phone: new Phone(message.Phone),
                documentNumber: message.DocumentNumber,
                dateOfBirth: message.DateOfBirth,
                createdAt: message.CreatedAt,
                updatedAt: message.UpdatedAt,
                isActive: message.IsActive
            );

            await _readContext.Customers.AddAsync(customer, cancellationToken);
            await _readContext.SaveChangesAsync(cancellationToken);

            _logger.LogDebug("Cliente criado via sync: {CustomerId}", message.Id);
        }

        private async Task HandleUpdate(CustomerMessage message, Customer existing, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Atualizando cliente via sync: {CustomerId}", message.Id);

            existing.UpdateFromSync(
                firstName: message.FirstName,
                lastName: message.LastName,
                email: new Email(message.Email),
                phone: new Phone(message.Phone),
                documentNumber: message.DocumentNumber,
                dateOfBirth: message.DateOfBirth,
                updatedAt: message.UpdatedAt,
                isActive: message.IsActive
            );

            await _readContext.SaveChangesAsync(cancellationToken);

            _logger.LogDebug("Cliente atualizado via sync: {CustomerId}", message.Id);
        }
    }
}