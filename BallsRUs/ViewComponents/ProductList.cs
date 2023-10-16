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

        private readonly ApplicationDbContext _context;

        public ProductList(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(bool isHomepageShowcase = false, string? category = null, string? search = null,
            List<Product>? productsAlreadyFiltered = null, bool priceHighToLow = false, bool priceLowToHigh = false, bool brandAlphabetical = false, bool newToOld = false)
        {
            ViewBag.IsHomePageShowcase = false;
            IQueryable<Product> products;

            if (productsAlreadyFiltered is not null && productsAlreadyFiltered.Any())
            {
                products = productsAlreadyFiltered.AsQueryable();
            }
            else
            {
                products = _context.Products.AsQueryable();

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
                    products = products.Where(p => p.Name!.ToLower().Contains(search) || p.Brand!.ToLower().Contains(search) || p.Model!.ToLower().Contains(search) ||
                                                   p.SKU!.ToLower() == search || p.Categories.Any(c => c.Name!.ToLower().Contains(search)));
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
                }

                List<Product>? filteredProducts = await products.ToListAsync();
            }

            // Trier les produits
            if (priceHighToLow)
            {
                products = products.OrderByDescending(p => p.DiscountedPrice ?? p.RetailPrice);
            }
            else if (priceLowToHigh)
            {
                products = products.OrderBy(p => p.DiscountedPrice ?? p.RetailPrice);
            }
            else if (brandAlphabetical)
            {
                products = products.OrderBy(p => p.Brand);
            }
            else if (newToOld)
            {
                products = products.OrderByDescending(p => p.PublicationDate);
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