using BallsRUs.Context;
using BallsRUs.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            List<Product>? productsAlreadyFiltered = null)
        {
            if (productsAlreadyFiltered is not null && productsAlreadyFiltered.Any())
            {
                return View(productsAlreadyFiltered);
            }
            else
            {
                var products = _context.Products.AsQueryable();

                if (isHomepageShowcase)
                {
                    products = products.Where(p => p.DiscountedPrice.HasValue)
                                       .OrderByDescending(p => (p.RetailPrice - p.DiscountedPrice) / p.RetailPrice * 100)
                                       .Take(NB_OF_SHOWCASED_PRODUCTS)
                                       .AsQueryable();
                }
                else if (search is not null)
                {
                    search = search.ToLower();
                    products = products.Where(p => p.Name!.ToLower().Contains(search) || p.Brand!.ToLower().Contains(search) || p.Model!.ToLower().Contains(search) ||
                                                   p.SKU!.ToLower() == search || p.ShortDescription!.ToLower().Contains(search));
                }
                else if (category is not null)
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
                return View(filteredProducts);
            }
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
