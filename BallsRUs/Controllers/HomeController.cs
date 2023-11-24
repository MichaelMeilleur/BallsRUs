using BallsRUs.Entities;
using BallsRUs.Models;
using BallsRUs.Models.Purchase;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BallsRUs.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public HomeController(ILogger<HomeController> logger, UserManager<User> userManager, SignInManager<User> signInManager)
        {
            this._signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        //Get : à déplacer dans le controller shopping cart
        public IActionResult ConnexionPurchase()
        {
            ConnexionPurchaseVM model = new ConnexionPurchaseVM();

            if (User.Identity.IsAuthenticated)
            {
                // L'utilisateur est connecté
                model.estConnecte = true;
                model.NomUtilisateur = User.Identity.Name; // Vous pouvez récupérer d'autres informations ici
            }
            else
            {
                // L'utilisateur n'est pas connecté
                model.estConnecte = false;
            }

            return PartialView(model);
        }

        //Post : à déplacer dans le controller shopping cart
        [HttpPost]
        public async Task<IActionResult> LogIn(ConnexionPurchaseVM vm)
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

            return RedirectToAction("PurchaseUserInfo", "Purchase");
        }
    }
}