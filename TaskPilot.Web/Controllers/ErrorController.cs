using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TaskPilot.Web.Controllers
{
    [AllowAnonymous]
    public class ErrorController : Controller
    {
        public IActionResult AccessDenied()
        {
            return View();
        }
        public IActionResult NotFound()
        {
            return View();
        }

        public IActionResult Back()
        {
            return RedirectToAction("Login", "Account");
        }
    }
}
