using BallsRUs.Entities;
using FluentValidation;
using System.ComponentModel.DataAnnotations;

namespace BallsRUs.Models.Account
{
    public class AccountDetailsVM
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public Address? Address { get; set; }
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
            RuleFor(vm => vm.PhoneNumber).Must(BeValidNumbers).WithMessage("Le numéro ne doit contenir que des chiffres.");
            RuleFor(vm => vm.Address.City)
              .NotEmpty()
                  .WithMessage("Veuillez entrer une ville.");
            RuleFor(vm => vm.Address.Street)
         .NotEmpty()
             .WithMessage("Veuillez entrer votre rue et numéro civique.");
            RuleFor(vm => vm.Address.StateProvince)
            .NotEmpty()
                .WithMessage("Veuillez entrer une province.");
            RuleFor(vm => vm.Address.PostalCode)
            .NotEmpty()
                .WithMessage("Veuillez entrer un code postal.");
            RuleFor(vm => vm.Address.Country)
            .NotEmpty()
                .WithMessage("Veuillez entrer un pays.");

        }

        private bool BeValidLetters(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return true;
            }
            return value.All(char.IsLetter);
        }
        private bool BeValidNumbers(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return true;
            }
            return value.All(char.IsNumber);
        }
    }
}
