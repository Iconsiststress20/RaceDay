
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using RaceDay.Web.Models;

namespace RaceDay.Web.Controllers
{
    public class ParticipantEnrollmentsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ParticipantEnrollmentsController(
            IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private string? GetToken()
        {
            if (HttpContext.Session.GetString("UserRole") != "Participant")
                return null;

            return HttpContext.Session.GetString("JWToken");
        }

        private HttpClient GetClient(string token)
        {
            var client = _httpClientFactory.CreateClient("RaceDayAPI");
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            return client;
        }

        [HttpGet]
        public async Task<IActionResult> Enrol(int eventId)
        {
            var token = GetToken();

            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            var client = GetClient(token);

            var eventResponse = await client.GetAsync("api/Events");
            var categoryResponse =
                await client.GetAsync("api/EventCategories");

            if (!eventResponse.IsSuccessStatusCode ||
                !categoryResponse.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] =
                    "Unable to load event information.";
                return RedirectToAction("Browse", "Events");
            }

            var events = await eventResponse.Content
                .ReadFromJsonAsync<List<EventViewModel>>();

            var categories = await categoryResponse.Content
                .ReadFromJsonAsync<List<EventCategoryViewModel>>();

            var selectedEvent = events?
                .FirstOrDefault(e => e.EventID == eventId);

            if (selectedEvent == null)
                return NotFound();

            var model = new ParticipantEnrollmentViewModel
            {
                EventID = selectedEvent.EventID,
                EventName = selectedEvent.EventName,
                Categories = categories?.Select(c => new
                    Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = c.CategoryID.ToString(),
                    Text = $"{c.CategoryName} ({c.DistanceKM} KM)"
                }).ToList() ?? new()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enrol(
            ParticipantEnrollmentViewModel model)
        {
            var token = GetToken();

            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            var client = GetClient(token);

            var request = new
            {
                model.EventID,
                model.CategoryID
            };

            var response = await client.PostAsJsonAsync(
                "api/ParticipantEnrollments", request);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] =
                    "You have successfully enrolled in the event.";

                return RedirectToAction("Index");
            }

            var error = await response.Content.ReadAsStringAsync();

            TempData["ErrorMessage"] =
                "Enrolment failed. You may already be registered, " +
                "the event may be full, or the information may be invalid.";

            return RedirectToAction("Browse", "Events");
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var token = GetToken();

            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            var client = GetClient(token);

            var response = await client.GetAsync(
                "api/ParticipantEnrollments");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error =
                    "Unable to retrieve your enrolments.";

                return View(new List<MyEnrollmentViewModel>());
            }

            var enrollments = await response.Content
                .ReadFromJsonAsync<List<MyEnrollmentViewModel>>();

            return View(enrollments ??
                new List<MyEnrollmentViewModel>());
        }
    }
}

