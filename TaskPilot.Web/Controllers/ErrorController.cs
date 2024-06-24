using Microsoft.AspNetCore.Mvc;
using TaskPilot.Application.Common.Utility;

namespace TaskPilot.Web.Controllers
{
    public class ErrorController : Controller
    {
        [Route("/Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            ViewBag.Title = "Error 403: Access Denied";
            ViewBag.ErrorMessage = Message.ACCESS_DENIED;

            switch (statusCode)
            {
                case 404:
                    ViewBag.Title = "Error 404: Not Found";
                    ViewBag.ErrorMessage = Message.NOT_FOUND;
                    break;
            }
            return View("Error");
        }
    }
}
