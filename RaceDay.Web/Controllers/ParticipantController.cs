

using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;
using RaceDay.Web.Models;

namespace RaceDay.Web.Controllers
{
    public class ParticipantController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ParticipantController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(role) || role != "Participant")
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        public async Task<IActionResult> Results()
        {
            var role = HttpContext.Session.GetString("UserRole");
            var token = HttpContext.Session.GetString("JWToken");

            if (role != "Participant" || string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var client = _httpClientFactory.CreateClient("RaceDayAPI");

                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync("api/Results/my-results");

                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.Error = "Unable to retrieve your results. Please try again.";
                    return View(new List<ResultViewModel>());
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var results = await response.Content.ReadFromJsonAsync<List<ResultViewModel>>(options);

                return View(results ?? new List<ResultViewModel>());
            }
            catch (HttpRequestException)
            {
                ViewBag.Error = "Unable to connect to the RaceDay API.";
                return View(new List<ResultViewModel>());
            }
        }
    }
}
