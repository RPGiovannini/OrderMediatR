using OrderMediatR.Domain.Entities;

namespace OrderMediatR.SyncWorker.Messages
{
    public class OrderMessage
    {
        public Guid Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public OrderStatus Status { get; set; }
        public decimal SubtotalAmount { get; set; }
        public string SubtotalCurrency { get; set; } = "BRL";
        public decimal TaxAmount { get; set; }
        public string TaxAmountCurrency { get; set; } = "BRL";
        public decimal ShippingAmount { get; set; }
        public string ShippingAmountCurrency { get; set; } = "BRL";
        public decimal DiscountAmount { get; set; }
        public string DiscountAmountCurrency { get; set; } = "BRL";
        public decimal TotalAmount { get; set; }
        public string TotalAmountCurrency { get; set; } = "BRL";
        public string? Notes { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }
        public DateTime? ShippedDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public DateTime? CancelledAt { get; set; }
        public Guid CustomerId { get; set; }
        public Guid? DeliveryAddressId { get; set; }
        public Guid? BillingAddressId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}