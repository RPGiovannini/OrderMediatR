using OrderMediatR.Common;
using OrderMediatR.Domain.ValueObjects;

namespace OrderMediatR.Domain.Entities
{
    public class OrderItem : BaseEntity
    {
        public int Quantity { get; private set; }
        public Money UnitPrice { get; private set; }
        public Money TotalPrice { get; private set; }
        public Money? DiscountAmount { get; private set; }
        public string? Notes { get; private set; }
        
        public Guid OrderId { get; private set; }
        public Guid ProductId { get; private set; }
        
        public virtual Order Order { get; private set; } = null!;
        public virtual Product Product { get; private set; } = null!;

        protected OrderItem() { }

        public OrderItem(Order order, Product product, int quantity)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (quantity <= 0)
                throw new ArgumentException("Quantidade deve ser maior que zero", nameof(quantity));

            OrderId = order.Id;
            Order = order;
            ProductId = product.Id;
            Product = product;
            Quantity = quantity;
            UnitPrice = new Money(product.Price);
            
            CalculateTotalPrice();
        }

        public void UpdateQuantity(int newQuantity)
        {
            if (newQuantity <= 0)
                throw new ArgumentException("Quantidade deve ser maior que zero", nameof(newQuantity));

            Quantity = newQuantity;
            CalculateTotalPrice();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetDiscountAmount(Money discountAmount)
        {
            if (discountAmount > TotalPrice)
                throw new ArgumentException("Desconto não pode ser maior que o preço total");

            DiscountAmount = discountAmount;
            CalculateTotalPrice();
            UpdatedAt = DateTime.UtcNow;
        }

        public void RemoveDiscount()
        {
            DiscountAmount = null;
            CalculateTotalPrice();
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateNotes(string notes)
        {
            Notes = notes;
            UpdatedAt = DateTime.UtcNow;
        }

        private void CalculateTotalPrice()
        {
            TotalPrice = UnitPrice * Quantity;

            if (DiscountAmount != null)
            {
                TotalPrice = TotalPrice - DiscountAmount;
            }
        }
    }
}