using MediatR;

namespace OrderMediatR.Application.Features.Customers.UpdateCustomer
{
    public class UpdateCustomerCommand : IRequest<UpdateCustomerCommandResponse>
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? DocumentNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
    }
}