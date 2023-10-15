using FluentValidation;
using System.ComponentModel.DataAnnotations;

namespace BallsRUs.Models.Admin
{
    public class AdminEditProductVM
    {
        [Display(Name = "SKU")]
        public string? SKU { get; set; }

        [Display(Name = "Nom")]
        public string? Name { get; set; }

        [Display(Name = "Marque")]
        public string? Brand { get; set; }

        [Display(Name = "Modèle")]
        public string? Model { get; set; }

        [Display(Name = "Description courte")]
        public string? ShortDescription { get; set; }

        [Display(Name = "Description complète")]
        public string? FullDescription { get; set; }

        [Display(Name = "Poids en grammes")]
        public int? WeightInGrams { get; set; }

        [Display(Name = "Taille")]
        public string? Size { get; set; }

        [Display(Name = "Quantités")]
        public int? Quantity { get; set; }

        [Display(Name = "Prix")]
        public decimal? RetailPrice { get; set; }

        [Display(Name = "Prix en rabais (si en rabais)")]
        public decimal? DiscountedPrice { get; set; }

        public class Validator : AbstractValidator<AdminEditProductVM>
        {
            private const int MIN_NAME_LENGTH = 5;
            private const int MAX_NAME_LENGTH = 100;
            private const int MIN_SHORT_DESC_LENGTH = 20;
            private const int MAX_SHORT_DESC_LENGTH = 200;
            private const int MIN_FULL_DESC_LENGTH = 100;
            private const int MAX_FULL_DESC_LENGTH = 1200;
            private const string SKU_REGEX = "^[A-Z]{3}\\d{4}[A-Z]{3}$";
            public Validator()
            {
                RuleFor(vm => vm.SKU)
                    .NotEmpty()
                        .WithMessage("Veuillez entrer un SKU.")
                    .Matches(SKU_REGEX)
                        .WithMessage("Veuillez entrer un SKU valide. Assurez-vous de respecter le format suivant: ABC1234DEF. " +
                                     "(ABC est la catégorie, 1234 le numéro de modèle et DEF est la marque.");
                RuleFor(vm => vm.Name)
                    .NotEmpty()
                        .WithMessage("Veuillez entrer un nom.")
                    .Length(MIN_NAME_LENGTH, MAX_NAME_LENGTH)
                        .WithMessage($"Veuillez entrer un nom entre {MIN_NAME_LENGTH} et {MAX_NAME_LENGTH} caractères.");
                RuleFor(vm => vm.Brand)
                    .NotEmpty()
                        .WithMessage("Veuillez entrer un nom de marque.")
                    .Length(MIN_NAME_LENGTH, MAX_NAME_LENGTH)
                        .WithMessage($"Veuillez entrer un nom de marque entre {MIN_NAME_LENGTH} et {MAX_NAME_LENGTH} caractères.");
                RuleFor(vm => vm.Model)
                    .NotEmpty()
                        .WithMessage("Veuillez entrer un modèle.")
                    .Length(MIN_NAME_LENGTH, MAX_NAME_LENGTH)
                        .WithMessage($"Veuillez entrer un modèle entre {MIN_NAME_LENGTH} et {MAX_NAME_LENGTH} caractères.");
                RuleFor(vm => vm.ShortDescription)
                    .NotEmpty()
                        .WithMessage("Veuillez entrer une description courte.")
                    .Length(MIN_SHORT_DESC_LENGTH, MAX_SHORT_DESC_LENGTH)
                        .WithMessage($"Veuillez entrer une description courte entre {MIN_SHORT_DESC_LENGTH} et {MAX_SHORT_DESC_LENGTH} caractères.");
                RuleFor(vm => vm.FullDescription)
                    .NotEmpty()
                        .WithMessage("Veuillez entrer une description complète.")
                    .Length(MIN_FULL_DESC_LENGTH, MAX_FULL_DESC_LENGTH)
                        .WithMessage($"Veuillez entrer une description complète entre {MIN_FULL_DESC_LENGTH} et {MAX_FULL_DESC_LENGTH} caractères.");
                RuleFor(vm => vm.Quantity)
                    .NotEmpty()
                        .WithMessage("Veuillez entrer une quantité.")
                    .Must(x => x >= 0)
                        .WithMessage("Veuillez entrer une quantité positive.");
                RuleFor(vm => vm.RetailPrice)
                    .NotEmpty()
                        .WithMessage("Veuillez entrer un prix.");
            }
        }
    }
}
