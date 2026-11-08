
using Microsoft.AspNetCore.Mvc;

namespace RaceDay.Web.Controllers
{
    public class OrganiserController : Controller
    {
        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(role))
            {
                return RedirectToAction("Login", "Account");
            }

            if (role != "Organiser")
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }
    }
}

