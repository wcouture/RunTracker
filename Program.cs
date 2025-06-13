using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using RunTracker.Components;
using RunTracker.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddServerSideBlazor();


// builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
//     .AddCookie(options => {
//         options.Cookie.Name = "RunTrackerAuth";
//         options.LoginPath = "/login";
//         options.AccessDeniedPath = "/access-denied";
//         options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
//     });

builder.Services.AddBlazorBootstrap();
// builder.Services.AddAuthorization();
// builder.Services.AddCascadingAuthenticationState();

// Add password hasher service
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();

builder.Services.AddHttpClient("RunTracker", (serviceProvider, httpClient) =>
{
    if (builder.Environment.IsDevelopment())
    {
        httpClient.BaseAddress = new Uri("http://localhost:5239");
    }
    else
    {
        httpClient.BaseAddress = new Uri("https://runtrackerapi-deebfxdpbjdmdsh5.canadaeast-01.azurewebsites.net/");
    }
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

// app.UseAuthentication();
// app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
