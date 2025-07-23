using MediatR;

namespace OrderMediatR.Application.Features.Customers.GetCustomer
{
    public class GetCustomerQuery : IRequest<GetCustomerQueryResponse>
    {
        public Guid Id { get; set; }
    }
}