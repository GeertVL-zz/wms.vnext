using Microsoft.AspNetCore.Mvc;

namespace ProductAPI.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return new RedirectResult("~/swagger/ui");
        }
    }
}
