﻿using BallsRUs.Context;
using BallsRUs.Entities;
using BallsRUs.Models.ShoppingCart;
using BallsRUs.Utilities;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Security.Claims;

namespace BallsRUs.Controllers
{
    public class ShoppingCartController : Controller
    {
        private ApplicationDbContext _context;

        public ShoppingCartController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            Guid scId = GetShoppingCartId();

            ShoppingCart? shoppingCart = _context.ShoppingCarts.FirstOrDefault(sc => sc.Id == scId);

            if (shoppingCart is null)
                throw new Exception("The shopping cart is not valid.");

            List<ShoppingCartItem>? items = _context.ShoppingCartItems.Where(sci => sci.ShoppingCartId == scId).ToList();
            List<ShoppingCartProductVM>? productsVM = new();

            int quantity = 0;
            decimal? productsCost = 0m;

            if (items.Any())
            {
                foreach (ShoppingCartItem item in items)
                {
                    Product? product = _context.Products.FirstOrDefault(p => p.Id == item.ProductId);

                    if (product is null)
                        throw new Exception("The product is not valid.");

                    ShoppingCartProductVM p = new ShoppingCartProductVM
                    {
                        Id = item.Id,
                        ProductId = product.Id,
                        Name = product.Name,
                        RetailPrice = product.RetailPrice,
                        DiscountedPrice = product.DiscountedPrice,
                        ImagePath = product.ImagePath,
                        Quantity = item.Quantity
                    };
                    productsVM.Add(p);

                    quantity += item.Quantity;
                    productsCost += product.DiscountedPrice is not null ? product.DiscountedPrice * item.Quantity : product.RetailPrice * item.Quantity;
                }
            }

            ShoppingCartListVM vm = new ShoppingCartListVM
            {
                Quantity = quantity,
                ShippingCost = Constants.ESTIMATED_SHIPPING_COST,
                ProductsCost = productsCost,
                Items = productsVM 
            };

            return View(vm);
        }

        public IActionResult AddProductToCart(Guid productId)
        {
            Guid scId = GetShoppingCartId();

            Product? product = _context.Products.Find(productId);

            if (product is null)
                throw new ArgumentOutOfRangeException(nameof(productId));

            ShoppingCart? shoppingCart = _context.ShoppingCarts.Find(scId);

            if (shoppingCart is null)
                throw new Exception("The shopping cart is not valid.");

            ShoppingCartItem? item = _context.ShoppingCartItems.FirstOrDefault(i => i.ShoppingCartId == scId && i.ProductId == product.Id);

            if (item is null)
            {
                ShoppingCartItem cartItem = new ShoppingCartItem
                {
                    Quantity = 1,
                    CreationDate = DateTime.Now,
                    ShoppingCartId = scId,
                    ProductId = product.Id
                };

                shoppingCart.ProductsQuantity++;

                _context.ShoppingCartItems.Add(cartItem);
                _context.SaveChanges();
            }
            else
            {
                shoppingCart.ProductsQuantity++;
                item.Quantity++;

                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Index));
        }

        private Guid GetShoppingCartId()
        {
            if (User.Identity!.IsAuthenticated)
            {
                string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (userId is null)
                    throw new Exception("No user was found.");

                if (Guid.TryParse(userId, out Guid userGuid))
                {
                    ShoppingCart? shoppingCart = _context.ShoppingCarts.FirstOrDefault(sc => sc.UserId == userGuid);

                    if (shoppingCart is null)
                    {
                        ShoppingCart newSC = new ShoppingCart
                        {
                            Id = Guid.NewGuid(),
                            ProductsQuantity = 0,
                            CreationDate = DateTime.Now,
                            UserId = userGuid
                        };

                        _context.ShoppingCarts.Add(newSC);
                        _context.SaveChanges();

                        return newSC.Id;
                    }
                    else
                    {
                        return shoppingCart.Id;
                    }
                }
                else
                {
                    throw new Exception("The ID of the user isn't valid.");
                }
            }
            else
            {
                string? shoppingCartId = HttpContext.Session.GetString(Constants.SHOPPING_CART_SESSION_KEY);

                if (shoppingCartId is null)
                {
                    ShoppingCart newSC = new ShoppingCart
                    {
                        Id = Guid.NewGuid(),
                        ProductsQuantity = 0,
                        CreationDate = DateTime.Now
                    };

                    _context.ShoppingCarts.Add(newSC);
                    _context.SaveChanges();

                    HttpContext.Session.SetString(Constants.SHOPPING_CART_SESSION_KEY, newSC.Id.ToString());

                    return newSC.Id;
                }
                else
                {
                    if (Guid.TryParse(shoppingCartId, out Guid shoppingCartGuid))
                        return shoppingCartGuid;
                    else
                        throw new Exception("The key of the cart isn't valid.");
                }
            }
        }
    }
}
