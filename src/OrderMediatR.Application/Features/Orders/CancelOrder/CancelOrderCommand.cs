using MediatR;

namespace OrderMediatR.Application.Features.Orders.CancelOrder
{
    public class CancelOrderCommand : IRequest<CancelOrderCommandResponse>
    {
        public Guid Id { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}