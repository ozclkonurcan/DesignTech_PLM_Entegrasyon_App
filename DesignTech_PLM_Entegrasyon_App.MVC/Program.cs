using DesignTech_PLM_Entegrasyon_App.MVC.Dtos;
using DesignTech_PLM_Entegrasyon_App.MVC.Helper;
using DesignTech_PLM_Entegrasyon_App.MVC.Hubs;
using DesignTech_PLM_Entegrasyon_App.MVC.Models.DapperContext;
using DesignTech_PLM_Entegrasyon_App.MVC.Models.SignalR;
using DesignTech_PLM_Entegrasyon_App.MVC.Services;
using DesignTech_PLM_Entegrasyon_App.MVC.Services.ApiServices;
using DesignTech_PLM_Entegrasyon_App.MVC.Services.Rabbitmq;
using DesignTech_PLM_Entegrasyon_App.MVC.Services.SignalR;
using Humanizer.Configuration;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Serilog;
using SqlKata.Compilers;
using SqlKata.Execution;
using System.Data;
using System.Globalization;
using System.Threading.RateLimiting;
using static System.Runtime.InteropServices.JavaScript.JSType;

var builder = WebApplication.CreateBuilder(args);




// Add services to the container.
builder.Services.AddControllersWithViews();

var configuration = builder.Configuration;



builder.Services.AddApplicationServices(configuration);



builder.Services.AddCors(opt =>
{
    opt.AddPolicy("CorsPolicy", builder =>
    {
        builder
        .AllowAnyHeader()
        .SetIsOriginAllowed((host) => true)
        .AllowCredentials();
    });
});
builder.Services.AddSignalR();



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

//builder.Services.AddSingleton<RabbitMQService>();
builder.Services.AddHttpClient<WindchillApiService>();
//builder.Services.AddScoped<IMessageProducer, MessageProducer>();

var currentMonthFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "logs", DateTime.Now.ToString("MMMM-yyyy", CultureInfo.InvariantCulture));
var logFileName = Path.Combine(currentMonthFolder, DateTime.Now.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture) + ".json");

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.File(new CustomJsonFormatter(), logFileName, rollingInterval: RollingInterval.Day, shared: true)
    .CreateLogger();



builder.Services.AddAuthorization(opt =>
{
    opt.AddPolicy("Admin", policy => policy.RequireClaim("Role", "Admin"));
});


builder.Services.AddApplicationServices(builder.Configuration);

builder.Services.AddAuthentication(options =>
{
	options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
	options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
	options.LoginPath = "/Login/Index"; // Giriþ sayfasýnýn yolu
    options.Events = new CookieAuthenticationEvents
    {
        OnValidatePrincipal = context =>
        {
            var loggedInUsername = context.Principal.Identity.Name;

            // Çýkýþ yapýlýrsa logla
            if (context.ShouldRenew)
            {
                LogService logService = new LogService(configuration);
                logService.AddNewLogEntry("Çýkýþ baþarýlý.", null, "Çýkýþ Yapýldý", loggedInUsername);
            }

            return Task.CompletedTask;
        }
    };
});
builder.Services.AddSession(options =>
{
	options.IdleTimeout = TimeSpan.FromMinutes(180);

    options.Cookie.HttpOnly = true;
	options.Cookie.IsEssential = true;
});

builder.Services.AddTransient<IDbConnection>(_ =>
    new SqlConnection(configuration.GetConnectionString("Plm")));

//try
//{
//    builder.Services.AddHostedService<ChangeNoticeService>();

//}
//catch (InvalidOperationException ex)
//{
//    LogService logService = new LogService(configuration);
//    logService.AddNewLogEntry("Auto Post Aktif edilemedi."+ex.Message, null, "Auto Post Aktif Deðil", null);
//}


//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("CorsPolicy", builder =>
//    {
//        builder.WithOrigins("https://localhost:44444", "https://localhost:55555").  
//         AllowAnyHeader().
//         AllowAnyMethod().
//         AllowCredentials();
//    });
//});

//builder.Services.AddHostedService<StatusService>();
//builder.Services.AddSignalR();




//Ýstek limitleyici gibi birþey ama çalýþtýramadým bunu sonra tekrar deneyeceðim
//builder.Services.AddRateLimiter(opt =>
//{

//    opt.AddPolicy("fixed-by-user", httpContext =>
//    {
//        // Log politika tetiklendi

//        LogService logService = new LogService(configuration);
//        var loggedInUsername = httpContext.User.Identity.Name;

//        logService.AddNewLogEntry("Fixed user rate limiter triggered", null, "Limit Aþýmý", loggedInUsername);


//        return RateLimitPartition.GetFixedWindowLimiter(
//            partitionKey: httpContext.User.Identity?.Name?.ToString(),
//            factory: _ => new FixedWindowRateLimiterOptions
//            {
//                PermitLimit = 1,
//                Window = TimeSpan.FromMinutes(1)
//            }
//        );
//    });

//});
var app = builder.Build();

Host.CreateDefaultBuilder(args)
     .ConfigureWebHostDefaults(webBuilder =>
     {
         webBuilder.UseStartup<Startup>();
         webBuilder.UseUrls("http://localhost:5002");
     }).Build();

//app.MapHub<StatusHub>("/statushub");

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}


//app.UseCors("CorsPolicy");
//app.UseRateLimiter();
app.UseCors("CorsPolicy");
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();



app.UseSession();
app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<FormDataListHub>("/FormDataListHub");
app.Run();
