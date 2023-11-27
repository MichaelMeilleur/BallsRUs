using BallsRUs.Context;
using BallsRUs.Entities;
using BallsRUs.Models.Checkout;
using BallsRUs.Utilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;

namespace BallsRUs.Controllers
{
    public class CheckoutController : Controller
    {
        private ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public CheckoutController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Information()
        {
            if (User.Identity!.IsAuthenticated)
            {
                string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (userId is null)
                    throw new Exception("The ID of the user is not valid.");

                if (Guid.TryParse(userId, out Guid userGuid))
                {
                    User? user = await _userManager.FindByIdAsync(userId);

                    if (user is null)
                        throw new Exception("The user was not found.");

                    CheckoutInformationVM vm = new CheckoutInformationVM()
                    {
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        EmailAddress = user.Email,
                        PhoneNumber = user.PhoneNumber
                    };

                    Address? address = _context.Addresses.FirstOrDefault(a => a.UserId == userGuid);

                    if (address is not null)
                    {
                        vm.AddressStreet = address.Street;
                        vm.AddressCity = address.City;
                        vm.AddressStateProvince = address.StateProvince;
                        vm.AddressCountry = address.Country;
                        vm.AddressPostalCode = address.PostalCode;
                        vm.HasExistingAddress = true;
                    }
                    else
                    {
                        vm.HasExistingAddress = false;
                    }

                    return View(vm);
                }
                else
                {
                    throw new Exception("The ID of the user isn't valid.");
                }
            }

            return View();
        }

        [HttpPost]
        public IActionResult Information(CheckoutInformationVM vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            if (User.Identity!.IsAuthenticated)
            {
                string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (userId is null)
                    throw new Exception("The ID of the user is not valid.");

                if (Guid.TryParse(userId, out Guid userGuid))
                {
                    Address? address;

                    if (vm.HasExistingAddress && vm.UseExistingAddress)
                    {
                        address = _context.Addresses.FirstOrDefault(a => a.UserId == userGuid);

                        if (address is null)
                            throw new Exception("The address of the user wasn't found.");
                    }
                    else
                    {
                        address = new Address()
                        {
                            Street = vm.AddressStreet!,
                            City = vm.AddressCity!,
                            StateProvince = vm.AddressStateProvince!,
                            Country = vm.AddressCountry!,
                            PostalCode = vm.AddressPostalCode!
                        };

                        if (vm.SaveAddress)
                            address.UserId = userGuid;
                    }

                    Order order = new Order()
                    {
                        Number = GenerateRandomOrderNumber(),
                        FirstName = vm.FirstName,
                        LastName = vm.LastName,
                        EmailAddress = vm.EmailAddress,
                        PhoneNumber = vm.PhoneNumber,
                        AddressId = address.Id,
                        UserId = userGuid,
                        CreationDate = DateTime.UtcNow,
                        ShippingCost = Constants.ESTIMATED_SHIPPING_COST
                    };

                    ShoppingCart? shoppingCart = _context.ShoppingCarts.FirstOrDefault(sc => sc.UserId == userGuid);

                    if (shoppingCart is null)
                        throw new Exception("The shopping cart of the user wasn't found");

                    decimal productCost = 0.0m;
                    int productQuantity = 0;

                    List<ShoppingCartItem> shoppingCartItems = _context.ShoppingCartItems.Where(sci => sci.ShoppingCartId == shoppingCart.Id).ToList();

                    foreach (var item in shoppingCartItems)
                    {
                        Product? product = _context.Products.Find(item.ProductId);

                        if (product is null)
                            throw new Exception("The product of the item wasn't found.");

                        productCost += product.DiscountedPrice is not null
                                        ? (decimal)(product.DiscountedPrice! * item.Quantity!)
                                        : (decimal)(product.RetailPrice! * item.Quantity!);

                        productQuantity += item.Quantity;

                        _context.ShoppingCartItems.Remove(item);
                    }

                    _context.ShoppingCarts.Remove(shoppingCart);

                    order.ProductQuantity = productQuantity;
                    order.ProductsCost = productCost;
                    order.SubTotal = productCost + order.ShippingCost;
                    order.Taxes = order.SubTotal * Constants.TAXES_PERCENTAGE;
                    order.Total = order.SubTotal + order.Taxes;

                    _context.Order.Add(order);
                    _context.SaveChanges();
                }
                else
                {
                    throw new Exception("The ID of the user isn't valid.");
                }
            }
            else
            {
                Address address = new Address()
                {
                    Street = vm.AddressStreet!,
                    City = vm.AddressCity!,
                    StateProvince = vm.AddressStateProvince!,
                    Country = vm.AddressCountry!,
                    PostalCode = vm.AddressPostalCode!
                };

                Order order = new Order()
                {
                    Number = GenerateRandomOrderNumber(),
                    FirstName = vm.FirstName,
                    LastName = vm.LastName,
                    EmailAddress = vm.EmailAddress,
                    PhoneNumber = vm.PhoneNumber,
                    AddressId = address.Id,
                    CreationDate = DateTime.UtcNow,
                    ShippingCost = Constants.ESTIMATED_SHIPPING_COST
                };

                string? shoppingCartId = HttpContext.Session.GetString(Constants.SHOPPING_CART_SESSION_KEY);

                if (shoppingCartId is null)
                    throw new Exception("The session key for the ID of the shopping cart doesn't exist.");

                if (Guid.TryParse(shoppingCartId, out Guid shoppingCartGuid))
                {
                    ShoppingCart? shoppingCart = _context.ShoppingCarts.FirstOrDefault(sc => sc.Id == shoppingCartGuid);

                    if (shoppingCart is null)
                        throw new Exception("The shopping cart of the anonym user wasn't found");

                    decimal productCost = 0.0m;
                    int productQuantity = 0;

                    List<ShoppingCartItem> shoppingCartItems = _context.ShoppingCartItems.Where(sci => sci.ShoppingCartId == shoppingCart.Id).ToList();

                    foreach (var item in shoppingCartItems)
                    {
                        Product? product = _context.Products.Find(item.ProductId);

                        if (product is null)
                            throw new Exception("The product of the item wasn't found.");

                        productCost += product.DiscountedPrice is not null
                                        ? (decimal)(product.DiscountedPrice! * item.Quantity!)
                                        : (decimal)(product.RetailPrice! * item.Quantity!);

                        productQuantity += item.Quantity;

                        _context.ShoppingCartItems.Remove(item);
                    }

                    _context.ShoppingCarts.Remove(shoppingCart);

                    order.ProductQuantity = productQuantity;
                    order.ProductsCost = productCost;
                    order.SubTotal = productCost + order.ShippingCost;
                    order.Taxes = order.SubTotal * Constants.TAXES_PERCENTAGE;
                    order.Total = order.SubTotal + order.Taxes;

                    _context.Order.Add(order);
                    _context.SaveChanges();
                }
                else
                {
                    throw new Exception("The ID of shopping cart isn't valid.");
                }
            }

            return RedirectToAction(nameof(Confirmation)); // Rediriger à la page de confirmation (trouver une façon de rediriger les clients anonymes).
        }

        public IActionResult Confirmation()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Confirmation(bool MettreLeViewModelIci)
        {
            return View();
        }

        private string GenerateRandomOrderNumber()
        {
            const string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random random = new Random();
            StringBuilder stringBuilder = new StringBuilder(16);

            for (int i = 0; i < 16; i++)
            {
                int index = random.Next(characters.Length);
                stringBuilder.Append(characters[index]);
            }

            return stringBuilder.ToString();
        }
    }
}