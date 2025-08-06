using OrderMediatR.Common;
using OrderMediatR.Domain.ValueObjects;

namespace OrderMediatR.Domain.Entities
{
    public enum OrderStatus
    {
        Pending = 1,
        Confirmed = 2,
        Processing = 3,
        Shipped = 4,
        Delivered = 5,
        Cancelled = 6,
        Returned = 7
    }

    public class Order : BaseEntity
    {
        public OrderNumber OrderNumber { get; private set; }
        public OrderStatus Status { get; private set; } = OrderStatus.Pending;
        public Money Subtotal { get; private set; }
        public Money TaxAmount { get; private set; }
        public Money ShippingAmount { get; private set; }
        public Money DiscountAmount { get; private set; }
        public Money TotalAmount { get; private set; }
        public string? Notes { get; private set; }
        public DateTime? EstimatedDeliveryDate { get; private set; }
        public DateTime? ShippedDate { get; private set; }
        public DateTime? DeliveredDate { get; private set; }
        public DateTime? CancelledAt { get; private set; }

        public Guid CustomerId { get; private set; }
        public Guid? DeliveryAddressId { get; private set; }
        public Guid? BillingAddressId { get; private set; }

        private readonly List<OrderItem> _orderItems = new();
        private readonly List<Payment> _payments = new();

        public virtual Customer Customer { get; private set; } = null!;
        public virtual Address? DeliveryAddress { get; private set; }
        public virtual Address? BillingAddress { get; private set; }
        public IReadOnlyCollection<OrderItem> OrderItems => _orderItems.AsReadOnly();
        public IReadOnlyCollection<Payment> Payments => _payments.AsReadOnly();

        public int TotalItems => _orderItems.Sum(item => item.Quantity);

        protected Order() { }

        public Order(Customer customer, Address? deliveryAddress = null, Address? billingAddress = null)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            CustomerId = customer.Id;
            Customer = customer;
            DeliveryAddressId = deliveryAddress?.Id;
            DeliveryAddress = deliveryAddress;
            BillingAddressId = billingAddress?.Id;
            BillingAddress = billingAddress;

            OrderNumber = OrderNumber.Generate();
            Subtotal = Money.Zero;
            TaxAmount = Money.Zero;
            ShippingAmount = Money.Zero;
            DiscountAmount = Money.Zero;
            TotalAmount = Money.Zero;
        }

        public void AddItem(Product product, int quantity)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (quantity <= 0)
                throw new ArgumentException("Quantidade deve ser maior que zero", nameof(quantity));

            if (!product.IsAvailable)
                throw new InvalidOperationException("Produto não está disponível");

            if (product.StockQuantity < quantity)
                throw new InvalidOperationException("Quantidade solicitada excede o estoque disponível");

            var existingItem = _orderItems.FirstOrDefault(item => item.ProductId == product.Id);

            if (existingItem != null)
            {
                existingItem.UpdateQuantity(existingItem.Quantity + quantity);
            }
            else
            {
                var orderItem = new OrderItem(this, product, quantity);
                _orderItems.Add(orderItem);
            }

            RecalculateTotals();
        }

        public void RemoveItem(Guid productId)
        {
            var item = _orderItems.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                _orderItems.Remove(item);
                RecalculateTotals();
            }
        }

        public void UpdateItemQuantity(Guid productId, int newQuantity)
        {
            var item = _orderItems.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                item.UpdateQuantity(newQuantity);
                RecalculateTotals();
            }
        }

        public void SetShippingAmount(Money shippingAmount)
        {
            ShippingAmount = shippingAmount;
            RecalculateTotals();
        }

        public void SetDiscountAmount(Money discountAmount)
        {
            DiscountAmount = discountAmount;
            RecalculateTotals();
        }

        public void SetTaxAmount(Money taxAmount)
        {
            TaxAmount = taxAmount;
            RecalculateTotals();
        }

        public void Confirm()
        {
            if (Status != OrderStatus.Pending)
                throw new InvalidOperationException("Pedido não está pendente");

            if (!_orderItems.Any())
                throw new InvalidOperationException("Pedido deve ter pelo menos um item");

            Status = OrderStatus.Confirmed;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Process()
        {
            if (Status != OrderStatus.Confirmed)
                throw new InvalidOperationException("Pedido deve estar confirmado para ser processado");

            Status = OrderStatus.Processing;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Ship()
        {
            if (Status != OrderStatus.Processing)
                throw new InvalidOperationException("Pedido deve estar em processamento para ser enviado");

            Status = OrderStatus.Shipped;
            ShippedDate = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deliver()
        {
            if (Status != OrderStatus.Shipped)
                throw new InvalidOperationException("Pedido deve estar enviado para ser entregue");

            Status = OrderStatus.Delivered;
            DeliveredDate = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Cancel(string reason = null)
        {
            if (Status == OrderStatus.Delivered)
                throw new InvalidOperationException("Pedido entregue não pode ser cancelado");

            Status = OrderStatus.Cancelled;
            Notes = reason;
            CancelledAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void AddPayment(Payment payment)
        {
            if (payment == null)
                throw new ArgumentNullException(nameof(payment));

            _payments.Add(payment);
        }

        public void SetEstimatedDeliveryDate(DateTime estimatedDeliveryDate)
        {
            if (estimatedDeliveryDate <= DateTime.UtcNow)
                throw new ArgumentException("Data estimada de entrega deve ser no futuro");

            EstimatedDeliveryDate = estimatedDeliveryDate;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateNotes(string notes)
        {
            Notes = notes;
            UpdatedAt = DateTime.UtcNow;
        }

        private void RecalculateTotals()
        {
            Subtotal = new Money(_orderItems.Sum(item => (decimal)item.TotalPrice));
            TotalAmount = Subtotal + TaxAmount + ShippingAmount - DiscountAmount;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}