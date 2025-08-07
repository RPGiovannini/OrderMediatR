using System.Text.RegularExpressions;

namespace OrderMediatR.Domain.ValueObjects
{
    public class Phone
    {
        public string Value { get; set; }

        public Phone(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Telefone não pode ser vazio", nameof(value));

            if (!IsValidPhone(value))
                throw new ArgumentException("Telefone inválido", nameof(value));

            Value = CleanPhone(value);
        }

        public Phone()
        {
        }

        public static Phone Create(string value) => new Phone(value);

        private static bool IsValidPhone(string phone)
        {
            var cleanPhone = CleanPhone(phone);
            return cleanPhone.Length >= 10 && cleanPhone.Length <= 11;
        }

        private static string CleanPhone(string phone)
        {
            return new string(phone.Where(char.IsDigit).ToArray());
        }

        public static implicit operator string(Phone phone) => phone.Value;
        public static explicit operator Phone(string value) => new Phone(value);

        public override string ToString() => Value;
        public override bool Equals(object? obj) => obj is Phone phone && Value == phone.Value;
        public override int GetHashCode() => Value.GetHashCode();
    }
}