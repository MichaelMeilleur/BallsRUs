using BallsRUs.Context;
using BallsRUs.Entities;
using BallsRUs.Models.Account;
using BallsRUs.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BallsRUs.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;

        public AccountController(SignInManager<User> signInManager, UserManager<User> userManager, ApplicationDbContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
        }

        [AllowAnonymous]
        public IActionResult LogIn()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> LogIn(LogInVM vm, bool redirectToCheckout)
        {
            if (!ModelState.IsValid)
                return View(vm);

            try
            {
                string? anonymUserShoppingCartId = HttpContext.Session.GetString(Constants.SHOPPING_CART_SESSION_KEY);
                Guid? anonymUserShoppingCartGuid = null;

                if (!string.IsNullOrWhiteSpace(anonymUserShoppingCartId))
                {
                    if (Guid.TryParse(anonymUserShoppingCartId, out Guid sessionShoppingCartGuid))
                        anonymUserShoppingCartGuid = sessionShoppingCartGuid;
                    else
                        throw new Exception("The ID of the user isn't valid.");
                }

                var result = await _signInManager.PasswordSignInAsync(vm.NomUtilisateur!, vm.MotDePasse!, false, false);

                if (!result.Succeeded)
                {
                    ModelState.AddModelError(string.Empty, "Le nom d'utilisateur et le mot de passe ne correspondent pas. Veuillez réessayer");
                    return View(vm);
                }

                if (anonymUserShoppingCartGuid is not null)
                {
                    if (vm.NomUtilisateur is not null)
                    {
                        var user = await _userManager.FindByNameAsync(vm.NomUtilisateur);

                        if (user is null)
                            throw new Exception("The user was not found.");

                        Guid userGuid = user.Id;

                        ShoppingCart? accountShoppingCart = _context.ShoppingCarts.FirstOrDefault(sc => sc.UserId == userGuid);
                        ShoppingCart? anonymUserShoppingCart = _context.ShoppingCarts.FirstOrDefault(sc => sc.Id == anonymUserShoppingCartGuid);

                        if (anonymUserShoppingCart is not null)
                        {
                            if (accountShoppingCart is null)
                            {
                                anonymUserShoppingCart.UserId = userGuid;
                                _context.SaveChanges();
                            }
                            else
                            {
                                if (accountShoppingCart.ProductsQuantity == 0)
                                {
                                    anonymUserShoppingCart.UserId = userGuid;
                                    DeleteShoppingCart(accountShoppingCart.Id);
                                }
                                else
                                {
                                    DeleteShoppingCart(anonymUserShoppingCart.Id);
                                }
                            }

                            HttpContext.Session.Remove(Constants.SHOPPING_CART_SESSION_KEY);
                        }
                        else
                        {
                            throw new Exception("The ID of the anonym user's shopping cart isn't valid.");
                        }
                    }
                    else
                    {
                        throw new Exception("The ID of the user isn't valid.");
                    }
                }
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Une erreur est survenue. Veuillez réessayer.");
                return View(vm);
            }

            if (redirectToCheckout)
                return RedirectToAction("Index", "ShoppingCart");
            else
                return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterVM vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            bool userAlreadyExists = _context.Users.Any(u => u.UserName == vm.UserName);

            if (userAlreadyExists)
            {
                ModelState.AddModelError(string.Empty, "Un utilisateur avec ce courriel existe déjà.");
                return View(vm);
            }

            try
            {
                var newUser = new User(vm.UserName!)
                {
                    FirstName = vm.FirstName,
                    LastName = vm.LastName,
                    Email = vm.UserName,
                    NormalizedEmail = vm.UserName!.ToUpper()
                };

                var result = await _userManager.CreateAsync(newUser, vm.Password!);

                if (vm.addAddress)
                {
                    var address = new Address()
                    {
                        StateProvince = vm.StateProvince!,
                        Street = vm.Street!,
                        City = vm.City!,
                        Country = vm.Country!,
                        PostalCode = vm.PostalCode!,
                        UserId = newUser.Id
                    };
                    _context.Addresses.Add(address);
                    _context.SaveChanges();
                }

                if (!result.Succeeded)
                {
                    ModelState.AddModelError(string.Empty, "Impossible de créer le compte. Veuillez réessayer.");
                    return View(vm);
                }

                result = await _userManager.AddToRoleAsync(newUser, Constants.ROLE_UTILISATEUR);

                if (!result.Succeeded)
                {
                    ModelState.AddModelError(string.Empty, "Impossible de créer le compte. Veuillez réessayer.");
                    return View(vm);
                }
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Une erreur est survenue. Veuillez réessayer.");
                return View(vm);
            }

            return RedirectToAction("LogIn", "Account");
        }

        [HttpPost]
        public async Task<IActionResult> LogOut()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }

        private void DeleteShoppingCart(Guid shoppingCartId)
        {
            ShoppingCart? shoppingCart = _context.ShoppingCarts.Find(shoppingCartId);

            if (shoppingCart is null)
                throw new ArgumentOutOfRangeException(nameof(shoppingCartId));

            if (shoppingCart.ProductsQuantity == 0)
            {
                _context.ShoppingCarts.Remove(shoppingCart);
                _context.SaveChanges();
            }
            else
            {
                IEnumerable<ShoppingCartItem> items = _context.ShoppingCartItems.Where(i => i.ShoppingCartId == shoppingCartId);
                foreach (ShoppingCartItem item in items)
                {
                    Product? product = _context.Products.Find(item.ProductId);

                    if (product is null)
                        throw new Exception("The product of the item wasn't found.");

                    product.Quantity += item.Quantity;

                    _context.ShoppingCartItems.Remove(item);
                }

                _context.ShoppingCarts.Remove(shoppingCart);
                _context.SaveChanges();
            }
        }
    }
}
