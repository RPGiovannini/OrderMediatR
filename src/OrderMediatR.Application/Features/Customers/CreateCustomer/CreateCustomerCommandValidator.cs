using FluentValidation;

namespace OrderMediatR.Application.Features.Customers.CreateCustomer
{
    public class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
    {
        public CreateCustomerCommandValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("Nome é obrigatório")
                .MinimumLength(2).WithMessage("Nome deve ter pelo menos 2 caracteres")
                .MaximumLength(100).WithMessage("Nome não pode exceder 100 caracteres")
                .Matches(@"^[a-zA-ZÀ-ÿ\s]+$").WithMessage("Nome deve conter apenas letras");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Sobrenome é obrigatório")
                .MinimumLength(2).WithMessage("Sobrenome deve ter pelo menos 2 caracteres")
                .MaximumLength(100).WithMessage("Sobrenome não pode exceder 100 caracteres")
                .Matches(@"^[a-zA-ZÀ-ÿ\s]+$").WithMessage("Sobrenome deve conter apenas letras");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email é obrigatório")
                .EmailAddress().WithMessage("Email deve ter um formato válido")
                .MaximumLength(255).WithMessage("Email não pode exceder 255 caracteres");

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("Telefone é obrigatório")
                .Matches(@"^\(\d{2}\)\s\d{4,5}-\d{4}$").WithMessage("Telefone deve estar no formato (11) 99999-9999");

            RuleFor(x => x.DocumentNumber)
                .Must(BeValidDocument).When(x => !string.IsNullOrWhiteSpace(x.DocumentNumber))
                .WithMessage("CPF/CNPJ deve ter formato válido");

            RuleFor(x => x.DateOfBirth)
                .Must(BeValidDateOfBirth).When(x => x.DateOfBirth.HasValue)
                .WithMessage("Data de nascimento não pode ser no futuro");
        }

        private static bool BeValidDocument(string document)
        {
            if (string.IsNullOrWhiteSpace(document))
                return true;

            var cleanDocument = new string(document.Where(char.IsDigit).ToArray());

            if (cleanDocument.Length == 11)
                return IsValidCpf(cleanDocument);

            if (cleanDocument.Length == 14)
                return IsValidCnpj(cleanDocument);

            return false;
        }

        private static bool IsValidCpf(string cpf)
        {
            if (cpf.Length != 11 || cpf.All(c => c == cpf[0]))
                return false;

            var sum = 0;
            for (int i = 0; i < 9; i++)
                sum += int.Parse(cpf[i].ToString()) * (10 - i);

            var remainder = sum % 11;
            var digit1 = remainder < 2 ? 0 : 11 - remainder;

            if (int.Parse(cpf[9].ToString()) != digit1)
                return false;

            sum = 0;
            for (int i = 0; i < 10; i++)
                sum += int.Parse(cpf[i].ToString()) * (11 - i);

            remainder = sum % 11;
            var digit2 = remainder < 2 ? 0 : 11 - remainder;

            return int.Parse(cpf[10].ToString()) == digit2;
        }

        private static bool IsValidCnpj(string cnpj)
        {
            if (cnpj.Length != 14 || cnpj.All(c => c == cnpj[0]))
                return false;

            var weights1 = new[] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            var weights2 = new[] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

            var sum = 0;
            for (int i = 0; i < 12; i++)
                sum += int.Parse(cnpj[i].ToString()) * weights1[i];

            var remainder = sum % 11;
            var digit1 = remainder < 2 ? 0 : 11 - remainder;

            if (int.Parse(cnpj[12].ToString()) != digit1)
                return false;

            sum = 0;
            for (int i = 0; i < 13; i++)
                sum += int.Parse(cnpj[i].ToString()) * weights2[i];

            remainder = sum % 11;
            var digit2 = remainder < 2 ? 0 : 11 - remainder;

            return int.Parse(cnpj[13].ToString()) == digit2;
        }

        private static bool BeValidDateOfBirth(DateTime? dateOfBirth)
        {
            return !dateOfBirth.HasValue || dateOfBirth.Value <= DateTime.Today;
        }
    }
}