using OrderMediatR.Common;
using OrderMediatR.Domain.ValueObjects;

namespace OrderMediatR.Domain.Entities
{
    public class Customer : BaseEntity
    {
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public Email Email { get; private set; }
        public string Phone { get; private set; }
        public string? DocumentNumber { get; private set; }
        public DateTime? DateOfBirth { get; private set; }

        private readonly List<Address> _addresses = new();
        private readonly List<Order> _orders = new();

        public IReadOnlyCollection<Address> Addresses => _addresses.AsReadOnly();
        public IReadOnlyCollection<Order> Orders => _orders.AsReadOnly();

        public string FullName => $"{FirstName} {LastName}".Trim();

        protected Customer() { }

        public Customer(string firstName, string lastName, Email email, string phone)
        {
            ValidateCustomer(firstName, lastName, phone);

            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Phone = phone;
        }

        public void UpdatePersonalInfo(string firstName, string lastName, string phone)
        {
            ValidateCustomer(firstName, lastName, phone);

            FirstName = firstName;
            LastName = lastName;
            Phone = phone;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateEmail(Email email)
        {
            Email = email;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetDocumentNumber(string documentNumber)
        {
            if (!string.IsNullOrWhiteSpace(documentNumber) && !IsValidDocument(documentNumber))
                throw new ArgumentException("Número de documento inválido");

            DocumentNumber = documentNumber;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetDateOfBirth(DateTime dateOfBirth)
        {
            if (dateOfBirth > DateTime.Today)
                throw new ArgumentException("Data de nascimento não pode ser no futuro");

            DateOfBirth = dateOfBirth;
            UpdatedAt = DateTime.UtcNow;
        }

        public void AddAddress(Address address)
        {
            if (address == null)
                throw new ArgumentNullException(nameof(address));

            if (_addresses.Any(a => a.IsDefault && address.IsDefault))
                throw new InvalidOperationException("Já existe um endereço padrão");

            _addresses.Add(address);
        }

        public void RemoveAddress(Guid addressId)
        {
            var address = _addresses.FirstOrDefault(a => a.Id == addressId);
            if (address != null)
                _addresses.Remove(address);
        }

        public Address? GetDefaultAddress()
        {
            return _addresses.FirstOrDefault(a => a.IsDefault);
        }

        public void AddOrder(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            _orders.Add(order);
        }

        private static void ValidateCustomer(string firstName, string lastName, string phone)
        {
            if (string.IsNullOrWhiteSpace(firstName))
                throw new ArgumentException("Nome não pode ser vazio", nameof(firstName));

            if (string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentException("Sobrenome não pode ser vazio", nameof(lastName));

            if (string.IsNullOrWhiteSpace(phone))
                throw new ArgumentException("Telefone não pode ser vazio", nameof(phone));

            if (firstName.Length < 2)
                throw new ArgumentException("Nome deve ter pelo menos 2 caracteres", nameof(firstName));

            if (lastName.Length < 2)
                throw new ArgumentException("Sobrenome deve ter pelo menos 2 caracteres", nameof(lastName));
        }

        private static bool IsValidDocument(string document)
        {
            var cleanDocument = new string(document.Where(char.IsDigit).ToArray());
            return cleanDocument.Length == 11 || cleanDocument.Length == 14;
        }
    }
}