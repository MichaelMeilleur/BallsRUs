using Microsoft.AspNetCore.Mvc;

namespace BallsRUs.Controllers
{
    public class CheckoutController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
