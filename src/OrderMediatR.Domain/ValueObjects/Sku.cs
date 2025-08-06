namespace OrderMediatR.Domain.ValueObjects
{
    public class Sku
    {
        public string Value { get; private set; }

        public Sku(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("SKU não pode ser vazio", nameof(value));

            if (value.Length < 3 || value.Length > 50)
                throw new ArgumentException("SKU deve ter entre 3 e 50 caracteres", nameof(value));

            Value = value.ToUpperInvariant();
        }

        public static Sku Create(string value) => new Sku(value);

        public static Sku Generate(string prefix = "SKU")
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd");
            var random = new Random().Next(1000, 9999);
            return new Sku($"{prefix}-{timestamp}-{random}");
        }

        public static implicit operator string(Sku sku) => sku.Value;
        public static explicit operator Sku(string value) => new Sku(value);

        public override string ToString() => Value;
        public override bool Equals(object? obj) => obj is Sku sku && Value == sku.Value;
        public override int GetHashCode() => Value.GetHashCode();
    }
}