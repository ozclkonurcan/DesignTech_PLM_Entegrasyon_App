using DesignTech_PLM_Entegrasyon_App.MVC.Models.Login;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using DesignTech_PLM_Entegrasyon_App.MVC.Helper;
using Microsoft.Extensions.Configuration;

namespace DesignTech_PLM_Entegrasyon_App.MVC.Controllers
{
    [AllowAnonymous]
    public class LoginController : Controller
	{
		private readonly IConfiguration _configuration;

        public LoginController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
		{
			return View();
		}


		public async Task<IActionResult> Login(string username, string password)
		{
		
            LogService logService = new LogService(_configuration);
            string logsPath = "wwwroot\\LoginJson\\users.json";
			var json = await System.IO.File.ReadAllTextAsync(logsPath);
			var users = JsonSerializer.Deserialize<List<User>>(json);

			// Kullanıcıyı bul.
			var user = users.FirstOrDefault(u => u.Username == username && u.Password == password);

			// Kullanıcı yoksa hata döndür.
			if (user == null)
			{
				TempData["ErrorMessage"] = "Kullanıcı adı veya parola hatalı.";
				return RedirectToAction("Index", "Login");
				//return Json(new { success = false, message = "Kullanıcı adı veya parola hatalı." });
			}

			if (user != null)
			{
				// Session'a kaydet 

				HttpContext.Session.SetString("isLoggedIn", "true");
				HttpContext.Session.SetString("username", user.Username);

				var claims = new List<Claim>
						{
						  new Claim(ClaimTypes.Name, user.Username)
						};

				var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

				var authProperties = new AuthenticationProperties
				{
				

					ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(180),
					IsPersistent = true
				
				};

				await HttpContext.SignInAsync(
				  CookieAuthenticationDefaults.AuthenticationScheme,
				  new ClaimsPrincipal(claimsIdentity),
				  authProperties);
                logService.AddNewLogEntry("Giriş başarılı.", null, "Giriş Yapıldı",user.Username);

                TempData["SuccessMessage"] = "Giriş başarılı.";
				// Başarılı girişten sonra ana sayfaya yönlendir
				return RedirectToAction("Index", "Home");
			}
			else
			{
                // Kullanıcı bulunamadı
                logService.AddNewLogEntry("Hatalı kullanıcı adı veya parola denemesi.", null, "Hatalı giriş denemesi.", username);
                TempData["ErrorMessage"] = "Kullanıcı adı veya parola hatalı.";
				return RedirectToAction("Index", "Login");
			}
          

        }

		[HttpPost]
		public async Task<IActionResult> Logout()
		{
            LogService logService = new LogService(_configuration);
            var loggedInUsername = HttpContext.User.Identity.Name;

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

			HttpContext.Session.Clear();
            logService.AddNewLogEntry("Çıkış başarılı.", null, "Çıkış Yapıldı", loggedInUsername);
            return RedirectToAction("Index", "Login");
			//return Json(new { success = true, message = "Çıkış başarılı." });

		}





	}
}
