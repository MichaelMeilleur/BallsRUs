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
            string? rangePrice = null, string? brands = null, bool selectAllBrands = true)
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
            if(brands is not null)
            {
                checkedBoxBrandFilter = ParseBrandFilter(brands);
                ViewBag.checkedBoxBrandFilter = checkedBoxBrandFilter;
                ViewBag.brandsSelectedCheckedBoxStringConcat = brands;
            }
            else
            {
                //Quick fix, au début le count du dictionnaire est à 1 et on a la category comme entrée dans la liste
                if (selectAllBrands || checkedBoxBrandFilter is null || !checkedBoxBrandFilter.Any() || 
                    checkedBoxBrandFilter.Count() == 1 && checkedBoxBrandFilter.ContainsKey("category"))
                {
                    checkedBoxBrandFilter = new Dictionary<string, bool>();

                    for (int i = 0; i < brandNumberTotal; i++)
                    {
                        checkedBoxBrandFilter.Add(ViewBag.listBrands[i], true);
                    }
                }
                ViewBag.checkedBoxBrandFilter = checkedBoxBrandFilter;

                string brandsSelectedCheckedBoxStringConcat = "";
                foreach(var brand in checkedBoxBrandFilter)
                {
                    brandsSelectedCheckedBoxStringConcat += brand.Key.ToString() + ",";
                }
                ViewBag.brandsSelectedCheckedBoxStringConcat = brandsSelectedCheckedBoxStringConcat;
            }

            //Selection toutes les marques bouton
            ViewBag.SelectAllBrands = selectAllBrands;

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

            ViewBag.PriceRange = ViewBag.minValue + "$ - " + ViewBag.maxValue + "$";

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

        private Dictionary<string, bool> ParseBrandFilter(string brands)
        {
            var brandFilter = new Dictionary<string, bool>();

            if (!string.IsNullOrEmpty(brands))
            {
                var brandArray = brands.Split(',');
                foreach (var brand in brandArray)
                {
                    brandFilter.Add(brand, true);
                }
            }

            return brandFilter;
        }
    }
}
