using Microsoft.AspNetCore.Mvc;

namespace DATN.Controllers
{
    public class ErrorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
