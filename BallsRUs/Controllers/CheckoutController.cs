using Microsoft.AspNetCore.Mvc;

namespace BallsRUs.Controllers
{
    public class CheckoutController : Controller
    {
        public IActionResult Information()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Information(bool MettreLeViewModelIci)
        {
            return View();
        }
    }
}
