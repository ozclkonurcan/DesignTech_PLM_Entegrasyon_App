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
						  new Claim(ClaimTypes.Name, user.Username),
						  new Claim(ClaimTypes.Role,user.Role),
						  new Claim("Role",user.Role)
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
		[Authorize(Policy = "Admin")]
		[HttpGet]
		public async Task<IActionResult> User()
		{
			try
			{
			


				var response = GetUsersFromJsonFile();

				ViewBag.usersList = response;

		
				return View();
			}
			catch (Exception)
			{
			return View();

			}


		}
		[Authorize(Policy = "Admin")]
		[HttpPost]
		public async Task<IActionResult> AddUser(User model)
		{
			try
			{
			
				if(model != null)
				{
					model.Id = Guid.NewGuid();
					await SaveUsersToJsonFile(model);
				}

				TempData["SuccessMessage"] = "Kullanıcı başarıyla eklendi";
				return RedirectToAction("User", "Login");
			}
			catch (Exception ex)
			{

				TempData["ErrorMessage"] = "Kullanıcı ekleme işlemi başarısız. "+ex.Message;
				return RedirectToAction("User", "Login");

			}


		}

        private List<User> GetUsersFromJsonFile()
        {
            string userFilePath = "wwwroot/LoginJson/users.json";
            var json = System.IO.File.ReadAllText(userFilePath);
            return JsonSerializer.Deserialize<List<User>>(json);
        }

        private async Task SaveUsersToJsonFile(User newUser)
        {
            string userFilePath = "wwwroot/LoginJson/users.json";

            // Dosya var mı kontrol et
            if (System.IO.File.Exists(userFilePath))
            {
                // Dosyayı oku
                string existingJson = await System.IO.File.ReadAllTextAsync(userFilePath);

                // Json'dan objeye dönüştür
                List<User> existingUsers = JsonSerializer.Deserialize<List<User>>(existingJson);

                // Aynı kullanıcı var mı kontrol et
                if (!existingUsers.Any(u => u.Username == newUser.Username))
                {
                    // Yeni kullanıcıyı ekle
                    existingUsers.Add(newUser);

                    // Json'a dönüştür ve dosyaya yaz
                    var updatedJson = JsonSerializer.Serialize(existingUsers);
                    await System.IO.File.WriteAllTextAsync(userFilePath, updatedJson);
                }
                else
                {
                    // Aynı kullanıcı zaten var, işlem yapma
                    Console.WriteLine("Bu kullanıcı zaten var.");
                }
            }
            else
            {
                // Dosya yoksa yeni dosya oluştur ve kullanıcıyı ekle
                var json = JsonSerializer.Serialize(new List<User> { newUser });
                await System.IO.File.WriteAllTextAsync(userFilePath, json);
            }
        }



        [Authorize(Policy = "Admin")]
		[HttpPost]
		public async Task<JsonResult> UpdateUser()
		{
			try
			{

				return Json("");
			}
			catch (Exception)
			{
				return Json("");

			}

		}


        [HttpPost]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> RemoveUser(Guid Id)
        {
            var userList = GetUsersFromJsonFile();

            var user = userList.FirstOrDefault(x => x.Id == Id);

            if (user != null)
            {
                try
                {
                    userList.Remove(user);

                    await SaveUsersToJson(userList);

                    TempData["Success"] = "Kullanıcı silindi";

                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Hata oluştu: " + ex.Message;
                }
            }

            return RedirectToAction("User", "Login");
        }
        [Authorize(Policy = "Admin")]
        private async Task SaveUsersToJson(List<User> users)
        {
            string filePath = "wwwroot/LoginJson/users.json";

            string json = JsonSerializer.Serialize(users);

            await System.IO.File.WriteAllTextAsync(filePath, json);
        }

        //[Authorize(Policy = "Admin")]
        //[HttpPost]
        //public async Task<IActionResult> RemoveUser(User model)
        //{
        //	try
        //	{
        //		var userList = GetUsersFromJsonFile();

        //		var user = userList.FirstOrDefault(x => x.Username == model.Username && x.Role == model.Role);
        //		if (user != null)
        //		{
        //			userList.Remove(user);

        //			await SaveUsersToJsonFile(user);
        //		}
        //              TempData["SuccessMessage"] = "Kullanıcı başarıyla silindi";
        //              return RedirectToAction("User", "Login");
        //          }
        //	catch (Exception ex)
        //	{
        //              TempData["SuccessMessage"] = "Kullanıcı silme işlemi başarısız. "+ex.Message;
        //              return RedirectToAction("User", "Login");

        //          }

        //}





    }
}
