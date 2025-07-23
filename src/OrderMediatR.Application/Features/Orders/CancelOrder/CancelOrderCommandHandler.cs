using MediatR;
using OrderMediatR.Domain.Entities;

namespace OrderMediatR.Application.Features.Orders.CancelOrder
{
    public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, CancelOrderCommandResponse>
    {
        private readonly IOrderRepository _orderRepository;

        public CancelOrderCommandHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<CancelOrderCommandResponse> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetByIdAsync(request.Id);
            if (order == null)
            {
                throw new NotFoundException($"Pedido com ID {request.Id} não encontrado.");
            }

            order.Cancel(request.Reason);
            await _orderRepository.UpdateAsync(order);

            return new CancelOrderCommandResponse
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber.Value,
                Status = order.Status.ToString(),
                Reason = request.Reason,
                CancelledAt = order.CancelledAt ?? DateTime.UtcNow
            };
        }
    }

    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
    }

    public interface IOrderRepository
    {
        Task<Order?> GetByIdAsync(Guid id);
        Task UpdateAsync(Order order);
    }
}