using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using RaceDay.Web.Models;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;


namespace RaceDay.Web.Controllers
{
    public class ProfileController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly BlobServiceClient _blobServiceClient;


        public ProfileController(
            IHttpClientFactory httpClientFactory,
            BlobServiceClient blobServiceClient)
        {
            _httpClientFactory = httpClientFactory;
            _blobServiceClient = blobServiceClient;
        }

        

        private HttpClient GetClient()
        {
            var token = HttpContext.Session.GetString("JWToken");

            var client = _httpClientFactory.CreateClient("RaceDayAPI");
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            return client;
        }

        private bool IsAllowedRole()
        {
            var role = HttpContext.Session.GetString("UserRole");

            return role == "Participant" || role == "Organiser";
        }

        public async Task<IActionResult> Index()
        {
            if (!IsAllowedRole())
                return RedirectToAction("Login", "Account");

            var client = GetClient();

            var response = await client.GetAsync("api/Users/me");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Unable to load your profile.";
                return View();
            }

            var profile = await response.Content.ReadFromJsonAsync<ProfileViewModel>();

            if (profile == null)
            {
                TempData["Error"] = "Profile not found.";
                return View();
            }

            return View(profile);
        }


        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            if (!IsAllowedRole())
                return RedirectToAction("Login", "Account");

            var response = await GetClient().GetAsync("api/Users/me");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Unable to load profile.";
                return RedirectToAction("Index");
            }

            var profile = await response.Content.ReadFromJsonAsync<ProfileViewModel>();

            if (profile == null)
                return RedirectToAction("Index");

            var model = new EditProfileViewModel
            {
                FirstName = profile.FirstName,
                LastName = profile.LastName,
                PhoneNumber = profile.PhoneNumber
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditProfileViewModel model)
        {
            if (!IsAllowedRole())
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
                return View(model);

            var response = await GetClient().PutAsJsonAsync("api/Users/me", model);

            if (response.IsSuccessStatusCode)
            {
                HttpContext.Session.SetString("UserName", model.FirstName);
                TempData["Success"] = "Profile updated successfully.";
                return RedirectToAction("Index");
            }

            TempData["Error"] = "Unable to update profile.";
            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadPicture(ProfilePictureViewModel model)
        {
            if (!IsAllowedRole())
                return RedirectToAction("Login", "Account");

            if (model.Picture == null || model.Picture.Length == 0)
            {
                TempData["Error"] = "Please select a picture.";
                return RedirectToAction("Index");
            }

            if (model.Picture.Length > 5 * 1024 * 1024)
            {
                TempData["Error"] = "Picture must be smaller than 5 MB.";
                return RedirectToAction("Index");
            }

            var extension = Path.GetExtension(model.Picture.FileName).ToLowerInvariant();

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };

            if (!allowedExtensions.Contains(extension))
            {
                TempData["Error"] = "Only JPG and PNG pictures are allowed.";
                return RedirectToAction("Index");
            }

            var userId = HttpContext.Session.GetString("UserID");

            if (string.IsNullOrWhiteSpace(userId))
                return RedirectToAction("Login", "Account");

            var blobName = $"{userId}/{Guid.NewGuid()}{extension}";

            try
            {
                var container = _blobServiceClient.GetBlobContainerClient(
                    "profile-pictures");

                var blob = container.GetBlobClient(blobName);

                using var stream = model.Picture.OpenReadStream();

                await blob.UploadAsync(stream, new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders
                    {
                        ContentType = extension == ".png"
                            ? "image/png"
                            : "image/jpeg"
                    }
                });

                var response = await GetClient().PutAsJsonAsync(
                    "api/Users/me/profile-picture",
                    new { BlobName = blobName });

                if (!response.IsSuccessStatusCode)
                {
                    await blob.DeleteIfExistsAsync();
                    TempData["Error"] = "Unable to save your profile picture.";
                    return RedirectToAction("Index");
                }

                TempData["Success"] = "Profile picture uploaded successfully.";
            }
            catch
            {
                TempData["Error"] = "An error occurred while uploading your picture.";
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Picture()
        {
            if (!IsAllowedRole())
                return Unauthorized();

            var response = await GetClient().GetAsync("api/Users/me");

            if (!response.IsSuccessStatusCode)
                return NotFound();

            var profile = await response.Content.ReadFromJsonAsync<ProfileViewModel>();

            if (string.IsNullOrWhiteSpace(profile?.ProfilePictureBlobName))
                return NotFound();

            var container = _blobServiceClient.GetBlobContainerClient(
                "profile-pictures");

            var blob = container.GetBlobClient(profile.ProfilePictureBlobName);

            if (!await blob.ExistsAsync())
                return NotFound();

            var download = await blob.DownloadContentAsync();

            var contentType = download.Value.Details.ContentType
                ?? "application/octet-stream";

            return File(download.Value.Content.ToArray(), contentType);
        }


    }

    public class ProfileViewModel
    {
        public int UserID { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string? PhoneNumber { get; set; }
        public string Role { get; set; } = "";
        public string? ProfilePictureBlobName { get; set; }
    }
}
