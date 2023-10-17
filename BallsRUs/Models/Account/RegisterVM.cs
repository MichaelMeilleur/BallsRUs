﻿using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace BallsRUs.Models.Account
{
    public class RegisterVM
    {
        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? UserName { get; set; }

        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        public string? PasswordConfirmation { get; set; }
    }
    public class RegisterVMValidator : AbstractValidator<RegisterVM>
    {
        public RegisterVMValidator()
        {
            RuleFor(vm => vm.UserName)
                .NotEmpty()
                    .WithMessage("Veuillez entrer une adresse courriel.")
                .EmailAddress()
                    .WithMessage("Veuillez entrer une adresse courriel valide.");
            RuleFor(vm => vm.FirstName)
                .NotEmpty()
                    .WithMessage("Veuillez entrer un prénom.");
            RuleFor(vm => vm.LastName)
                .NotEmpty()
                    .WithMessage("Veuillez entrer un nom.");
            RuleFor(vm => vm.Password)
                .NotEmpty()
                    .WithMessage("Veuillez entrer un mot de passe");
            RuleFor(vm => vm.PasswordConfirmation)
                .NotEmpty()
                    .WithMessage("Veuillez confirmer votre mot de passe.")
                .Equal(vm => vm.Password)
                    .WithMessage("Le mot de passe et le mot de passe de confirmation de correspondent pas.");
        }
    }
}
