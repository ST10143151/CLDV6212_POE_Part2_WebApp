using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using ABC_Retailers.Services;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

builder.Services.AddControllersWithViews();

var azureStorageConnectionString = configuration.GetConnectionString("AzureStorage") 
    ?? throw new ArgumentNullException("AzureStorage connection string is not provided in the configuration.");

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

builder.Services.AddHttpClient();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); 
    options.Cookie.HttpOnly = true; 
    options.Cookie.IsEssential = true; 
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; 
        options.AccessDeniedPath = "/Account/AccessDenied"; 
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
