using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TaskPilot.Web.Controllers
{
    public class ErrorController : Controller
    {
        [Route("/Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            ViewBag.Title = "Error: Access Denied";
            ViewBag.ErrorMessage = "Hey! You're not supposed to be here!";

            switch (statusCode)
            {
                case 404:
                    ViewBag.Title = "Error 404: Not Found";
                    ViewBag.ErrorMessage = "The page you trying to access is not found.";
                    break;
                case 401:
                    ViewBag.Title = "Error 403: Access Denied";
                    ViewBag.ErrorMessage = "Hey! You're not supposed to be here!";
                    break;
                case 400:
                    ViewBag.Title = "Error 400: Bad Request";
                    ViewBag.ErrorMessage = "Something went wrong!";
                    break;
            }
            return View("Error");
        }
    }
}
