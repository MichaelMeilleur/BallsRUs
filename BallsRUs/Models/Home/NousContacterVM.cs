using FluentValidation;
using System.ComponentModel.DataAnnotations;

namespace BallsRUs.Models.Home
{
    public class NousContacterVM
    {
        [Display(Name = "Nom")]
        public string? Name { get; set; }

        [Display(Name = "Courriel")]
        public string? Email { get; set; }

        [Display(Name = "Description")]
        public string? Description { get; set; }

        public class Validator : AbstractValidator<NousContacterVM>
        {
            private const string NAME_REG = "^[a-zA-Zà-üÀ-Ü -]+$";
            private const string EMAIL_REG = "^[\\w-\\.]+@([\\w-]+\\.)+[\\w-]{2,4}$";
            public Validator()
            {
                RuleFor(u => u.Name)
                    .NotEmpty().WithMessage("Vous devez entrer votre nom").Length(2, 30).WithMessage("Votre nom doit être entre 2 et 30 caractères")
                    .Matches(NAME_REG).WithMessage("Entrez un nom valide");

                RuleFor(u => u.Email)
                    .NotEmpty().WithMessage("Vous devez entrer votre courriel")
                    .Matches(EMAIL_REG).WithMessage("Entrez un courriel valide");

                RuleFor(u => u.Description)
                    .NotEmpty().WithMessage("Vous devez spécifier votre demande").Length(10, 2000).WithMessage("Votre message doit contenir entre 10 et 2000 caractères");
            }
        }
    }
}
