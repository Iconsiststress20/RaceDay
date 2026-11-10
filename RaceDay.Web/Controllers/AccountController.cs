
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using System.Net.Http.Json;

namespace RaceDay.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AccountController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            var response = await client.PostAsJsonAsync(
                "api/Users/login", model);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty,
                    "Invalid email or password.");

                return View(model);
            }

            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

            if (result == null || string.IsNullOrWhiteSpace(result.Token))
            {
                ModelState.AddModelError(string.Empty,
                    "Unable to complete login.");

                return View(model);
            }

            HttpContext.Session.SetString("JWToken", result.Token);
            HttpContext.Session.SetString("UserID", result.User.UserID.ToString());
            HttpContext.Session.SetString("UserName", result.User.FirstName);
            HttpContext.Session.SetString("UserRole", result.User.Role);

            if (result.User.Role == "Organiser")
            {
                return RedirectToAction("Index", "Organiser");
            }

            return RedirectToAction("Index", "Participant");
        }


        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            var registrationData = new
            {
                model.FirstName,
                model.LastName,
                model.Email,
                model.Password,
                model.PhoneNumber
            };

            var response = await client.PostAsJsonAsync(
                "api/Users/register", registrationData);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] =
                    "Your Participant account has been created. You can now log in.";

                return RedirectToAction("Login");
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                ModelState.AddModelError(
                    "Email",
                    "An account with this email already exists.");
            }
            else
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Registration failed. Please try again.");
            }

            return View(model);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Login", "Account");
        }


    }
}
