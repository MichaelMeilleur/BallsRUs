using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace BallsRUs.Models.Account
{
    public class RegisterVM
    {
        [Display(Name = "Nom d'utilisateur")]
        public string UserName { get; set; }

        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [Display(Name = "Password confirmation")]
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
                .Matches("^[^\\s]{5,20}$").WithMessage("Le nom d'utilisateur doit contenir entre 5 et 20 caractères sans espaces.");
        }
    }
}
