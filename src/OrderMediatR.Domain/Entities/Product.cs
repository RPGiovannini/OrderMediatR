using OrderMediatR.Common;
using OrderMediatR.Domain.ValueObjects;

namespace OrderMediatR.Domain.Entities
{
    public class Product : BaseEntity
    {
        public string Name { get; private set; }
        public string Description { get; private set; }
        public string Sku { get; private set; }
        public Money Price { get; private set; }
        public int StockQuantity { get; private set; }
        public string Category { get; private set; }
        public string? Brand { get; private set; }
        public decimal Weight { get; private set; }
        public string? ImageUrl { get; private set; }
        public bool IsAvailable { get; private set; } = true;

        private readonly List<OrderItem> _orderItems = new();
        public IReadOnlyCollection<OrderItem> OrderItems => _orderItems.AsReadOnly();

        protected Product() { }

        public Product(string name, string description, string sku, Money price, int stockQuantity, string category)
        {
            ValidateProduct(name, description, sku, stockQuantity, category);

            Name = name;
            Description = description;
            Sku = sku;
            Price = price;
            StockQuantity = stockQuantity;
            Category = category;
        }

        public void UpdateBasicInfo(string name, string description, string category)
        {
            ValidateBasicInfo(name, description, category);

            Name = name;
            Description = description;
            Category = category;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdatePrice(Money newPrice)
        {
            Price = newPrice;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateStock(int newQuantity)
        {
            if (newQuantity < 0)
                throw new ArgumentException("Quantidade em estoque não pode ser negativa", nameof(newQuantity));

            StockQuantity = newQuantity;

            if (newQuantity == 0)
                IsAvailable = false;
            else if (!IsAvailable)
                IsAvailable = true;

            UpdatedAt = DateTime.UtcNow;
        }

        public void AddToStock(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantidade deve ser maior que zero", nameof(quantity));

            StockQuantity += quantity;

            if (!IsAvailable)
                IsAvailable = true;

            UpdatedAt = DateTime.UtcNow;
        }

        public void RemoveFromStock(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantidade deve ser maior que zero", nameof(quantity));

            if (StockQuantity < quantity)
                throw new InvalidOperationException("Quantidade solicitada excede o estoque disponível");

            StockQuantity -= quantity;

            if (StockQuantity == 0)
                IsAvailable = false;

            UpdatedAt = DateTime.UtcNow;
        }

        public void SetBrand(string brand)
        {
            Brand = brand;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetWeight(decimal weight)
        {
            if (weight < 0)
                throw new ArgumentException("Peso não pode ser negativo", nameof(weight));

            Weight = weight;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetImageUrl(string imageUrl)
        {
            ImageUrl = imageUrl;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetAvailability(bool isAvailable)
        {
            if (!isAvailable && StockQuantity > 0)
                throw new InvalidOperationException("Produto com estoque não pode ser marcado como indisponível");

            IsAvailable = isAvailable;
            UpdatedAt = DateTime.UtcNow;
        }

        public bool HasStock(int quantity)
        {
            return IsAvailable && StockQuantity >= quantity;
        }

        private static void ValidateProduct(string name, string description, string sku, int stockQuantity, string category)
        {
            ValidateBasicInfo(name, description, category);

            if (string.IsNullOrWhiteSpace(sku))
                throw new ArgumentException("SKU não pode ser vazio", nameof(sku));

            if (stockQuantity < 0)
                throw new ArgumentException("Quantidade em estoque não pode ser negativa", nameof(stockQuantity));
        }

        private static void ValidateBasicInfo(string name, string description, string category)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Nome não pode ser vazio", nameof(name));

            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Descrição não pode ser vazia", nameof(description));

            if (string.IsNullOrWhiteSpace(category))
                throw new ArgumentException("Categoria não pode ser vazia", nameof(category));

            if (name.Length < 3)
                throw new ArgumentException("Nome deve ter pelo menos 3 caracteres", nameof(name));

            if (description.Length < 10)
                throw new ArgumentException("Descrição deve ter pelo menos 10 caracteres", nameof(description));
        }
    }
}