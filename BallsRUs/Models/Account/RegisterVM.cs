using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace BallsRUs.Models.Account
{
    public class RegisterVM
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password))]
        public string PasswordConfirmation { get; set; }
    }
    public class RegisterVMValidator : AbstractValidator<RegisterVM>
    {
        public RegisterVMValidator()
        {
            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("Le nom d'utilisateur est obligatoire.")
                .EmailAddress().WithMessage("Veuillez entrer une adresse courriel valide.");
        }
    }
}
