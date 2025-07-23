using OrderMediatR.Common;

namespace OrderMediatR.Domain.Entities
{
    public enum PaymentStatus
    {
        Pending = 1,
        Processing = 2,
        Approved = 3,
        Declined = 4,
        Refunded = 5,
        Cancelled = 6
    }

    public enum PaymentMethod
    {
        CreditCard = 1,
        DebitCard = 2,
        BankTransfer = 3,
        PIX = 4,
        Boleto = 5,
        Cash = 6
    }

    public class Payment : BaseEntity
    {
        public decimal Amount { get; set; }
        public PaymentMethod Method { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public string? TransactionId { get; set; }
        public string? CardLastFourDigits { get; set; }
        public string? CardBrand { get; set; }
        public int? Installments { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string? FailureReason { get; set; }

        public Guid OrderId { get; set; }

        public virtual Order Order { get; set; } = null!;
    }
}