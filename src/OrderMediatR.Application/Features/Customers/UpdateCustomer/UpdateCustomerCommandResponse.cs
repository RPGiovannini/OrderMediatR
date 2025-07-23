namespace OrderMediatR.Application.Features.Customers.UpdateCustomer
{
    public class UpdateCustomerCommandResponse
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }
    }
}