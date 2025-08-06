using MediatR;
using OrderMediatR.Application.Interfaces;
using OrderMediatR.Domain.Entities;

namespace OrderMediatR.Application.Features.Customers.GetCustomer
{
    public class GetCustomerQueryHandler : IRequestHandler<GetCustomerQuery, GetCustomerQueryResponse>
    {
        private readonly ICustomerRepository _customerRepository;

        public GetCustomerQueryHandler(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<GetCustomerQueryResponse> Handle(GetCustomerQuery request, CancellationToken cancellationToken)
        {
            var customer = await _customerRepository.GetByIdWithAddressesAsync(request.Id);
            if (customer == null)
            {
                throw new NotFoundException($"Cliente com ID {request.Id} não encontrado.");
            }

            return new GetCustomerQueryResponse
            {
                Id = customer.Id,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                FullName = customer.FullName,
                Email = customer.Email.Value,
                Phone = customer.Phone.Value,
                DocumentNumber = customer.DocumentNumber,
                DateOfBirth = customer.DateOfBirth,
                CreatedAt = customer.CreatedAt,
                UpdatedAt = customer.UpdatedAt,
                Addresses = customer.Addresses.Select(a => new CustomerAddressDto
                {
                    Id = a.Id,
                    Street = a.Street,
                    Number = a.Number,
                    Complement = a.Complement,
                    Neighborhood = a.Neighborhood,
                    City = a.City,
                    State = a.State,
                    ZipCode = a.ZipCode,
                    AddressType = a.AddressType.ToString(),
                    IsDefault = a.IsDefault
                }).ToList()
            };
        }
    }

    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
    }


}