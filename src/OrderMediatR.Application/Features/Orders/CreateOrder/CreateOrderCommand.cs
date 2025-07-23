using MediatR;

namespace OrderMediatR.Application.Features.Orders.CreateOrder
{
    public class CreateOrderCommand : IRequest<CreateOrderCommandResponse>
    {
        public Guid CustomerId { get; set; }
        public Guid? DeliveryAddressId { get; set; }
        public Guid? BillingAddressId { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
        public decimal? ShippingAmount { get; set; }
        public decimal? DiscountAmount { get; set; }
        public string? Notes { get; set; }
    }

    public class OrderItemDto
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class CreateOrderCommandResponse
    {
        public Guid Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
    }
}