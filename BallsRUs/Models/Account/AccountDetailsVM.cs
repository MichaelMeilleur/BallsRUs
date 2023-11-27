using BallsRUs.Entities;
using FluentValidation;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace BallsRUs.Models.Account
{
    public class AccountDetailsVM
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? AddressStreet { get; set; }
        public string? AddressCity { get; set; }
        public string? AddressStateProvince { get; set; }
        public string? AddressCountry { get; set; }
        public string? AddressPostalCode { get; set; }
    }

    public class AccountDetailsVMValidator : AbstractValidator<AccountDetailsVM>
    {
        public AccountDetailsVMValidator()
        {
            RuleFor(vm => vm.Email)
                .NotEmpty()
                    .WithMessage("Veuillez entrer une adresse courriel.")
                .EmailAddress()
                    .WithMessage("Veuillez entrer une adresse courriel valide.");
            RuleFor(vm => vm.FirstName)
                 .NotEmpty().WithMessage("Veuillez entrer un prénom.")
                 .Must(BeValidLetters).WithMessage("Le prénom ne doit contenir que des lettres.");

            RuleFor(vm => vm.LastName)
                .NotEmpty().WithMessage("Veuillez entrer un nom.")
                .Must(BeValidLetters).WithMessage("Le nom ne doit contenir que des lettres.");

            RuleFor(vm => vm.PhoneNumber)
              .NotEmpty().WithMessage("Veuillez entrer un numéro.")
              .Must(BeNumbers).WithMessage("Le numéro ne doit contenir que des chiffres.");

                RuleFor(vm => vm.AddressCity)
              .NotEmpty()
                  .WithMessage("Veuillez entrer une ville.");
                RuleFor(vm => vm.AddressStreet)
             .NotEmpty()
                 .WithMessage("Veuillez entrer votre rue et numéro civique.");
                RuleFor(vm => vm.AddressStateProvince)
                .NotEmpty()
                    .WithMessage("Veuillez entrer une province.");
                RuleFor(vm => vm.AddressCountry)
                .NotEmpty()
                    .WithMessage("Veuillez entrer un code postal.");
            RuleFor(vm => vm.AddressPostalCode)
        .NotEmpty()
            .WithMessage("Veuillez entrer un code postal.")
        .Must(BeAValidCanadianPostalCode)
            .WithMessage("Le code postal doit être au format canadien (ex. A1A 1A1).");
        }

        private bool BeValidLetters(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return true;
            }
            return value.All(char.IsLetter);
        }

        private bool BeNumbers(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return true;
            }
            return value.All(char.IsNumber);
        }

        private bool BeAValidCanadianPostalCode(string postalCode)
        {
            var canadianPostalCodeRegex = new Regex(@"^[A-Za-z]\d[A-Za-z] \d[A-Za-z]\d$");

            return !string.IsNullOrEmpty(postalCode) && canadianPostalCodeRegex.IsMatch(postalCode);
        }
    }
}
