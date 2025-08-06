using MediatR;
using OrderMediatR.Application.Interfaces;
using OrderMediatR.Domain.Entities;
using OrderMediatR.Domain.ValueObjects;

namespace OrderMediatR.Application.Features.Customers.CreateCustomer
{
    public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, CreateCustomerCommandResponse>
    {
        private readonly ICustomerRepository _customerRepository;

        public CreateCustomerCommandHandler(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<CreateCustomerCommandResponse> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
        {
            // FluentValidation já validou os dados de entrada
            // Agora criamos a entidade com as regras de domínio

            var email = new Email(request.Email);
            var phone = new Phone(request.Phone);
            var customer = new Customer(
                request.FirstName,
                request.LastName,
                email,
                phone
            );

            if (!string.IsNullOrWhiteSpace(request.DocumentNumber))
            {
                customer.SetDocumentNumber(request.DocumentNumber);
            }

            if (request.DateOfBirth.HasValue)
            {
                customer.SetDateOfBirth(request.DateOfBirth.Value);
            }

            await _customerRepository.AddAsync(customer);

            return new CreateCustomerCommandResponse { Id = customer.Id };
        }
    }
}