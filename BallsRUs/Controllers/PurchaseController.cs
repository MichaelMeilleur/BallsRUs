using BallsRUs.Entities;
using BallsRUs.Models.Purchase;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BallsRUs.Controllers
{
    public class PurchaseController : Controller
    {
        private readonly UserManager<User> _userManager;

        public PurchaseController(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> PurchaseUserInfo()
        {
            var userId = _userManager.GetUserId(User);

            // Récupérer l'utilisateur complet
            var user = await _userManager.FindByIdAsync(userId);

            PurchaseUserInfoVM purchaseInfo = new();

            if (user != null) 
            {
                purchaseInfo = new PurchaseUserInfoVM()
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                };
            }

            return View(purchaseInfo);
        }

        [HttpPost]
        public IActionResult PurchaseUserInfo(PurchaseUserInfoVM purchaseInfo)
        {
            return RedirectToAction("OrderConfirmation");
        }

        public IActionResult OrderConfirmation()
        {
            // Ajoutez une action pour afficher la confirmation de la commande
            return View();
        }
    }
}
