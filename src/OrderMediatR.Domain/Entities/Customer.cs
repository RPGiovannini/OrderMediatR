using OrderMediatR.Common;
using OrderMediatR.Domain.Events;
using OrderMediatR.Domain.ValueObjects;

namespace OrderMediatR.Domain.Entities
{
    public class Customer : BaseEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Email Email { get; set; }
        public Phone Phone { get; set; }
        public string? DocumentNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }

        private readonly List<Address> _addresses = new();
        private readonly List<Order> _orders = new();

        public IReadOnlyCollection<Address> Addresses => _addresses.AsReadOnly();
        public IReadOnlyCollection<Order> Orders => _orders.AsReadOnly();

        public string FullName => $"{FirstName} {LastName}".Trim();
        public int TotalOrders => _orders.Count;

        protected Customer() 
        {
            _addresses = new List<Address>();
            _orders = new List<Order>();
        }

        public Customer(string firstName, string lastName, Email email, Phone phone)
        {
            ValidateCustomer(firstName, lastName, phone?.Value);

            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Phone = phone;
            
            AddDomainEvent(new EntityChangedDomainEvent<Customer>(this, "Created"));
        }

        public void UpdatePersonalInfo(string firstName, string lastName, Phone phone)
        {
            ValidateCustomer(firstName, lastName, phone?.Value);

            FirstName = firstName;
            LastName = lastName;
            Phone = phone;
            SetUpdatedAt();
            
            // Disparar evento de atualização
            AddDomainEvent(new EntityChangedDomainEvent<Customer>(this, "Updated"));
        }

        public void UpdateEmail(Email email)
        {
            Email = email;
            SetUpdatedAt();
        }

        public void SetDocumentNumber(string documentNumber)
        {
            if (!string.IsNullOrWhiteSpace(documentNumber) && !IsValidDocument(documentNumber))
                throw new ArgumentException("Número de documento inválido");

            DocumentNumber = documentNumber;
            SetUpdatedAt();
        }

        public void SetDateOfBirth(DateTime dateOfBirth)
        {
            if (dateOfBirth > DateTime.Today)
                throw new ArgumentException("Data de nascimento não pode ser no futuro");

            DateOfBirth = dateOfBirth;
            SetUpdatedAt();
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
        }

        private static bool IsValidDocument(string document)
        {
            var cleanDocument = new string(document.Where(char.IsDigit).ToArray());
            return cleanDocument.Length == 11 || cleanDocument.Length == 14;
        }

        public static Customer FromSync(
            Guid id,
            string firstName,
            string lastName,
            Email email,
            Phone phone,
            string? documentNumber,
            DateTime? dateOfBirth,
            DateTime createdAt,
            DateTime? updatedAt,
            bool isActive)
        {
            var customer = new Customer
            {
                Id = id,
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Phone = phone,
                DocumentNumber = documentNumber,
                DateOfBirth = dateOfBirth,
                CreatedAt = createdAt,
                UpdatedAt = updatedAt,
                IsActive = isActive
            };
            return customer;
        }

        public void UpdateFromSync(
            string firstName,
            string lastName,
            Email email,
            Phone phone,
            string? documentNumber,
            DateTime? dateOfBirth,
            DateTime? updatedAt,
            bool isActive)
        {
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Phone = phone;
            DocumentNumber = documentNumber;
            DateOfBirth = dateOfBirth;
            UpdatedAt = updatedAt;
            IsActive = isActive;
        }
    }
}