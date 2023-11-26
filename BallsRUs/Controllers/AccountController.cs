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
        public async Task<IActionResult> LogIn(LogInVM vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            try
            {
                var result = await _signInManager.PasswordSignInAsync(
                    vm.NomUtilisateur!, vm.MotDePasse!, false, false);

                if (!result.Succeeded)
                {
                    ModelState.AddModelError(string.Empty, "Le nom d'utilisateur et le mot de passe ne correspondent pas. Veuillez réessayer");
                    return View(vm);
                }
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Une erreur est survenue. Veuillez réessayer.");
                return View(vm);
            }

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

                var vm = new AccountDetailsVM()
                {
                    FirstName = userToShow.FirstName,
                    LastName = userToShow.LastName,
                    Email = userToShow.Email,
                    PhoneNumber = userToShow.PhoneNumber ?? "Aucun numéro",
                    Address = _context.Addresses.FirstOrDefault(x => x.UserId == userId) ?? null
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

                if(userToChange.PhoneNumber != vm.PhoneNumber && !string.IsNullOrWhiteSpace(vm.PhoneNumber)) 
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

                var address = new Address()
                {
                    Id = Guid.NewGuid(),
                    StateProvince = vm.Address.StateProvince!,
                    Street = vm.Address.Street!,
                    City = vm.Address.City!,
                    Country = vm.Address.Country!,
                    PostalCode = vm.Address.PostalCode!,
                    UserId = userId
                };

                userToChange.Address = address;

                _context.Addresses.Add(address);
                _context.SaveChanges();

                return View(vm);
            }
            catch
            {
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
                    Address = adress
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

                if (addressToChange.Street != vm.Address.Street && !string.IsNullOrWhiteSpace(vm.Address.Street))
                {
                    addressToChange.Street = vm.Address.Street;
                }

                if (addressToChange.City != vm.Address.City && !string.IsNullOrWhiteSpace(vm.Address.City))
                {
                    addressToChange.City = vm.Address.City;
                }

                if (addressToChange.StateProvince != vm.Address.StateProvince && !string.IsNullOrWhiteSpace(vm.Address.StateProvince))
                {
                    addressToChange.StateProvince = vm.Address.StateProvince;
                }

                if (addressToChange.Country != vm.Address.Country && !string.IsNullOrWhiteSpace(vm.Address.Country))
                {
                    addressToChange.Country = vm.Address.Country;
                }

                if (addressToChange.PostalCode != vm.Address.PostalCode && !string.IsNullOrWhiteSpace(vm.Address.PostalCode))
                {
                    addressToChange.PostalCode = vm.Address.PostalCode;
                }

                _context.SaveChanges();
                return View(vm);
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Une erreur est survenue. Veuillez réessayer.");
                return View(vm);
            }

        }
    }
}
