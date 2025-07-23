using MediatR;

namespace OrderMediatR.Application.Features.Customers.GetCustomers
{
    public class GetCustomersQuery : IRequest<GetCustomersQueryResponse>
    {
        public string? SearchTerm { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; }
        public bool IsDescending { get; set; } = false;
    }
}