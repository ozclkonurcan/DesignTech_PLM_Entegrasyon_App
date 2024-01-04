using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace DesignTech_PLM_Entegrasyon_App.MVC.Controllers.Modules.WindowsFormSettings
{
	public class WindowsFormSettingsController : Controller
	{

        private readonly IConfiguration _configuration;

        public WindowsFormSettingsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            try
            {
                // appsettings.json dosyasının yolunu al
                var appSettingsPath = "appsettings.json";

                // appsettings.json dosyasını oku
                var json = System.IO.File.ReadAllText(appSettingsPath);
                var jsonObj = JObject.Parse(json);

                // Dosya yolu ve dosya adını al
                var windowsFormFileUrl = jsonObj["WindowsFormFileUrl"]?.ToString();

            
                    var targetJson = System.IO.File.ReadAllText(windowsFormFileUrl);
                    var targetJsonObj = JObject.Parse(targetJson);

                // Değişiklikleri yap
                ViewBag.Catalog = targetJsonObj["Catalog"].ToString();
                ViewBag.ServerName = targetJsonObj["ServerName"].ToString();
                ViewBag.KullaniciAdi = targetJsonObj["KullaniciAdi"].ToString();
                ViewBag.Parola = targetJsonObj["Parola"].ToString();
                ViewBag.ApiURL = targetJsonObj["APIConnectionINFO"]["ApiURL"].ToString();
                ViewBag.ApiEndpoint = targetJsonObj["APIConnectionINFO"]["ApiEndpoint"].ToString();
                ViewBag.API = targetJsonObj["APIConnectionINFO"]["API"].ToString();
                ViewBag.ConnectionType = Convert.ToBoolean(targetJsonObj["ConnectionType"]);

                    // View'e geçir
                    ViewBag.TargetJson = targetJsonObj.ToString();
                    return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "HATA! " + ex.Message;
                return View();

            }
        }


        public IActionResult windowsFormURLSettings(string jsonFile)
        {
            try
            {
                if (jsonFile != null && jsonFile.Length > 0)
                {
                 
                    // appsettings.json dosyasını oku
                    var appSettingsPath = "appsettings.json";
                    var json = System.IO.File.ReadAllText(appSettingsPath);
                    var jsonObj = JObject.Parse(json);

                    // Eğer WindowsFormFileUrl zaten varsa güncelle, yoksa ekle
                    if (jsonObj["WindowsFormFileUrl"] == null)
                    {
                        jsonObj.Add("WindowsFormFileUrl", jsonFile);
                    }
                    else
                    {
                        jsonObj["WindowsFormFileUrl"] = jsonFile;
                    }

                    // Değişiklikleri kaydet
                    System.IO.File.WriteAllText(appSettingsPath, jsonObj.ToString());

                    // Başarılı mesajı
                    TempData["SuccessMessage"] = "appsettings.json dosyası güncellendi. Dosya Yolu: " + jsonFile;
                }
                else
                {
                    TempData["ErrorMessage"] = "Hata! Dosya seçimi yapılmadı.";
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Hata! " + ex.Message;
                return RedirectToAction("Index");
            }
        }
    
    


        public IActionResult windowssFormSQLSettings(string Catalog, string ServerName, string KullaniciAdi,string Parola,string ConnectionStrings)
        {
            try
            {
                // appsettings.json dosyasının yolunu al
                var appSettingsPath = "appsettings.json";

                // appsettings.json dosyasını oku
                var json = System.IO.File.ReadAllText(appSettingsPath);
                var jsonObj = JObject.Parse(json);

                // Dosya yolu ve dosya adını al
                var windowsFormFileUrl = jsonObj["WindowsFormFileUrl"]?.ToString();

                // Hedef json dosyasını oku
                if (!string.IsNullOrEmpty(windowsFormFileUrl))
                {
                    var targetJson = System.IO.File.ReadAllText(windowsFormFileUrl);
                    var targetJsonObj = JObject.Parse(targetJson);

                    // Değişiklikleri yap
                    targetJsonObj["Catalog"] = Catalog;
                    targetJsonObj["ServerName"] = ServerName;
                    targetJsonObj["KullaniciAdi"] = KullaniciAdi;
                    targetJsonObj["Parola"] = Parola;

                    if (!string.IsNullOrEmpty(KullaniciAdi))
                    {
                        ConnectionStrings = $"Persist Security Info=False;User ID={KullaniciAdi};Password={Parola};Initial Catalog={Catalog};Server={ServerName}";
                    }
                    else
                    {
                        ConnectionStrings = $"Data Source={ServerName};Initial Catalog={Catalog};Integrated Security=True";
                    }


                    targetJsonObj["ConnectionStrings"]["Plm"] = ConnectionStrings;
      

                    // Değişiklikleri kaydet
                    System.IO.File.WriteAllText(windowsFormFileUrl, targetJsonObj.ToString());
                    System.IO.File.SetAttributes(windowsFormFileUrl, FileAttributes.Normal);
                    // View'e geçir
                    ViewBag.TargetJson = targetJsonObj.ToString();
                }
                else
                {
                    ViewBag.TargetJson = "Hedef dosya bulunamadı.";
                }

                TempData["SuccessMessage"] = "SQL Ayarları güncellendi.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Hata! " + ex.Message;
                return RedirectToAction("Index");
            }
        }



        public IActionResult windowssFormAPISettings(string ApiURL, string ApiEndpoint)
        {
            try
            {
                // appsettings.json dosyasının yolunu al
                var appSettingsPath = "appsettings.json";

                // appsettings.json dosyasını oku
                var json = System.IO.File.ReadAllText(appSettingsPath);
                var jsonObj = JObject.Parse(json);

                // Dosya yolu ve dosya adını al
                var windowsFormFileUrl = jsonObj["WindowsFormFileUrl"]?.ToString();

                // Hedef json dosyasını oku
                if (!string.IsNullOrEmpty(windowsFormFileUrl))
                {
                    var targetJson = System.IO.File.ReadAllText(windowsFormFileUrl);
                    var targetJsonObj = JObject.Parse(targetJson);

                    // Değişiklikleri yap
 
                    targetJsonObj["APIConnectionINFO"]["ApiURL"] = "YeniApiURL";
                    targetJsonObj["APIConnectionINFO"]["ApiEndpoint"] = "YeniApiEndpoint";
                    targetJsonObj["APIConnectionINFO"]["API"] = ApiURL+"/"+ApiEndpoint;

                    // Değişiklikleri kaydet
                    System.IO.File.WriteAllText(windowsFormFileUrl, targetJsonObj.ToString());
                    System.IO.File.SetAttributes(windowsFormFileUrl, FileAttributes.Normal);
                    // View'e geçir
                    ViewBag.TargetJson = targetJsonObj.ToString();
                }
                else
                {
                    ViewBag.TargetJson = "Hedef dosya bulunamadı.";
                }


                TempData["SuccessMessage"] = "API Ayarları güncellendi.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Hata! " + ex.Message;
                return RedirectToAction("Index");
            }
        }


    }
}
