using System.ComponentModel.DataAnnotations;

namespace BallsRUs.Models.Admin
{
    public class AdminCreateProductVM
    {
        [Display(Name = "SKU")]
        public string? SKU { get; set; }

        [Display(Name = "Nom")]
        public string? Name { get; set; }

        [Display(Name = "Marque")]
        public string? Brand { get; set; }

        [Display(Name = "Modèle")]
        public string? Model { get; set; }

        [Display(Name = "Fichier image")]
        public IFormFile? Image { get; set; }

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
    }
}
