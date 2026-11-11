
var builder = WebApplication.CreateBuilder(args);

// Add MVC services
builder.Services.AddControllersWithViews();

// Allow the MVC application to communicate with RaceDay API

builder.Services.AddHttpClient("RaceDayAPI", client =>
{
    var apiUrl = builder.Configuration["RaceDayAPI:BaseUrl"]
                 ?? "http://localhost:5180/";

    client.BaseAddress = new Uri(apiUrl);
});


// Add session support
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddSingleton(
    new Azure.Storage.Blobs.BlobServiceClient(
        builder.Configuration["AzureStorage:ConnectionString"]
    )
);
var app = builder.Build();


// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// Enable sessions
app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}")
    .WithStaticAssets();

app.Run();
