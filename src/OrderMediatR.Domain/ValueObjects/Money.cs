namespace OrderMediatR.Domain.ValueObjects
{
    public class Money
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; }

        public Money(decimal amount, string currency = "BRL")
        {
            if (amount < 0)
                throw new ArgumentException("Valor não pode ser negativo", nameof(amount));

            if (string.IsNullOrWhiteSpace(currency))
                throw new ArgumentException("Moeda não pode ser vazia", nameof(currency));

            Amount = Math.Round(amount, 2);
            Currency = currency.ToUpperInvariant();
        }

        public Money()
        {
        }

        public static Money Zero => new Money(0);

        public static Money Create(decimal amount, string currency = "BRL") => new Money(amount, currency);

        public static Money operator +(Money left, Money right)
        {
            if (left.Currency != right.Currency)
                throw new InvalidOperationException("Não é possível somar valores em moedas diferentes");

            return new Money(left.Amount + right.Amount, left.Currency);
        }

        public static Money operator -(Money left, Money right)
        {
            if (left.Currency != right.Currency)
                throw new InvalidOperationException("Não é possível subtrair valores em moedas diferentes");

            return new Money(left.Amount - right.Amount, left.Currency);
        }

        public static Money operator *(Money money, int quantity)
        {
            return new Money(money.Amount * quantity, money.Currency);
        }

        public static bool operator >(Money left, Money right)
        {
            if (left.Currency != right.Currency)
                throw new InvalidOperationException("Não é possível comparar valores em moedas diferentes");

            return left.Amount > right.Amount;
        }

        public static bool operator <(Money left, Money right)
        {
            if (left.Currency != right.Currency)
                throw new InvalidOperationException("Não é possível comparar valores em moedas diferentes");

            return left.Amount < right.Amount;
        }

        public static implicit operator decimal(Money money) => money.Amount;
        public static explicit operator Money(decimal amount) => new Money(amount);

        public override string ToString() => $"{Amount:C}";
        public override bool Equals(object? obj) => obj is Money money && Amount == money.Amount && Currency == money.Currency;
        public override int GetHashCode() => HashCode.Combine(Amount, Currency);
    }
}