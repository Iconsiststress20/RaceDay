using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;

namespace RaceDay.Web.Controllers
{
    public class ResultsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ResultsController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private bool IsOrganiser()
        {
            return HttpContext.Session.GetString("UserRole") == "Organiser";
        }

        private HttpClient? GetApiClient()
        {
            var token = HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
                return null;

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            return client;
        }

        // Display existing results and enrolments without results
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            var client = GetApiClient();

            if (client == null)
                return RedirectToAction("Login", "Account");

            try
            {
                var resultsResponse = await client.GetAsync("api/Results");

                if (resultsResponse.StatusCode == HttpStatusCode.Unauthorized)
                    return RedirectToAction("Login", "Account");

                if (!resultsResponse.IsSuccessStatusCode)
                {
                    ViewBag.Error = "Could not load results. Please try again.";
                    return View(new List<ResultViewModel>());
                }

                var results = await resultsResponse.Content
                    .ReadFromJsonAsync<List<ResultViewModel>>(_jsonOptions)
                    ?? new List<ResultViewModel>();

                var enrolmentsResponse =
                    await client.GetAsync("api/EventEnrollments");

                if (!enrolmentsResponse.IsSuccessStatusCode)
                {
                    ViewBag.Error =
                        "Results loaded, but available enrolments could not be loaded.";

                    return View(results);
                }

                var enrolments = await enrolmentsResponse.Content
                    .ReadFromJsonAsync<List<EnrolmentViewModel>>(_jsonOptions)
                    ?? new List<EnrolmentViewModel>();

                var resultEnrollmentIds = results
                    .Select(r => r.EnrollmentID)
                    .ToHashSet();

                var availableEnrolments = enrolments
                    .Where(e => !resultEnrollmentIds.Contains(e.EnrollmentID))
                    .ToList();

                ViewBag.AvailableEnrolments = availableEnrolments;

                return View(results);
            }
            catch (HttpRequestException)
            {
                ViewBag.Error =
                    "Could not connect to the RaceDay API. Make sure it is running.";

                return View(new List<ResultViewModel>());
            }
            catch (TaskCanceledException)
            {
                ViewBag.Error =
                    "The request took too long. Please try again.";

                return View(new List<ResultViewModel>());
            }
        }

        // Add a new result
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ResultViewModel model)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            var client = GetApiClient();

            if (client == null)
                return RedirectToAction("Login", "Account");

            if (model.EnrollmentID <= 0)
            {
                TempData["Error"] = "Please select an enrolment.";
                return RedirectToAction(nameof(Index));
            }

            var request = new
            {
                model.EnrollmentID,
                model.FinishTime,
                model.OverallPosition,
                model.CategoryPosition,
                model.Points,
                model.Remarks
            };

            try
            {
                var response = await client.PostAsJsonAsync(
                    "api/Results", request);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Result recorded successfully.";
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();

                    TempData["Error"] =
                        $"Could not record result. {error}";
                }
            }
            catch (HttpRequestException)
            {
                TempData["Error"] =
                    "Could not connect to the RaceDay API.";
            }
            catch (TaskCanceledException)
            {
                TempData["Error"] =
                    "The request took too long. Please try again.";
            }

            return RedirectToAction(nameof(Index));
        }

        // Display the edit page
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            var client = GetApiClient();

            if (client == null)
                return RedirectToAction("Login", "Account");

            try
            {
                var response = await client.GetAsync("api/Results");

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                    return RedirectToAction("Login", "Account");

                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Could not load results.";
                    return RedirectToAction(nameof(Index));
                }

                var results = await response.Content
                    .ReadFromJsonAsync<List<ResultViewModel>>(_jsonOptions)
                    ?? new List<ResultViewModel>();

                var result = results.FirstOrDefault(r => r.ResultID == id);

                if (result == null)
                {
                    TempData["Error"] = "Result not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(result);
            }
            catch (HttpRequestException)
            {
                TempData["Error"] = "Could not connect to the RaceDay API.";
                return RedirectToAction(nameof(Index));
            }
            catch (TaskCanceledException)
            {
                TempData["Error"] = "The request took too long.";
                return RedirectToAction(nameof(Index));
            }
        }

        // Save edited result
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ResultViewModel model)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            var client = GetApiClient();

            if (client == null)
                return RedirectToAction("Login", "Account");

            var request = new
            {
                model.FinishTime,
                model.OverallPosition,
                model.CategoryPosition,
                model.Points,
                model.Remarks
            };

            try
            {
                var response = await client.PutAsJsonAsync(
                    $"api/Results/{id}", request);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Result updated successfully.";
                    return RedirectToAction(nameof(Index));
                }

                var error = await response.Content.ReadAsStringAsync();

                TempData["Error"] = $"Could not update result. {error}";
                return View(model);
            }
            catch (HttpRequestException)
            {
                TempData["Error"] = "Could not connect to the RaceDay API.";
                return View(model);
            }
            catch (TaskCanceledException)
            {
                TempData["Error"] = "The request took too long.";
                return View(model);
            }
        }

        // Delete a result
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsOrganiser())
                return RedirectToAction("Login", "Account");

            var client = GetApiClient();

            if (client == null)
                return RedirectToAction("Login", "Account");

            try
            {
                var response = await client.DeleteAsync(
                    $"api/Results/{id}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Result deleted successfully.";
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();

                    TempData["Error"] =
                        $"Could not delete result. {error}";
                }
            }
            catch (HttpRequestException)
            {
                TempData["Error"] = "Could not connect to the RaceDay API.";
            }
            catch (TaskCanceledException)
            {
                TempData["Error"] = "The request took too long.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
