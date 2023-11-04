using BallsRUs.Context;
using BallsRUs.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.EnvironmentVariables;

namespace BallsRUs.ViewComponents
{
    public class ProductList : ViewComponent
    {
        private const int NB_OF_SHOWCASED_PRODUCTS = 4;
        private const string PRICE_HIGH_TO_LOW = "high-to-low";
        private const string PRICE_LOW_TO_HIGH = "low-to-high";
        private const string BRAND_ALPHABETICAL = "brand-alphabetical";
        private const string RELEASE_NEW_TO_OLD = "new-to-old";

        private readonly ApplicationDbContext _context;

        public ProductList(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(bool isHomepageShowcase = false, string? category = null, string? search = null,
            string? sortingType = null, bool discounted = false)
        {
            ViewBag.IsHomePageShowcase = false;
            IQueryable<Product> products;

            products = _context.Products.Where(p => !p.IsArchived).AsQueryable();

            if (isHomepageShowcase)
            {
                ViewBag.IsHomePageShowcase = true;
                products = products.Where(p => p.DiscountedPrice.HasValue)
                                   .OrderByDescending(p => (p.RetailPrice - p.DiscountedPrice) / p.RetailPrice * 100)
                                   .Take(NB_OF_SHOWCASED_PRODUCTS)
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

            // Trier les produits
            if (!string.IsNullOrWhiteSpace(sortingType))
            {
                switch (sortingType)
                {
                    case PRICE_HIGH_TO_LOW: products = products.OrderByDescending(p => p.DiscountedPrice ?? p.RetailPrice);
                        break;
                    case PRICE_LOW_TO_HIGH: products = products.OrderBy(p => p.DiscountedPrice ?? p.RetailPrice);
                        break;
                    case BRAND_ALPHABETICAL: products = products.OrderBy(p => p.Brand);
                        break;
                    case RELEASE_NEW_TO_OLD: products = products.OrderByDescending(p => p.PublicationDate);
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