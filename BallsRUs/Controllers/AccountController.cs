using BallsRUs.Context;
using BallsRUs.Entities;
using BallsRUs.Models.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BallsRUs.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly ApplicationDbContext _context;

        public AccountController(SignInManager<User> signInManager,
           UserManager<User> userManager, RoleManager<IdentityRole<Guid>>
           roleManager, ApplicationDbContext context)
        {
            this._signInManager = signInManager;
            this._userManager = userManager;
            this._roleManager = roleManager;
            this._context = context;
        }

        public IActionResult LogIn(string? returnUrl = "")
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
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

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
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

                result = await _userManager.AddToRoleAsync(newUser, "Utilisateur");

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
    }
}
