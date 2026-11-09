using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;
using RaceDay.Web.Models;

namespace RaceDay.Web.Controllers
{
    public class EnrolmentsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public EnrolmentsController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private bool IsOrganiser()
        {
            return HttpContext.Session.GetString("UserRole") == "Organiser";
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!IsOrganiser())
            {
                return RedirectToAction("Login", "Account");
            }

            var token = HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Account");
            }

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var response = await client.GetAsync("api/EventEnrollments");

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return RedirectToAction("Login", "Account");
                }

                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.Error = "Could not load enrolments. Please try again.";
                    return View(new List<EnrolmentViewModel>());
                }

                var json = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var enrolments =
                    JsonSerializer.Deserialize<List<EnrolmentViewModel>>(
                        json, options)
                    ?? new List<EnrolmentViewModel>();

                return View(enrolments);
            }
            catch (HttpRequestException)
            {
                ViewBag.Error = "Could not connect to the RaceDay API. Make sure it is running.";
                return View(new List<EnrolmentViewModel>());
            }
            catch (TaskCanceledException)
            {
                ViewBag.Error = "The request took too long. Please try again.";
                return View(new List<EnrolmentViewModel>());
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(
            int id,
            EnrolmentViewModel model)
        {
            if (!IsOrganiser())
            {
                return RedirectToAction("Login", "Account");
            }

            var token = HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Account");
            }

            var client = _httpClientFactory.CreateClient("RaceDayAPI");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var request = new
                {
                    model.Status,
                    model.PaymentStatus,
                    model.RaceNumber
                };

                var response = await client.PutAsJsonAsync(
                    $"api/EventEnrollments/{id}", request);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Enrolment updated successfully.";
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();

                    TempData["Error"] =
                        $"Could not update enrolment. {error}";
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

    }
}

