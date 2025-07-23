using MediatR;

namespace OrderMediatR.Application.Features.Customers.CreateCustomer
{
    public class CreateCustomerCommand : IRequest<CreateCustomerCommandResponse>
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? DocumentNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
    }

    public class CreateCustomerCommandResponse
    {
        public Guid Id { get; set; }
    }
}