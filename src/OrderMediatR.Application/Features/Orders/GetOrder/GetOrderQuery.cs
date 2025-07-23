using MediatR;

namespace OrderMediatR.Application.Features.Orders.GetOrder
{
    public class GetOrderQuery : IRequest<GetOrderQueryResponse>
    {
        public Guid Id { get; set; }
    }
}