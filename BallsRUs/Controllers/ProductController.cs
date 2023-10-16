using BallsRUs.Context;
using BallsRUs.Entities;
using BallsRUs.Models.Product;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace BallsRUs.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Catalog(string? category, string? search = null, string? filter = null)
        {
            if (!string.IsNullOrWhiteSpace(search))
                ViewBag.Search = search;

            if (!string.IsNullOrWhiteSpace(category))
                ViewBag.Category = category;

            // Appliquer les filtres
            ViewBag.HighToLow = false;
            ViewBag.LowToHigh = false;
            ViewBag.BrandAlphabetical = false;
            ViewBag.NewToOld = false;

            if (filter is not null && filter.ToLower() == "high-to-low")
                ViewBag.HighToLow = true;
            else if (filter is not null && filter.ToLower() == "low-to-high")
                ViewBag.LowToHigh = true;
            else if (filter is not null && filter.ToLower() == "brand-alphabetical")
                ViewBag.BrandAlphabetical = true;
            else if (filter is not null && filter.ToLower() == "new-to-old")
                ViewBag.NewToOld = true;

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
