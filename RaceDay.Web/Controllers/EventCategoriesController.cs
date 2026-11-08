using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace RaceDay.Web.Controllers
{
    public class EventCategoriesController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public EventCategoriesController(
            IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private bool IsOrganiser()
        {
            return !string.IsNullOrEmpty(
                HttpContext.Session.GetString("JWToken")) &&
                HttpContext.Session.GetString("UserRole") == "Organiser";
        }

        private HttpClient CreateApiClient()
        {
            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    HttpContext.Session.GetString("JWToken"));

            return client;
        }

        public async Task<IActionResult> Index()
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            var client = CreateApiClient();

            var response = await client.GetAsync("api/EventCategories");

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] =
                    "Unable to load event categories.";

                return View(new List<EventCategoryViewModel>());
            }

            var categories = await response.Content
                .ReadFromJsonAsync<List<EventCategoryViewModel>>();

            return View(categories ?? new List<EventCategoryViewModel>());
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            EventCategoryViewModel model)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            if (model.MinAge.HasValue && model.MaxAge.HasValue &&
                model.MinAge > model.MaxAge)
            {
                ModelState.AddModelError(
                    nameof(model.MaxAge),
                    "Maximum age must be greater than or equal to minimum age.");
            }

            if (!ModelState.IsValid)
                return View(model);

            var client = CreateApiClient();

            var request = new
            {
                model.CategoryName,
                model.Description,
                model.DistanceKM,
                model.MinAge,
                model.MaxAge
            };

            var response = await client.PostAsJsonAsync(
                "api/EventCategories", request);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Unable to create category. Please try again.");

                return View(model);
            }

            TempData["SuccessMessage"] = "Category created successfully.";

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            var client = CreateApiClient();

            var response = await client.GetAsync("api/EventCategories");

            if (!response.IsSuccessStatusCode)
                return RedirectToAction("Index");

            var categories = await response.Content
                .ReadFromJsonAsync<List<EventCategoryViewModel>>();

            var category = categories?
                .FirstOrDefault(c => c.CategoryID == id);

            if (category == null)
                return NotFound();

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            EventCategoryViewModel model)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            if (model.MinAge.HasValue && model.MaxAge.HasValue &&
                model.MinAge > model.MaxAge)
            {
                ModelState.AddModelError(
                    nameof(model.MaxAge),
                    "Maximum age must be greater than or equal to minimum age.");
            }

            if (!ModelState.IsValid)
                return View(model);

            var client = CreateApiClient();

            var request = new
            {
                model.CategoryName,
                model.Description,
                model.DistanceKM,
                model.MinAge,
                model.MaxAge
            };

            var response = await client.PutAsJsonAsync(
                $"api/EventCategories/{model.CategoryID}", request);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Unable to update category. Please try again.");

                return View(model);
            }

            TempData["SuccessMessage"] = "Category updated successfully.";

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            var client = CreateApiClient();

            var response = await client.DeleteAsync(
                $"api/EventCategories/{id}");

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] =
                    "Category deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] =
                    "Unable to delete category. It may be linked to an event.";
            }

            return RedirectToAction("Index");
        }
    }
}