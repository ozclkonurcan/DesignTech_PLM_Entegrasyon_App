using DesignTech_PLM_Entegrasyon_App.MVC.Helper;
using DesignTech_PLM_Entegrasyon_App.MVC.Models.DapperContext;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SqlKata.Compilers;
using SqlKata.Execution;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


var configuration = builder.Configuration;

//builder.Services.AddSingleton<IUygulamaDbContextFactory, UygulamaDbContextFactory>();

//builder.Services.AddDbContext<Context>(options =>
//{

//    var connectionString = configuration.GetConnectionString("Plm");
//    options.UseSqlServer(connectionString);
//});

builder.Services.AddScoped(factory =>
{
    return new QueryFactory
    {
        Compiler = new SqlServerCompiler(),
        Connection = new SqlConnection(configuration.GetConnectionString("Plm"))
    };
});

var currentMonthFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "logs", DateTime.Now.ToString("MMMM-yyyy", CultureInfo.InvariantCulture));
var logFileName = Path.Combine(currentMonthFolder, DateTime.Now.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture) + ".json");

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.File(new CustomJsonFormatter(), logFileName, rollingInterval: RollingInterval.Day, shared: true)
    .CreateLogger();



builder.Services.AddAuthorization();

builder.Services.AddAuthentication(options =>
{
	options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
	options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
	options.LoginPath = "/Login/Index"; // Giriþ sayfasýnýn yolu
});
builder.Services.AddSession(options =>
{
	options.IdleTimeout = TimeSpan.FromMinutes(180); 
	options.Cookie.HttpOnly = true;
	options.Cookie.IsEssential = true;
});
var app = builder.Build();



// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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
