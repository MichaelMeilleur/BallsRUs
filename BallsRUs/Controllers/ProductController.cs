using BallsRUs.Context;
using BallsRUs.Entities;
using BallsRUs.Models.Product;
using BallsRUs.Utilities;
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

        public IActionResult Catalog(string? category = null, string? search = null,
            bool discounted = false, string? sorting = null, Dictionary<string, bool>? checkedBoxBrandFilter = null, 
            string? rangePrice = null)
        {
            if (!string.IsNullOrWhiteSpace(search))
                ViewBag.Search = search;

            if (!string.IsNullOrWhiteSpace(category))
                ViewBag.Category = category;

            // Appliquer le tri
            if (sorting is not null && sorting.ToLower() == Constants.PRICE_HIGH_TO_LOW)
                ViewBag.SortingType = Constants.PRICE_HIGH_TO_LOW;
            else if (sorting is not null && sorting.ToLower() == Constants.PRICE_LOW_TO_HIGH)
                ViewBag.SortingType = Constants.PRICE_LOW_TO_HIGH;
            else if (sorting is not null && sorting.ToLower() == Constants.BRAND_ALPHABETICAL)
                ViewBag.SortingType = Constants.BRAND_ALPHABETICAL;
            else if (sorting is not null && sorting.ToLower() == Constants.RELEASE_NEW_TO_OLD)
                ViewBag.SortingType = Constants.RELEASE_NEW_TO_OLD;

            // Appliquer les filtres
            ViewBag.FilterDiscounted = discounted;

            ViewBag.listBrands = _context.Products.Select(x => x.Brand).Distinct().ToList();
            int brandNumberTotal = _context.Products.Select(x => x.Brand).Distinct().ToList().Count();

            //Vérifier pourquoi le count est à 1 au début
            if (checkedBoxBrandFilter is null || !checkedBoxBrandFilter.Any() || checkedBoxBrandFilter.Count() == 1)
            {
                checkedBoxBrandFilter = new Dictionary<string, bool>();

                for (int i = 0; i < brandNumberTotal; i++)
                {
                    checkedBoxBrandFilter.Add(ViewBag.listBrands[i], true);
                }
            }

            ViewBag.checkedBoxBrandFilter = checkedBoxBrandFilter;

            if (rangePrice is not null)
            {
                string[] parties = rangePrice.Split('$');
                int firstNumberRangePrice;
                int secondNumberRangePrice;

                // Convertir les parties en entiers
                if (parties.Length >= 2)
                {
                    if (int.TryParse(parties[0].Trim(), out int premierNombre) && int.TryParse(parties[1].Substring(2), out int deuxiemeNombre))
                    {
                        firstNumberRangePrice = premierNombre;
                        ViewBag.minValue = firstNumberRangePrice;
                        secondNumberRangePrice = deuxiemeNombre;
                        ViewBag.maxValue = secondNumberRangePrice;
                    }
                }
            }


            if (ViewBag.minValue is null || ViewBag.maxValue is null)
            {
                ViewBag.minValue = 0;
                ViewBag.maxValue = 2000;
            }

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
