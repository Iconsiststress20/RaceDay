
using Microsoft.AspNetCore.Mvc;

namespace RaceDay.Web.Controllers
{
    public class ParticipantController : Controller
    {
        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(role))
            {
                return RedirectToAction("Login", "Account");
            }

            if (role != "Participant")
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }
    }
}
