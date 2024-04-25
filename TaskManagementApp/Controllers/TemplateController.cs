using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TaskManagementApp.Controllers
{
    public class TemplateController : Controller
    {
        // GET: Template
        public ActionResult AccountCreation()
        {
            return View();
        }
    }
}