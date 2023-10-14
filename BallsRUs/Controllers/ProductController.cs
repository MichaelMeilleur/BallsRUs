using Microsoft.AspNetCore.Mvc;

namespace BallsRUs.Controllers
{
    public class ProductController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(Guid productId)
        {
            return View();
        }
    }
}
