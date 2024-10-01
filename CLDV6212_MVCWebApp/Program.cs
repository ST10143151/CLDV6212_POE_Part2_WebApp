using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using ABC_Retailers.Services;

var builder = WebApplication.CreateBuilder(args);

// Access the configuration object
var configuration = builder.Configuration;

// Add services to the container.
builder.Services.AddControllersWithViews();

var azureStorageConnectionString = configuration.GetConnectionString("AzureStorage") 
    ?? throw new ArgumentNullException("AzureStorage connection string is not provided in the configuration.");

// Register services
builder.Services.AddSingleton(new BlobService(azureStorageConnectionString));
builder.Services.AddSingleton(new TableStorageService(azureStorageConnectionString));
builder.Services.AddSingleton<QueueService>(sp =>
{
    return new QueueService(azureStorageConnectionString, "orderstatuses");
});
builder.Services.AddSingleton<AzureFileShareService>(sp =>
{
    return new AzureFileShareService(azureStorageConnectionString, "productshare");
});
builder.Services.AddSingleton<CartService>();
builder.Services.AddSingleton(new UserService(azureStorageConnectionString));
builder.Services.AddSingleton<OrderService>();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

// Add memory cache for session management
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Set session timeout
    options.Cookie.HttpOnly = true; // Ensure the session cookie is only accessible to the server
    options.Cookie.IsEssential = true; // Mark the cookie as essential
});

// Configure authentication using cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Redirect to login page if unauthorized
        options.AccessDeniedPath = "/Account/AccessDenied"; // Redirect to access denied page if forbidden
    });

// Build the app
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession(); // Enable session before authentication and authorization
app.UseAuthentication(); // Enable authentication
app.UseAuthorization(); // Enable authorization

// Configure endpoint routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
