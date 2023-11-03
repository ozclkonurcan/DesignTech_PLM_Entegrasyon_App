using DesignTech_PLM_Entegrasyon_App.MVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace DesignTech_PLM_Entegrasyon_App.MVC.Controllers
{
    public class PLMController : Controller
    {
		private readonly IConfiguration _configuration;
		public PLMController(IConfiguration configuration)
		{
			_configuration = configuration;
		}
		public IActionResult Index()
        {
            return View();
        }


        public async Task<IActionResult> plmConnectionSettingsPage()
		{
			try
			{

				// Apsseting dosyasındaki bağlantıları alın
				string plmConnectionString = _configuration.GetConnectionString("Plm");
				if (plmConnectionString != null || plmConnectionString != "")
				{
					var path = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");

					// appsettings.json dosyasını oku
					var appSettingsText = await System.IO.File.ReadAllTextAsync(path);

					// JSON verisini bir JObject'a dönüştür
					JObject appSettingsJson = JObject.Parse(appSettingsText);

					bool isConnectionSuccessful = await TestDatabaseConnection(plmConnectionString);

					if (isConnectionSuccessful)
					{
						appSettingsJson["connectionType"] = true;
						ViewBag.ConnectionType = appSettingsJson["connectionType"];
						// Yeni ayarları JSON olarak dönüştür
						string newAppSettingsText = appSettingsJson.ToString(Newtonsoft.Json.Formatting.Indented);

						// Dosyayı güncelle
						await System.IO.File.WriteAllTextAsync(path, newAppSettingsText);
					}
					else
					{
						appSettingsJson["connectionType"] = false;
						ViewBag.ConnectionType = appSettingsJson["connectionType"];
						// Yeni ayarları JSON olarak dönüştür
						string newAppSettingsText = appSettingsJson.ToString(Newtonsoft.Json.Formatting.Indented);

						// Dosyayı güncelle
						await System.IO.File.WriteAllTextAsync(path, newAppSettingsText);
					}


					ViewBag.PlmConnectionString = plmConnectionString;
					return View();

				}

				// ViewBag ile bağlantıları görünüme aktarın

				return View();
			}
			catch (Exception ex)
			{
				TempData["SystemErrorMessage"] = ex.Message;
				return RedirectToAction("Index");
			}
		}



		//SQL AYARLARINI APPSETTINGS JSON A AKTARMA KISMI

		public async Task<JsonResult> sqlSettings(ConnectionStringModel model, string connectionType)
		{
			try
			{

				if (ModelState.IsValid && connectionType != "0")
				{
					// Bağlantı tipine göre bağlantı dizesini oluşturun
					string connectionString = "";
					if (connectionType == "Local")
					{
						connectionString = $"Data Source={model.Server}; Database={model.Catalog};Integrated Security=True;TrustServerCertificate=true;";
					}
					else if (connectionType == "Server")
					{
						connectionString = $"Persist Security Info=False;User ID={model.UserId};Password={model.Password};Initial Catalog={model.Catalog};Server={model.Server};TrustServerCertificate=True";
					}

					// Bağlantıyı test et
					bool isConnectionSuccessful = await TestDatabaseConnection(connectionString);

					if (isConnectionSuccessful)
					{
						var path = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");

						// appsettings.json dosyasını oku
						var appSettingsText = await System.IO.File.ReadAllTextAsync(path);

						// JSON verisini bir JObject'a dönüştür
						JObject appSettingsJson = JObject.Parse(appSettingsText);
						appSettingsJson["Catalog"] = model.Catalog;

						// ConnectionStrings bölümünü kontrol et ve eğer yoksa oluştur
						if (appSettingsJson["ConnectionStrings"] == null)
						{
							appSettingsJson["ConnectionStrings"] = new JObject();
						}


						if (appSettingsJson["connectionType"] == null)
						{
							appSettingsJson["connectionType"] = true; // Bağlantı başarılı
						}
						// Bağlantı dizesini appsettings.json dosyasına kaydet
						appSettingsJson["ConnectionStrings"]["Plm"] = connectionString;

						// "Catalog" adında bir değişken ekleyin veya güncelleyin
						appSettingsJson["Catalog"] = model.Catalog;

						// Yeni ayarları JSON olarak dönüştür
						string newAppSettingsText = appSettingsJson.ToString(Newtonsoft.Json.Formatting.Indented);

						// Dosyayı güncelle
						await System.IO.File.WriteAllTextAsync(path, newAppSettingsText);
						TempData["SuccessMessage"] = "Bağlantı başarılı.";

						return Json(appSettingsJson);
						//return RedirectToAction("Index","Home"); // Kullanıcıyı başka bir sayfaya yönlendirin veya başka bir işlem yapın
					}
					else
					{
						TempData["WarningMessage"] = "Bağlantınız başarısız. Sunucuya bağlantı sağlanamadı. Lütfen sunucuyu veya local hesabınızı kontrol ediniz.";
						return Json("Hata");
					}
				}
				TempData["WarningMessage"] = "Bağlantınız başarısız.";
				return Json("Hata");
			}
			catch (Exception ex)
			{
				TempData["SystemErrorMessage"] = ex.Message;
				return Json("Hata" + ex.Message);
			}
		}


		private async Task<bool> TestDatabaseConnection(string connectionString)
		{
			try
			{
				// SqlConnection nesnesini oluşturun
				using (SqlConnection connection = new SqlConnection(connectionString))
				{
					// Bağlantıyı açın ve hemen kapatın
					await connection.OpenAsync();
					connection.Close();
					return true; // Bağlantı başarılı ise true döndürün
				}
			}
			catch (Exception ex)
			{
				TempData["SystemErrorMessage"] = ex.Message;
				return false; // Bağlantı başarısız ise false döndürün
			}
		}
		//SQL AYARLARINI APPSETTINGS JSON A AKTARMA KISMI

	}
}
