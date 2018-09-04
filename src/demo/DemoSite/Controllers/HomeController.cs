using Microsoft.AspNetCore.Mvc;

namespace DemoSite.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
