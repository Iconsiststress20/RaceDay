using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace RaceDay.Web.Controllers
{
    public class EventsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public EventsController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            var token = HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token) ||
                HttpContext.Session.GetString("UserRole") != "Organiser")
            {
                return RedirectToAction("Login", "Account");
            }

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("api/Events");

            if (!response.IsSuccessStatusCode)
            {
                return View(new List<EventViewModel>());
            }

            var events = await response.Content
                .ReadFromJsonAsync<List<EventViewModel>>();

            return View(events ?? new List<EventViewModel>());
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var token = HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token) ||
                HttpContext.Session.GetString("UserRole") != "Organiser")
            {
                return RedirectToAction("Login", "Account");
            }

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Bearer", token);

            var response = await client.GetAsync("api/EventEnvironments");

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty,
                    "Unable to load event environments.");

                return View(new CreateEventViewModel());
            }

            var environments = await response.Content
                .ReadFromJsonAsync<List<EventEnvironmentViewModel>>();

            var model = new CreateEventViewModel
            {
                Environments = environments?.Select(environment =>
                    new SelectListItem
                    {
                        Value = environment.EnvironmentID.ToString(),
                        Text = environment.EnvironmentName
                    }).ToList() ?? new List<SelectListItem>()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateEventViewModel model)
        {
            var token = HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token) ||
                HttpContext.Session.GetString("UserRole") != "Organiser")
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                await LoadEnvironments(model, token);
                return View(model);
            }

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Bearer", token);

            var eventRequest = new
            {
                model.EventName,
                model.EventDescription,
                model.EnvironmentID,
                model.EventDate,
                model.EventTime,
                model.Location,
                model.MaxParticipants,
                model.EntryFee
            };

            var response = await client.PostAsJsonAsync(
                "api/Events", eventRequest);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty,
                    "Unable to create event. Please check your information.");

                await LoadEnvironments(model, token);
                return View(model);
            }

            return RedirectToAction("Index");
        }

        private async Task LoadEnvironments(
            CreateEventViewModel model, string token)
        {
            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Bearer", token);

            var response = await client.GetAsync("api/EventEnvironments");

            if (response.IsSuccessStatusCode)
            {
                var environments = await response.Content
                    .ReadFromJsonAsync<List<EventEnvironmentViewModel>>();

                model.Environments = environments?.Select(environment =>
                    new SelectListItem
                    {
                        Value = environment.EnvironmentID.ToString(),
                        Text = environment.EnvironmentName
                    }).ToList() ?? new List<SelectListItem>();
            }
        }

        private async Task LoadEditEnvironments(
    EditEventViewModel model, string token)
        {
            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Bearer", token);

            var response = await client.GetAsync("api/EventEnvironments");

            if (response.IsSuccessStatusCode)
            {
                var environments = await response.Content
                    .ReadFromJsonAsync<List<EventEnvironmentViewModel>>();

                model.Environments = environments?.Select(environment =>
                    new SelectListItem
                    {
                        Value = environment.EnvironmentID.ToString(),
                        Text = environment.EnvironmentName
                    }).ToList() ?? new List<SelectListItem>();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var token = HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token) ||
                HttpContext.Session.GetString("UserRole") != "Organiser")
            {
                return RedirectToAction("Login", "Account");
            }

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Bearer", token);

            var response = await client.GetAsync("api/Events");

            if (!response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }

            var events = await response.Content
                .ReadFromJsonAsync<List<EventViewModel>>();

            var raceEvent = events?.FirstOrDefault(e => e.EventID == id);

            if (raceEvent == null)
            {
                return NotFound();
            }

            var model = new EditEventViewModel
            {
                EventID = raceEvent.EventID,
                EventName = raceEvent.EventName,
                EventDescription = raceEvent.EventDescription,
                EnvironmentID = raceEvent.EnvironmentID,
                EventDate = raceEvent.EventDate,
                EventTime = raceEvent.EventTime,
                Location = raceEvent.Location,
                MaxParticipants = raceEvent.MaxParticipants,
                EntryFee = raceEvent.EntryFee,
                Status = raceEvent.Status
            };

            await LoadEditEnvironments(model, token);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditEventViewModel model)
        {
            var token = HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token) ||
                HttpContext.Session.GetString("UserRole") != "Organiser")
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                await LoadEditEnvironments(model, token);
                return View(model);
            }

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Bearer", token);

            var eventRequest = new
            {
                model.EventName,
                model.EventDescription,
                model.EnvironmentID,
                model.EventDate,
                model.EventTime,
                model.Location,
                model.MaxParticipants,
                model.EntryFee,
                model.Status
            };

            var response = await client.PutAsJsonAsync(
                $"api/Events/{model.EventID}", eventRequest);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }

            ModelState.AddModelError(string.Empty,
                "Unable to update event. Please try again.");

            await LoadEditEnvironments(model, token);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var token = HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token) ||
                HttpContext.Session.GetString("UserRole") != "Organiser")
            {
                return RedirectToAction("Login", "Account");
            }

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Bearer", token);

            var response = await client.DeleteAsync($"api/Events/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Unable to delete event.";
            }

            return RedirectToAction("Index");
        }



        [HttpGet]
        public async Task<IActionResult> Browse(
            string? search,
            DateTime? eventDate)
        {
            var token = HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token) ||
                HttpContext.Session.GetString("UserRole") != "Participant")
            {
                return RedirectToAction("Login", "Account");
            }

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("api/Events");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Unable to load upcoming events.";
                return View(new List<EventViewModel>());
            }

            var events = await response.Content
                .ReadFromJsonAsync<List<EventViewModel>>();

            var upcomingEvents = events?
                .Where(e => e.EventDate.Date >= DateTime.Today &&
                            e.Status.Equals("Upcoming",
                                StringComparison.OrdinalIgnoreCase))
                .ToList() ?? new List<EventViewModel>();

            if (!string.IsNullOrWhiteSpace(search))
            {
                upcomingEvents = upcomingEvents
                    .Where(e =>
                        e.EventName.Contains(search,
                            StringComparison.OrdinalIgnoreCase) ||
                        e.Location.Contains(search,
                            StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (eventDate.HasValue)
            {
                upcomingEvents = upcomingEvents
                    .Where(e => e.EventDate.Date == eventDate.Value.Date)
                    .ToList();
            }

            ViewBag.Search = search;
            ViewBag.EventDate = eventDate?.ToString("yyyy-MM-dd");

            return View(upcomingEvents
                .OrderBy(e => e.EventDate)
                .ToList());
        }


    }
}