using BallsRUs.Context;
using BallsRUs.Entities;
using BallsRUs.Models.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BallsRUs.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<User> signInManager;
        private readonly UserManager<User> userManager;
        private readonly RoleManager<IdentityRole<Guid>> roleManager;
        private readonly ApplicationDbContext context;

        public AccountController(SignInManager<User> signInManager,
           UserManager<User> userManager, RoleManager<IdentityRole<Guid>>
           roleManager, ApplicationDbContext context)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.context = context;
        }

        public IActionResult LogIn(string? returnUrl = "")
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LogIn(LogInVM vm, string? returnUrl = "")
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ReturnUrl = returnUrl;
                return View(vm);
            }

            try
            {
                var result = await signInManager.PasswordSignInAsync(
                    vm.NomUtilisateur, vm.MotDePasse, false, false);

                if (!result.Succeeded)
                {
                    ModelState.AddModelError(string.Empty, "Mauvais nom d'utilisateur ou mot de passe!");
                    ViewBag.ReturnUrl = returnUrl;
                    return View(vm);
                }

            }
            catch
            {
                ModelState.AddModelError(string.Empty, "SVP veuillez essayer de nouveau!");
                ViewBag.ReturnUrl = returnUrl;
                return View(vm);
            }

            var idUser = context.Users.Where(u => u.UserName == vm.NomUtilisateur).Select(u => u.Id).FirstOrDefault();
            var roleId = context.UserRoles.Where(r => r.UserId == idUser).Select(r => r.RoleId).FirstOrDefault();

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

            var role = "Utilisateur";

            var newUser = new User(vm.UserName);
            newUser.LastName = vm.LastName;
            newUser.FirstName = vm.FirstName;

            var result = await userManager.CreateAsync(newUser, vm.Password);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Erreur");
                return View(vm);
            }

            result = await userManager.AddToRoleAsync(newUser, role);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, $"Erreur dans la création d'un compte!");
                return View(vm);
            }

            return RedirectToAction("LogIn", "Account");
        }

        [HttpPost]
        public async Task<IActionResult> LogOut()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
