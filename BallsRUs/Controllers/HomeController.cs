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

        public HomeController(ILogger<HomeController> logger, UserManager<User> userManager)
        {
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
    }
}