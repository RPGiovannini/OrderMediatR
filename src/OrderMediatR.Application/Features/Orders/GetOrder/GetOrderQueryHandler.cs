using MediatR;
using OrderMediatR.Application.Interfaces;
using OrderMediatR.Domain.Entities;

namespace OrderMediatR.Application.Features.Orders.GetOrder
{
    public class GetOrderQueryHandler : IRequestHandler<GetOrderQuery, GetOrderQueryResponse>
    {
        private readonly IOrderRepository _orderRepository;

        public GetOrderQueryHandler(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<GetOrderQueryResponse> Handle(GetOrderQuery request, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetByIdWithDetailsAsync(request.Id);
            if (order == null)
            {
                throw new NotFoundException($"Pedido com ID {request.Id} não encontrado.");
            }

            return new GetOrderQueryResponse
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber.Value,
                Status = order.Status.ToString(),
                Subtotal = order.Subtotal.Amount,
                TaxAmount = order.TaxAmount.Amount,
                ShippingAmount = order.ShippingAmount.Amount,
                DiscountAmount = order.DiscountAmount.Amount,
                TotalAmount = order.TotalAmount.Amount,
                Notes = order.Notes,
                EstimatedDeliveryDate = order.EstimatedDeliveryDate,
                ShippedDate = order.ShippedDate,
                DeliveredDate = order.DeliveredDate,
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt,
                Customer = new CustomerDto
                {
                    Id = order.Customer.Id,
                    FirstName = order.Customer.FirstName,
                    LastName = order.Customer.LastName,
                    FullName = order.Customer.FullName,
                    Email = order.Customer.Email.Value,
                    Phone = order.Customer.Phone.Value
                },
                DeliveryAddress = order.DeliveryAddress != null ? new AddressDto
                {
                    Id = order.DeliveryAddress.Id,
                    Street = order.DeliveryAddress.Street,
                    Number = order.DeliveryAddress.Number,
                    Complement = order.DeliveryAddress.Complement,
                    Neighborhood = order.DeliveryAddress.Neighborhood,
                    City = order.DeliveryAddress.City,
                    State = order.DeliveryAddress.State,
                    ZipCode = order.DeliveryAddress.ZipCode,
                    AddressType = order.DeliveryAddress.AddressType.ToString()
                } : null,
                BillingAddress = order.BillingAddress != null ? new AddressDto
                {
                    Id = order.BillingAddress.Id,
                    Street = order.BillingAddress.Street,
                    Number = order.BillingAddress.Number,
                    Complement = order.BillingAddress.Complement,
                    Neighborhood = order.BillingAddress.Neighborhood,
                    City = order.BillingAddress.City,
                    State = order.BillingAddress.State,
                    ZipCode = order.BillingAddress.ZipCode,
                    AddressType = order.BillingAddress.AddressType.ToString()
                } : null,
                OrderItems = order.OrderItems.Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice.Amount,
                    TotalPrice = oi.TotalPrice.Amount,
                    DiscountAmount = oi.DiscountAmount?.Amount,
                    Notes = oi.Notes,
                    Product = new ProductDto
                    {
                        Id = oi.Product.Id,
                        Name = oi.Product.Name,
                        Sku = oi.Product.Sku.Value,
                        ImageUrl = oi.Product.ImageUrl
                    }
                }).ToList(),
                Payments = order.Payments.Select(p => new PaymentDto
                {
                    Id = p.Id,
                    Amount = p.Amount,
                    Method = p.Method.ToString(),
                    Status = p.Status.ToString(),
                    TransactionId = p.TransactionId,
                    CardLastFourDigits = p.CardLastFourDigits,
                    CardBrand = p.CardBrand,
                    Installments = p.Installments,
                    ProcessedAt = p.ProcessedAt
                }).ToList()
            };
        }
    }

    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
    }


}