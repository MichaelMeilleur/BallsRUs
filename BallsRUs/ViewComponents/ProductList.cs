using BallsRUs.Context;
using BallsRUs.Entities;
using BallsRUs.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.EnvironmentVariables;

namespace BallsRUs.ViewComponents
{
    public class ProductList : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public ProductList(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(bool isHomepageShowcase = false, string? category = null, string? search = null,
            string? sortingType = null, bool discounted = false, Dictionary<string, bool>? checkedBoxBrandFilterVC = null, int? minValue = null, int? maxValue = null)
        {
            ViewBag.IsHomePageShowcase = false;
            IQueryable<Product> products;

            products = _context.Products.Where(p => !p.IsArchived).AsQueryable();

            if (isHomepageShowcase)
            {
                ViewBag.IsHomePageShowcase = true;
                products = products.Where(p => p.DiscountedPrice.HasValue)
                                   .OrderByDescending(p => (p.RetailPrice - p.DiscountedPrice) / p.RetailPrice * 100)
                                   .Take(Constants.NB_OF_SHOWCASED_PRODUCTS)
                                   .AsQueryable();
            }
            else if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                string[] searchTerms = search.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                foreach (string term in searchTerms)
                {
                    products = products.Where(p => p.Name!.ToLower().Contains(term) || p.Brand!.ToLower().Contains(term) || p.Model!.ToLower().Contains(term) ||
                                               p.SKU!.ToLower() == term || p.Categories.Any(c => c.Name!.ToLower().Contains(term)));
                }

                if (!products.Any())
                {
                    ViewBag.MessageBasDePage = "Aucun produit ne correspond à la recherche.";
                }
            }
            else if (!string.IsNullOrWhiteSpace(category))
            {
                IEnumerable<Category> categories = _context.Categories.AsEnumerable();

                if (categories.Any(c => c.Name!.ToLower() == category.ToLower()))
                {
                    Category? selectedCategory = categories.FirstOrDefault(c => c.Name!.ToLower() == category.ToLower());

                    List<Category> categoryAndChildren = GetCategoryAndChildren(categories, selectedCategory!);

                    products = products.Where(p => p.Categories.Any(productCategory => categoryAndChildren.Contains(productCategory)));
                }
                else
                {
                    products = products.Where(p => !p.Categories.Any());

                    ViewBag.MessageBasDePage = "Aucune catégorie trouvée.";
                }
            }

            List<Product>? filteredProducts = await products.ToListAsync();

            // Filtrer les produits
            if (discounted)
            {
                products = products.Where(p => p.DiscountedPrice != null);
            }

            // Filtrer les marques
            List<Product> productsBrandFiltered = new();

            if (checkedBoxBrandFilterVC is not null)
            {
                foreach (var product in products)
                {
                    if (checkedBoxBrandFilterVC.ContainsKey(product.Brand))
                    {
                        productsBrandFiltered.Add(product);
                    }
                }
                products = productsBrandFiltered.AsQueryable();
            }

            // Filtrer les prix
            if(minValue is not null && minValue >= 0 && maxValue is not null && maxValue >= 0)
            {
                products = products.Where(x => (x.RetailPrice >= minValue && x.RetailPrice <= maxValue) || 
                                   (x.DiscountedPrice != null && x.DiscountedPrice >= minValue && x.DiscountedPrice <= maxValue));
            }

            // Trier les produits
            if (!string.IsNullOrWhiteSpace(sortingType))
            {
                switch (sortingType)
                {
                    case Constants.PRICE_HIGH_TO_LOW: products = products.OrderByDescending(p => p.DiscountedPrice ?? p.RetailPrice);
                        break;
                    case Constants.PRICE_LOW_TO_HIGH: products = products.OrderBy(p => p.DiscountedPrice ?? p.RetailPrice);
                        break;
                    case Constants.BRAND_ALPHABETICAL: products = products.OrderBy(p => p.Brand);
                        break;
                    case Constants.RELEASE_NEW_TO_OLD: products = products.OrderByDescending(p => p.PublicationDate);
                        break;
                    default:
                        break;
                }
            }

            return View(products);
        }

        List<Category> GetCategoryAndChildren(IEnumerable<Category> allCategories, Category parentCategory)
        {
            var result = new List<Category> { parentCategory };

            var children = allCategories.Where(c => c.ParentCategoryId == parentCategory.Id).ToList();

            foreach (var child in children)
                result.AddRange(GetCategoryAndChildren(allCategories, child));

            return result;
        }
    }
}