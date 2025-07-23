using OrderMediatR.Common;

namespace OrderMediatR.Domain.Entities
{
    public class Address : BaseEntity
    {
        public string Street { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
        public string? Complement { get; set; }
        public string Neighborhood { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
        public string Country { get; set; } = "Brasil";
        public string AddressType { get; set; } = string.Empty;
        public bool IsDefault { get; set; } = false;

        public Guid CustomerId { get; set; }

        public virtual Customer Customer { get; set; } = null!;
    }
}