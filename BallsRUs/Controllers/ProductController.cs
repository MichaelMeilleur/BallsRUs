using BallsRUs.Context;
using BallsRUs.Entities;
using BallsRUs.Models.Product;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace BallsRUs.Controllers
{
    public class ProductController : Controller
    {
        private const string PRICE_HIGH_TO_LOW = "high-to-low";
        private const string PRICE_LOW_TO_HIGH = "low-to-high";
        private const string BRAND_ALPHABETICAL = "brand-alphabetical";
        private const string RELEASE_NEW_TO_OLD = "new-to-old";

        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Catalog(string? category = null, string? search = null, string? sorting = null, bool discounted = false)
        {
            if (!string.IsNullOrWhiteSpace(search))
                ViewBag.Search = search;

            if (!string.IsNullOrWhiteSpace(category))
                ViewBag.Category = category;

            // Appliquer le tri
            if (sorting is not null && sorting.ToLower() == PRICE_HIGH_TO_LOW)
                ViewBag.SortingType = PRICE_HIGH_TO_LOW;
            else if (sorting is not null && sorting.ToLower() == PRICE_LOW_TO_HIGH)
                ViewBag.SortingType = PRICE_LOW_TO_HIGH;
            else if (sorting is not null && sorting.ToLower() == BRAND_ALPHABETICAL)
                ViewBag.SortingType = BRAND_ALPHABETICAL;
            else if (sorting is not null && sorting.ToLower() == RELEASE_NEW_TO_OLD)
                ViewBag.SortingType = RELEASE_NEW_TO_OLD;

            // Appliquer les filtres
            ViewBag.FilterDiscounted = discounted;

            return View();
        }

        public IActionResult Details(Guid productId)
        {
            var productToShow = _context.Products.Find(productId);

            if (productToShow is null)
                return NotFound();

            List<Category> categories = _context.Categories.Where(c => productToShow.Categories.Contains(c)).ToList();

            var vm = new ProductDetailsVM()
            {
                SKU = productToShow.SKU,
                Name = productToShow.Name,
                Brand = productToShow.Brand,
                Model = productToShow.Model,
                ImagePath = productToShow.ImagePath,
                ShortDescription = productToShow.ShortDescription,
                FullDescription = productToShow.FullDescription,
                WeightInGrams = productToShow.WeightInGrams,
                Size = productToShow.Size,
                Quantity = productToShow.Quantity,
                RetailPrice = string.Format(new CultureInfo("fr-CA"), "{0:C}", productToShow.RetailPrice),
                DiscountedPrice = string.Format(new CultureInfo("fr-CA"), "{0:C}", productToShow.DiscountedPrice),
                PublicationDate = string.Format("{0:dd/MM/yyyy}", productToShow.PublicationDate),
                Categories = categories
            };

            return View(vm);
        }
    }
}
