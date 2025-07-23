namespace OrderMediatR.Domain.ValueObjects
{
    public class OrderNumber
    {
        public string Value { get; private set; }

        public OrderNumber(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Número do pedido não pode ser vazio", nameof(value));

            Value = value;
        }

        public static OrderNumber Generate()
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var random = new Random().Next(1000, 9999);
            return new OrderNumber($"ORD-{timestamp}-{random}");
        }

        public static implicit operator string(OrderNumber orderNumber) => orderNumber.Value;
        public static explicit operator OrderNumber(string value) => new OrderNumber(value);

        public override string ToString() => Value;
        public override bool Equals(object? obj) => obj is OrderNumber orderNumber && Value == orderNumber.Value;
        public override int GetHashCode() => Value.GetHashCode();
    }
}