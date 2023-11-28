using BallsRUs.Context;
using BallsRUs.Entities;
using BallsRUs.Models.Account;
using BallsRUs.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using System.Text.RegularExpressions;

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

        public IActionResult Details()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdString != null)
            {
                var userId = Guid.Parse(userIdString);
                var userToShow = _context.Users.Find(userId);
                var addressToShow = _context.Addresses.FirstOrDefault(x => x.UserId == userId) ?? null;

                AccountDetailsVM vm = new AccountDetailsVM()
                {
                    FirstName = userToShow.FirstName,
                    LastName = userToShow.LastName,
                    Email = userToShow.Email,
                    PhoneNumber = userToShow.PhoneNumber ?? "Aucun numéro",
                    AddressCity = addressToShow is not null ? addressToShow.City : null,
                    AddressCountry = addressToShow is not null ? addressToShow.Country : null,
                    AddressPostalCode = addressToShow is not null ? addressToShow.PostalCode : null,
                    AddressStateProvince = addressToShow is not null ? addressToShow.StateProvince : null,
                    AddressStreet = addressToShow is not null ? addressToShow.Street : null,
                };
                return View(vm);

            }
            return View();
        }

        public IActionResult Editinfo()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdString != null)
            {
                var userId = Guid.Parse(userIdString);
                var userToShow = _context.Users.Find(userId);

                var vm = new AccountDetailsVM()
                {
                    FirstName = userToShow.FirstName,
                    LastName = userToShow.LastName,
                    Email = userToShow.Email,
                    PhoneNumber = userToShow.PhoneNumber ?? "Aucun numéro"
                };
                return View(vm);
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Editinfo(AccountDetailsVM vm)
        {
            try
            {
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userId = Guid.Parse(userIdString);
                var userToChange = _context.Users.Find(userId);

                if (userToChange.FirstName != vm.FirstName && !string.IsNullOrWhiteSpace(vm.FirstName))
                {
                    userToChange.FirstName = vm.FirstName;
                }

                if (userToChange.LastName != vm.LastName && !string.IsNullOrWhiteSpace(vm.LastName))
                {
                    userToChange.LastName = vm.LastName;
                }

                if (userToChange.PhoneNumber != vm.PhoneNumber && !string.IsNullOrWhiteSpace(vm.PhoneNumber))
                {
                    userToChange.PhoneNumber = vm.PhoneNumber;
                }

                if (userToChange.Email != vm.Email && !string.IsNullOrWhiteSpace(vm.Email))
                {
                    bool userAlreadyExists = _context.Users.Any(u => u.UserName == vm.Email);

                    if (userAlreadyExists)
                    {
                        ModelState.AddModelError(string.Empty, "Un utilisateur avec ce courriel existe déjà.");
                        vm.Email = userToChange.Email!;
                        return View(vm);
                    }
                    else
                    {
                        userToChange.Email = vm.Email;
                        userToChange.UserName = vm.Email;
                        userToChange.NormalizedUserName = vm.Email!.ToUpper();
                        userToChange.NormalizedEmail = vm.Email!.ToUpper();
                    }
                }

                if (string.IsNullOrWhiteSpace(vm.FirstName) || string.IsNullOrWhiteSpace(vm.LastName) || string.IsNullOrWhiteSpace(vm.Email) || string.IsNullOrWhiteSpace(vm.PhoneNumber))
                {
                    TempData["SuccessMessage"] = null;
                }
                else
                    TempData["SuccessMessage"] = "Sauvegarde réussie";

                _context.SaveChanges();
                return View(vm);
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Une erreur est survenue. Veuillez réessayer.");
                return View(vm);
            }

        }

        public IActionResult AddAddress()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddAddress(AccountDetailsVM vm)
        {
            try
            {
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userId = Guid.Parse(userIdString);
                var userToChange = _context.Users.Find(userId);

                if (!IsCanadianPostalCodeValid(vm.AddressPostalCode))
                    TempData["SuccessMessage"] = null;
                else
                {
                    var address = new Address()
                    {
                        Id = Guid.NewGuid(),
                        StateProvince = vm.AddressStateProvince!,
                        Street = vm.AddressStreet!,
                        City = vm.AddressCity!,
                        Country = vm.AddressCountry!,
                        PostalCode = vm.AddressPostalCode!,
                        UserId = userId
                    };

                    userToChange.Address = address;
                    _context.Addresses.Add(address);
                    _context.SaveChanges();
                    TempData["SuccessMessage"] = "Sauvegarde réussie";
                }


                return View(vm);
            }
            catch
            {
                TempData["SuccessMessage"] = null;
                ModelState.AddModelError(string.Empty, "Une erreur est survenue. Veuillez réessayer.");
                return View(vm);
            }

        }

        public IActionResult EditAddress()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdString != null)
            {
                var userId = Guid.Parse(userIdString);
                var userToShow = _context.Users.Find(userId);
                var adress = _context.Addresses.FirstOrDefault(a => a.UserId == userId);

                var vm = new AccountDetailsVM()
                {
                    AddressStreet = adress.Street,
                    AddressCity = adress.City,
                    AddressCountry = adress.Country,
                    AddressStateProvince = adress.StateProvince,
                    AddressPostalCode = adress.PostalCode
                };
                return View(vm);
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> EditAddress(AccountDetailsVM vm)
        {
            try
            {
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userId = Guid.Parse(userIdString);
                var userToChange = _context.Users.Find(userId);
                var addressToChange = _context.Addresses.FirstOrDefault(address => address.UserId == userId);

                if (addressToChange.Street != vm.AddressStreet && !string.IsNullOrWhiteSpace(vm.AddressStreet))
                {
                    addressToChange.Street = vm.AddressStreet;
                }

                if (addressToChange.City != vm.AddressCity && !string.IsNullOrWhiteSpace(vm.AddressCity))
                {
                    addressToChange.City = vm.AddressCity;
                }

                if (addressToChange.StateProvince != vm.AddressStateProvince && !string.IsNullOrWhiteSpace(vm.AddressStateProvince))
                {
                    addressToChange.StateProvince = vm.AddressStateProvince;
                }

                if (addressToChange.Country != vm.AddressCountry && !string.IsNullOrWhiteSpace(vm.AddressCountry))
                {
                    addressToChange.Country = vm.AddressCountry;
                }

                if (addressToChange.PostalCode != vm.AddressPostalCode && !string.IsNullOrWhiteSpace(vm.AddressPostalCode))
                {
                    addressToChange.PostalCode = vm.AddressPostalCode;
                }

                if (string.IsNullOrWhiteSpace(vm.AddressStreet) || string.IsNullOrWhiteSpace(vm.AddressCity) || string.IsNullOrWhiteSpace(vm.AddressStateProvince)
                    || string.IsNullOrWhiteSpace(vm.AddressCountry) || string.IsNullOrWhiteSpace(vm.AddressPostalCode))
                {
                    TempData["SuccessMessage"] = null;
                }
                else
                    TempData["SuccessMessage"] = "Sauvegarde réussie";

                if (!IsCanadianPostalCodeValid(vm.AddressPostalCode))
                    TempData["SuccessMessage"] = null;

                _context.SaveChanges();
                return View(vm);
            }
            catch
            {
                TempData["SuccessMessage"] = null;
                ModelState.AddModelError(string.Empty, "Une erreur est survenue. Veuillez réessayer.");
                return View(vm);
            }

        }

        public IActionResult OrdersHistory()
        {
            Guid userId = GetCurrentUserId();
            IEnumerable<OrderManageVM> orders = _context.Orders.Where(x => x.UserId == userId).Select(order => new OrderManageVM
            {
                Id = order.Id,
                ConfirmationDate = order.ConfirmationDate,
                CreationDate = order.CreationDate,
                EmailAddress = order.EmailAddress,
                FirstName = order.FirstName,
                LastName = order.LastName,
                ModificationDate = order.ModificationDate,
                Number = order.Number,
                PaymentDate = order.PaymentDate,
                PhoneNumber = order.PhoneNumber,
                ProductQuantity = order.ProductQuantity,
                ProductsCost = order.ProductsCost,
                ShippingCost = order.ShippingCost,
                Status = order.Status,
                SubTotal = order.SubTotal,
                Taxes = order.Taxes,
                Total = order.Total,
                User = order.User
            });

            return View(orders);
        }

        private Guid GetCurrentUserId()
        {
            return (Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value));
        }

        private bool IsCanadianPostalCodeValid(string postalCode)
        {
            var canadianPostalCodeRegex = new Regex(@"^[A-Za-z]\d[A-Za-z] \d[A-Za-z]\d$");
            return !string.IsNullOrEmpty(postalCode) && canadianPostalCodeRegex.IsMatch(postalCode);
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
