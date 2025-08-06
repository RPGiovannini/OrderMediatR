using System.Text.RegularExpressions;

namespace OrderMediatR.Domain.ValueObjects
{
    public class Email
    {
        public string Value { get; private set; }

        public Email(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Email não pode ser vazio", nameof(value));

            if (!IsValidEmail(value))
                throw new ArgumentException("Email inválido", nameof(value));

            Value = value.ToLowerInvariant();
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                return regex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }

        public static Email Create(string value) => new Email(value);

        public static implicit operator string(Email email) => email.Value;
        public static explicit operator Email(string value) => new Email(value);

        public override string ToString() => Value;
        public override bool Equals(object? obj) => obj is Email email && Value == email.Value;
        public override int GetHashCode() => Value.GetHashCode();
    }
}