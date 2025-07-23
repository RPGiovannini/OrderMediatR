using MediatR;
using OrderMediatR.Domain.Entities;

namespace OrderMediatR.Application.Features.Customers.GetCustomers
{
    public class GetCustomersQueryHandler : IRequestHandler<GetCustomersQuery, GetCustomersQueryResponse>
    {
        private readonly ICustomerRepository _customerRepository;

        public GetCustomersQueryHandler(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<GetCustomersQueryResponse> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
        {
            var customers = await _customerRepository.GetCustomersAsync(
                request.SearchTerm,
                request.Page,
                request.PageSize,
                request.SortBy,
                request.IsDescending
            );

            var totalCount = await _customerRepository.GetTotalCountAsync(request.SearchTerm);
            var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

            return new GetCustomersQueryResponse
            {
                Customers = customers.Select(c => new CustomerDto
                {
                    Id = c.Id,
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    FullName = c.FullName,
                    Email = c.Email.Value,
                    Phone = c.Phone.Value,
                    DocumentNumber = c.DocumentNumber,
                    CreatedAt = c.CreatedAt,
                    TotalOrders = c.TotalOrders
                }).ToList(),
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = totalPages,
                HasNextPage = request.Page < totalPages,
                HasPreviousPage = request.Page > 1
            };
        }
    }

    public interface ICustomerRepository
    {
        Task<List<Customer>> GetCustomersAsync(
            string? searchTerm,
            int page,
            int pageSize,
            string? sortBy,
            bool isDescending
        );
        Task<int> GetTotalCountAsync(string? searchTerm);
    }
}