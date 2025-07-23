using MediatR;
using OrderMediatR.Domain.Entities;
using OrderMediatR.Domain.ValueObjects;

namespace OrderMediatR.Application.Features.Customers.UpdateCustomer
{
    public class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, UpdateCustomerCommandResponse>
    {
        private readonly ICustomerRepository _customerRepository;

        public UpdateCustomerCommandHandler(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<UpdateCustomerCommandResponse> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
        {
            var customer = await _customerRepository.GetByIdAsync(request.Id);
            if (customer == null)
            {
                throw new NotFoundException($"Cliente com ID {request.Id} não encontrado.");
            }

            var email = Email.Create(request.Email);
            var phone = Phone.Create(request.Phone);

            customer.UpdatePersonalInfo(
                request.FirstName,
                request.LastName,
                email,
                phone,
                request.DocumentNumber,
                request.DateOfBirth
            );

            await _customerRepository.UpdateAsync(customer);

            return new UpdateCustomerCommandResponse
            {
                Id = customer.Id,
                FullName = customer.FullName,
                Email = customer.Email.Value,
                UpdatedAt = customer.UpdatedAt ?? DateTime.UtcNow
            };
        }
    }

    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
    }

    public interface ICustomerRepository
    {
        Task<Customer?> GetByIdAsync(Guid id);
        Task UpdateAsync(Customer customer);
    }
}