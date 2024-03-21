using Dapper;
using DesignTech_PLM_Entegrasyon_App.MVC.Dtos;
using DesignTech_PLM_Entegrasyon_App.MVC.Models;
using DesignTech_PLM_Entegrasyon_App.MVC.Models.LogTable;
using DesignTech_PLM_Entegrasyon_App.MVC.Models.SignalR;
using DesignTech_PLM_Entegrasyon_App.MVC.Repository;
using DesignTech_PLM_Entegrasyon_App.MVC.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System.Data;
using System.Text;
using System.Web;


namespace DesignTech_PLM_Entegrasyon_App.MVC.Controllers.Modules.WindowsFormSettings
{
    public class WindowsFormSettingsController : Controller
	{

        private readonly IConfiguration _configuration;

       
        private readonly IGenericRepository<Change_Notice_LogTable> _logTableRepository;
        private readonly IWebHostEnvironment _environment;

        //private readonly IMessageProducer _messageProducer;

        public WindowsFormSettingsController(IConfiguration configuration, IGenericRepository<Change_Notice_LogTable> logTableRepository, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _logTableRepository = logTableRepository;
            _environment = environment;
        }



        public async Task<IActionResult> Index()
        {
            try
            {

				var WTPartDataSettings = "WTPartDataSettings.json";
				var json2 = System.IO.File.ReadAllText(WTPartDataSettings);
				var jsonObj2 = string.IsNullOrEmpty(json2) ? new JObject() : JObject.Parse(json2);

				var sablons = jsonObj2["sablons"] as JArray;


				List<SablonViewModel> sablonViewModelList = new List<SablonViewModel>();

				if (sablons != null)
				{
					foreach (var sablon in sablons)
					{
						var sablonName = sablon["sablonName"]?.ToString();
						var ID = sablon["ID"]?.ToString();
						var sablonDataDurumu = sablon["sablonDataDurumu"]?.ToString();
						var sablonData = sablon["sablonData"] as JArray;

						SablonViewModel sablonViewModel = new SablonViewModel
						{
							ID = ID,
							SablonName = sablonName,
							sablonDataDurumu = sablonDataDurumu,
							SablonDataList = new List<WTPartDataSettingsViewModel>()
						};

						if (sablonData != null)
						{
							foreach (var item in sablonData)
							{
								WTPartDataSettingsViewModel viewModel = new WTPartDataSettingsViewModel
								{
									SablonName = sablonName,
									ID = item["ID"]?.ToString(),
									Name = item["Name"]?.ToString(),
									SQLName = item["SQLName"]?.ToString(),
									IsActive = item["IsActive"]?.ToString()
								};

								sablonViewModel.SablonDataList.Add(viewModel);
							}
						}

						sablonViewModelList.Add(sablonViewModel);
					}
				}

				// ViewBag'e sablon verisini atama
				ViewBag.SablonViewModelList = sablonViewModelList;

				return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "HATA! " + ex.Message;
                return View();

            }
        }


        public async Task<IActionResult> ProdMgmt()
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
				var windowsFormFileUrl2 = jsonObj["ApiSendDataSettingsFolder"]?.ToString();


				var targetJson = System.IO.File.ReadAllText(windowsFormFileUrl);
				var targetJson2 = System.IO.File.ReadAllText(windowsFormFileUrl2);
				var targetJsonObj = JObject.Parse(targetJson);
				var targetJsonObj2 = JObject.Parse(targetJson2);


				//string connectionString = _configuration.GetConnectionString("Plm");
				//using IDbConnection connection = new SqlConnection(connectionString);
				string schema = _configuration["Catalog"];


				// Sorguları optimize edin ve sadece gerekli sütunları çekin
				var Change_Notice_LogTableList = (await _logTableRepository.GetAll(schema + ".Change_Notice_LogTable")).OrderByDescending(x => x.ProcessTimestamp).ToList();

				if (Change_Notice_LogTableList is not null)
				{
					ViewBag.Change_Notice_LogTableList = Change_Notice_LogTableList;
					ViewBag.Change_Notice_LogTableListCount = Change_Notice_LogTableList.Count();
				}



				var wtPartMasterList = new List<WTPartMasterItemViewModel>();
				try
				{
					foreach (var item2 in targetJsonObj2.Properties())
					{
					foreach (var item in item2.Value)
						{

						var wtPartMasterItem = new WTPartMasterItemViewModel
						{
							ID = item.Value<string>("ID"),
							Name = item.Value<string>("Name"),
							SQLName = item.Value<string>("SQLName"),
							IsActive = item.Value<bool>("IsActive")
						};

						wtPartMasterList.Add(wtPartMasterItem);
						}
					}
				}
				catch (Exception)
				{

				}


				// Değişiklikleri yap
				ViewBag.Catalog = targetJsonObj["Catalog"].ToString();
				ViewBag.ServerName = targetJsonObj["ServerName"].ToString();
				ViewBag.KullaniciAdi = targetJsonObj["KullaniciAdi"].ToString();
				ViewBag.Parola = targetJsonObj["Parola"].ToString();
				ViewBag.ApiURL = targetJsonObj["APIConnectionINFO"]["ApiURL"].ToString();
				ViewBag.ApiEndpoint = targetJsonObj["APIConnectionINFO"]["ApiEndpoint"].ToString();
				ViewBag.API = targetJsonObj["APIConnectionINFO"]["API"].ToString();

				ViewBag.ApiSendDataSettings = wtPartMasterList;

				// View'e geçir
				ViewBag.TargetJson = targetJsonObj.ToString();
                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Hata!" + ex.Message;
                return View();
            }
        } 
		public async Task<IActionResult> CADDocumentMgmt()
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
				var windowsFormFileUrl2 = jsonObj["ApiSendDataSettingsFolder"]?.ToString();


				var targetJson = System.IO.File.ReadAllText(windowsFormFileUrl);
				var targetJson2 = System.IO.File.ReadAllText(windowsFormFileUrl2);
				var targetJsonObj = JObject.Parse(targetJson);
				var targetJsonObj2 = JObject.Parse(targetJson2);


				//string connectionString = _configuration.GetConnectionString("Plm");
				//using IDbConnection connection = new SqlConnection(connectionString);
				string schema = _configuration["Catalog"];


				// Sorguları optimize edin ve sadece gerekli sütunları çekin
				var Change_Notice_LogTableList = (await _logTableRepository.GetAll(schema + ".Change_Notice_LogTable")).OrderByDescending(x => x.ProcessTimestamp).ToList();

				if (Change_Notice_LogTableList is not null)
				{
					ViewBag.Change_Notice_LogTableList = Change_Notice_LogTableList;
					ViewBag.Change_Notice_LogTableListCount = Change_Notice_LogTableList.Count();
				}



				var wtPartMasterList = new List<WTPartMasterItemViewModel>();
				try
				{
					foreach (var item2 in targetJsonObj2.Properties())
					{
					foreach (var item in item2.Value)
						{

						var wtPartMasterItem = new WTPartMasterItemViewModel
						{
							ID = item.Value<string>("ID"),
							Name = item.Value<string>("Name"),
							SQLName = item.Value<string>("SQLName"),
							IsActive = item.Value<bool>("IsActive")
						};

						wtPartMasterList.Add(wtPartMasterItem);
						}
					}
				}
				catch (Exception)
				{

				}


				// Değişiklikleri yap
				ViewBag.Catalog = targetJsonObj["Catalog"].ToString();
				ViewBag.ServerName = targetJsonObj["ServerName"].ToString();
				ViewBag.KullaniciAdi = targetJsonObj["KullaniciAdi"].ToString();
				ViewBag.Parola = targetJsonObj["Parola"].ToString();
				ViewBag.ApiURL = targetJsonObj["APIConnectionINFO"]["ApiURL"].ToString();
				ViewBag.ApiEndpoint = targetJsonObj["APIConnectionINFO"]["ApiEndpoint"].ToString();
				ViewBag.API = targetJsonObj["APIConnectionINFO"]["API"].ToString();

				ViewBag.ApiSendDataSettings = wtPartMasterList;

				// View'e geçir
				ViewBag.TargetJson = targetJsonObj.ToString();
                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Hata!" + ex.Message;
                return View();
            }
        }






		public IActionResult ApiSendDataPartsSettings(string ID, bool IsActive)
        {

            try
            {

                // appsettings.json dosyasının yolunu al
                var appSettingsPath = "appsettings.json";

                // appsettings.json dosyasını oku
                var json = System.IO.File.ReadAllText(appSettingsPath);
                var jsonObj = JObject.Parse(json);

                // Dosya yolu ve dosya adını al
                var ApiSendDataSettingsFolder = jsonObj["ApiSendDataSettingsFolder"]?.ToString();

                // Hedef json dosyasını oku
                if (!string.IsNullOrEmpty(ApiSendDataSettingsFolder))
                {

                    var targetJson = System.IO.File.ReadAllText(ApiSendDataSettingsFolder);
                    var targetJsonObj = JObject.Parse(targetJson);

                    // Gelen ID ile eşleşen objeyi bul
                    var targetItem = targetJsonObj.SelectToken($"$..[?(@.ID == '{ID}')]");

                    // Eşleşme varsa IsActive değerini güncelle
                    if (targetItem != null)
                    {
                        targetItem["IsActive"] = IsActive;
                    }

                    // Değişiklikleri kaydet
                    System.IO.File.WriteAllText(ApiSendDataSettingsFolder, targetJsonObj.ToString());

                }
                TempData["SuccessMessage"] = "Data parçaları güncellendi.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {

                TempData["ErrorMessage"] = "Hata! " + ex.Message;
                return RedirectToAction("Index");

            }

        }

        //public IActionResult ApiSendDataPartsSettings(string ID,bool IsActive)
        //{
        //    try
        //    {
        //        // appsettings.json dosyasının yolunu al
        //        var appSettingsPath = "appsettings.json";

        //        // appsettings.json dosyasını oku
        //        var json = System.IO.File.ReadAllText(appSettingsPath);
        //        var jsonObj = JObject.Parse(json);

        //        // Dosya yolu ve dosya adını al
        //        var ApiSendDataSettingsFolder = jsonObj["ApiSendDataSettingsFolder"]?.ToString();

        //        // Hedef json dosyasını oku
        //        if (!string.IsNullOrEmpty(ApiSendDataSettingsFolder))
        //        {
        //            var targetJson = System.IO.File.ReadAllText(ApiSendDataSettingsFolder);
        //            var targetJsonObj = JObject.Parse(targetJson);

        //            // Değişiklikleri yap

        //            targetJsonObj["IsActive"] = "IsActive";

        //            // Değişiklikleri kaydet
        //            System.IO.File.WriteAllText(ApiSendDataSettingsFolder, targetJsonObj.ToString());
        //            System.IO.File.SetAttributes(ApiSendDataSettingsFolder, FileAttributes.Normal);
        //            // View'e geçir
        //            ViewBag.TargetJson = targetJsonObj.ToString();
        //        }
        //        else
        //        {
        //            ViewBag.TargetJson = "Hedef dosya bulunamadı.";
        //        }


        //        TempData["SuccessMessage"] = "Data parçaları güncellendi.";
        //        return RedirectToAction("Index");
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["ErrorMessage"] = "Hata! " + ex.Message;
        //        return RedirectToAction("Index");
        //    }
        //}


        public IActionResult ApiSendDataSettings(string jsonFile)
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
                    if (jsonObj["ApiSendDataSettingsFolder"] == null)
                    {
                        jsonObj.Add("ApiSendDataSettingsFolder", jsonFile);
                    }
                    else
                    {
                        jsonObj["ApiSendDataSettingsFolder"] = jsonFile;
                    }

                    // Değişiklikleri kaydet
                    System.IO.File.WriteAllText(appSettingsPath, jsonObj.ToString());

                    // Başarılı mesajı
                    TempData["SuccessMessage"] = "ApiSendDataSettings.json dosyası güncellendi. Dosya Yolu: " + jsonFile;
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

		#region windowsFormURLSettings
		 
		//public IActionResult windowsFormURLSettings(IFormFile jsonFile)
		//{
		//    try
		//    {

		//        if (jsonFile != null && jsonFile.Length > 0)
		//        {
		//            // appsettings.json dosyasını oku
		//            var appSettingsPath = "appsettings.json";
		//            var json = System.IO.File.ReadAllText(appSettingsPath);
		//            var jsonObj = JObject.Parse(json);


		//            // Dosya adını almak için
		//            var fileName = jsonFile.FileName;

		//            // Dosya yolunu belirlemek için (sunucuya kaydetmeden)
		//            var fullFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Configuration", fileName);
		//            //var fullFilePath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", fileName);

		//            // Eğer WindowsFormFileUrl zaten varsa güncelle, yoksa ekle
		//            if (jsonObj["WindowsFormFileUrl"] == null)
		//            {
		//                jsonObj.Add("WindowsFormFileUrl", fullFilePath);
		//            }
		//            else
		//            {
		//                jsonObj["WindowsFormFileUrl"] = fullFilePath;
		//            }



		//            // Değişiklikleri kaydet
		//            System.IO.File.WriteAllText(appSettingsPath.ToString(), jsonObj.ToString());



		//            // Başarılı mesajı
		//            TempData["SuccessMessage"] = "appsettings.json dosyası güncellendi. Dosya Yolu: " + fullFilePath;
		//        }
		//        else
		//        {
		//            // Hata mesajı
		//            TempData["ErrorMessage"] = "Hata! Dosya seçimi yapılmadı.";
		//        }

		//        // Index sayfasına yönlendir
		//        return RedirectToAction("Index");
		//    }
		//    catch (Exception ex)
		//    {
		//        // Hata mesajı
		//        TempData["ErrorMessage"] = "Hata! " + ex.Message;

		//        // Index sayfasına yönlendir
		//        return RedirectToAction("Index");
		//    }
		//}



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


		#endregion



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

                    // RabbitMQ suncuda olmadığı için bunu kullanamıyoruz yinede dursun ilerde docker falan derken aktif ederiz
                    //CloudAMQP üzerinden rabbitmq ile yaptık birşeyler ama buda uzak sunucuda ve paralı
                    // _messageProducer.SendingMessage(ConnectionStrings);

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



		public IActionResult apiSablonsSendSettings(IFormCollection jsonData)
		{
			try
			{
				var WTPartDataSettings = "WTPartDataSettings.json";
				var json = System.IO.File.ReadAllText(WTPartDataSettings);
				var jsonObj = string.IsNullOrEmpty(json) ? new JObject() : JObject.Parse(json);

				var sablonName = jsonData.FirstOrDefault().Value.ToString();
                var sourceApi = jsonData["source_Api"].ToString();
                var yapilacakIslem = jsonData["yapilacakIslem"].ToString();
                var sablonDataDurumu = "false";

                // Şablon adına sahip olanları kontrol et
                var existingSablon = jsonObj["sablons"]?
					.FirstOrDefault(s => s["sablonName"]?.ToString() == sablonName || (s["Source_Api"]?.ToString() == sourceApi && s["yapilacakIslem"]?.ToString() == yapilacakIslem));

				if (existingSablon != null)
				{
					// Eğer şablon varsa, mevcut şablonu güncelle
					existingSablon["ID"] = Guid.NewGuid().ToString();
					existingSablon["Source_Api"] = sourceApi;
					existingSablon["State"] = yapilacakIslem;
                    existingSablon["sablonDataDurumu"] = sablonDataDurumu;
                    existingSablon["sablonData"] = JArray.FromObject(jsonData.Where(x => x.Key != jsonData.FirstOrDefault().Key && x.Key != "__RequestVerificationToken").Select(item => new
					{
						ID = Guid.NewGuid().ToString(),
						Name = item.Key,
						SQLName = item.Key,
						IsActive = item.Value.ToString()
					}).ToList());

					TempData["SuccessMessage"] = "Şablon güncelleştirildi";
				}
				else
				{
					// Şablon yoksa, yeni bir şablon oluştur
					var newTemplate = new
					{
						ID = Guid.NewGuid().ToString(),
                        Source_Api = sourceApi,
                        State = yapilacakIslem,
                        sablonDataDurumu = sablonDataDurumu,
                        sablonName = sablonName,
						sablonData = jsonData.Where(x => x.Key != jsonData.FirstOrDefault().Key && x.Key != "__RequestVerificationToken").Select(item => new
						{
							ID = Guid.NewGuid().ToString(),
							Name = item.Key,
							SQLName = item.Key,
							IsActive = item.Value.ToString()
						}).ToList()
					};

					// Eski veriye yeni veriyi ekle
					if (jsonObj["sablons"] == null)
					{
						jsonObj["sablons"] = new JArray();
					}

					(jsonObj["sablons"] as JArray).Add(JObject.FromObject(newTemplate));
                    TempData["SuccessMessage"] = "Şablon oluşturuldu";
				}

				// Dosyaya yaz
				System.IO.File.WriteAllText(WTPartDataSettings, JsonConvert.SerializeObject(jsonObj, Formatting.Indented));

				return RedirectToAction("Index");
			}
			catch (Exception)
			{
				return RedirectToAction("Index");
			}
		}
        public IActionResult apiSablonsRemoveSettings(string ID)
        {
            try
            {
                var WTPartDataSettings = "WTPartDataSettings.json";
                var json = System.IO.File.ReadAllText(WTPartDataSettings);
                var jsonObj = string.IsNullOrEmpty(json) ? new JObject() : JObject.Parse(json);

                // Şablonları içeren "sablons" dizisini kontrol et
                var sablonsArray = jsonObj["sablons"] as JArray;

                if (sablonsArray != null)
                {
                    // Eşleşen ID'yi içeren şablonu bul
                    var existingSablon = sablonsArray.FirstOrDefault(s => s["ID"]?.ToString() == ID);

                    if (existingSablon != null)
                    {
                        // Eğer şablon varsa, onu sil
                        existingSablon.Remove();
                        System.IO.File.WriteAllText(WTPartDataSettings, JsonConvert.SerializeObject(jsonObj, Formatting.Indented));

                        TempData["SuccessMessage"] = "Silme işlemi başarılı.";
                        return RedirectToAction("Index");
                    }
                }

                // Eğer eşleşen ID bulunamazsa veya hata oluşursa
                TempData["ErrorMessage"] = "ID ile eşleşen şablon bulunamadı veya silme işlemi başarısız.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Hata! :" + ex.Message;
                return RedirectToAction("Index");
            }
        }



        public IActionResult apiSablonsSendtToWindowsFormSettings(string ID, string sablonDataDurumu, string SablonName)
        {
            try
            {
                // Read the JSON file
                var WTPartDataSettings = "WTPartDataSettings.json";
                var json = System.IO.File.ReadAllText(WTPartDataSettings);
                var jsonObj = string.IsNullOrEmpty(json) ? new JObject() : JObject.Parse(json);

                var state = "";
                var source_Api = "";
                var sablons = jsonObj["sablons"] as JArray;

                if (sablons != null)
                {
                    // Iterate through sablons to find the one with the given ID
                    foreach (var sablon in sablons)
                    {
                        var sablonID = sablon["ID"]?.ToString();

                        if (sablonID == ID)
                        {
                            // Update sablonDataDurumu based on the provided value
                            sablon["sablonDataDurumu"] = sablonDataDurumu;
                            source_Api = sablon["Source_Api"]?.ToString();
                            state = sablon["State"]?.ToString();

                            //// If setting it to true, set other sablonDataDurumu values to false
                            //if (sablonDataDurumu.ToLower() == "true")
                            //{
                            //    foreach (var otherSablon in sablons)
                            //    {
                            //        if (otherSablon != sablon)
                            //        {
                            //            otherSablon["sablonDataDurumu"] = "false";
                            //        }
                            //    }
                            //}

                            // Save the changes back to the JSON file
                            System.IO.File.WriteAllText(WTPartDataSettings, jsonObj.ToString());
                            // Get sablonData values
                            var sablonData = sablon["sablonData"] as JArray;

							// Call ApiSendDataPartsSettings to update IsActive
							ApiSendDataPartsSettings(ID, sablonData, source_Api, state, SablonName, sablonDataDurumu);

							TempData["SuccessMessage"] = SablonName + " adlı şablon aktif edildi.";
                            break; // Exit the loop once the update is done
                        }
                    }
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Hata! :" + ex.Message;
                return RedirectToAction("Index");
            }
        }
         


        public IActionResult ApiSendDataPartsSettings(string ID, JArray sablonData,string source_Api,string state,string sablonName, string sablonDataDurumu)
        {

			try
			{
				// appsettings.json dosyasının yolunu al
				var appSettingsPath = "appsettings.json";

				// appsettings.json dosyasını oku
				var json = System.IO.File.ReadAllText(appSettingsPath);
				var jsonObj = JObject.Parse(json);

				// Dosya yolu ve dosya adını al
				var ApiSendDataSettingsFolder = jsonObj["ApiSendDataSettingsFolder"]?.ToString();

				// Hedef json dosyasını oku
				if (!string.IsNullOrEmpty(ApiSendDataSettingsFolder))
				{
					var targetJson = System.IO.File.ReadAllText(ApiSendDataSettingsFolder);
					var targetJsonObj = JObject.Parse(targetJson);

					// Eşleşen ID'yi bul
					var targetItem = targetJsonObj["WTPartMaster"]?.FirstOrDefault(t => t["ID"]?.ToString() == ID);

					if (targetItem != null)
					{
						// Eğer eşleşen öğe varsa, sablonData ile değiştir
						var wtPartMasterArray = targetItem[sablonName] as JArray;

						targetItem["source_Api"] = source_Api; // Eklenen kısım
						targetItem["state"] = state; // Eklenen kısım
						targetItem["sablonDataDurumu"] = sablonDataDurumu; // Eklenen kısım

						if (wtPartMasterArray != null)
						{
							if (sablonDataDurumu.ToLower() == "true")
							{
								var filteredSablonData = sablonData
									.Where(item =>
										item["IsActive"]?.ToString() != state &&
										item["IsActive"]?.ToString() != source_Api
									).ToList();

								wtPartMasterArray.ReplaceAll(filteredSablonData);
								wtPartMasterArray.Clear();
							}
							else if (sablonDataDurumu.ToLower() == "false")
							{
								// sablonDataDurumu false ise, WTPartMaster dizisini temizle
								wtPartMasterArray.Clear();
							}
						}
					}
					else
					{
						var filteredSablonData = sablonData
							.Where(item =>
								item["IsActive"]?.ToString() != state &&
								item["IsActive"]?.ToString() != source_Api
							).ToList();

						// Eşleşen öğe yoksa, WTPartMaster dizisini komple değiştir
						var newTargetItem = new JObject();
						newTargetItem["source_Api"] = source_Api; // Eklenen kısım
						newTargetItem["state"] = state; // Eklenen kısım
						newTargetItem["sablonDataDurumu"] = sablonDataDurumu; // Eklenen kısım

						if (sablonDataDurumu.ToLower() == "true")
						{
							//newTargetItem["WTPartMaster"] = JArray.FromObject(filteredSablonData);
						}
						else if (sablonDataDurumu.ToLower() == "false")
						{
							// sablonDataDurumu false ise, WTPartMaster dizisini temizle
							newTargetItem["WTPartMaster"] = new JArray();
						}

						var wtPartMasterArray = new JArray();
						wtPartMasterArray.Add(newTargetItem);

						targetJsonObj[sablonName] = wtPartMasterArray;
					}

					// Değişiklikleri kaydet
					System.IO.File.WriteAllText(ApiSendDataSettingsFolder, targetJsonObj.ToString());
				}

				TempData["SuccessMessage"] = "Data parçaları güncellendi.";
				return RedirectToAction("Index");
			}
			catch (Exception ex)
			{
				TempData["ErrorMessage"] = $"Hata! {ex.Message}";
				return RedirectToAction("Index");
			}


			//try
			//{
			//	// appsettings.json dosyasının yolunu al
			//	var appSettingsPath = "appsettings.json";

			//	// appsettings.json dosyasını oku
			//	var json = System.IO.File.ReadAllText(appSettingsPath);
			//	var jsonObj = JObject.Parse(json);

			//	// Dosya yolu ve dosya adını al
			//	var ApiSendDataSettingsFolder = jsonObj["ApiSendDataSettingsFolder"]?.ToString();

			//	// Hedef json dosyasını oku
			//	if (!string.IsNullOrEmpty(ApiSendDataSettingsFolder))
			//	{
			//		var targetJson = System.IO.File.ReadAllText(ApiSendDataSettingsFolder);
			//		var targetJsonObj = JObject.Parse(targetJson);

			//		// Eşleşen ID'yi bul
			//		var targetItem = targetJsonObj["WTPartMaster"]?.FirstOrDefault(t => t["ID"]?.ToString() == ID);

			//		if (targetItem != null)
			//		{
			//			// Eğer eşleşen öğe varsa, sablonData ile değiştir
			//			var wtPartMasterArray = targetItem[sablonName] as JArray;

			//			targetItem["source_Api"] = source_Api; // Eklenen kısım
			//			targetItem["state"] = state; // Eklenen kısım
			//			targetItem["sablonDataDurumu"] = sablonDataDurumu; // Eklenen kısım

			//			if (wtPartMasterArray != null)
			//			{
			//				var filteredSablonData = sablonData
			//					.Where(item =>
			//						item["IsActive"]?.ToString() != state &&
			//						item["IsActive"]?.ToString() != source_Api
			//					).ToList();

			//				wtPartMasterArray.ReplaceAll(filteredSablonData);
			//			}
			//		}
			//		else
			//		{
			//			var filteredSablonData = sablonData
			//				.Where(item =>
			//					item["IsActive"]?.ToString() != state &&
			//					item["IsActive"]?.ToString() != source_Api
			//				).ToList();

			//			// Eşleşen öğe yoksa, WTPartMaster dizisini komple değiştir
			//			var newTargetItem = new JObject();
			//			newTargetItem["source_Api"] = source_Api; // Eklenen kısım
			//			newTargetItem["state"] = state; // Eklenen kısım
			//			targetItem["sablonDataDurumu"] = sablonDataDurumu; // Eklenen kısım
			//			newTargetItem["WTPartMaster"] = JArray.FromObject(filteredSablonData);

			//			var wtPartMasterArray = new JArray();
			//			wtPartMasterArray.Add(newTargetItem);

			//			targetJsonObj[sablonName] = wtPartMasterArray;
			//		}

			//		// Değişiklikleri kaydet
			//		System.IO.File.WriteAllText(ApiSendDataSettingsFolder, targetJsonObj.ToString());
			//	}

			//	TempData["SuccessMessage"] = "Data parçaları güncellendi.";
			//	return RedirectToAction("Index");
			//}
			//catch (Exception ex)
			//{
			//	TempData["ErrorMessage"] = $"Hata! {ex.Message}";
			//	return RedirectToAction("Index");
			//}


			//try
			//{
			//    // appsettings.json dosyasının yolunu al
			//    var appSettingsPath = "appsettings.json";

			//    // appsettings.json dosyasını oku
			//    var json = System.IO.File.ReadAllText(appSettingsPath);
			//    var jsonObj = JObject.Parse(json);

			//    // Dosya yolu ve dosya adını al
			//    var ApiSendDataSettingsFolder = jsonObj["ApiSendDataSettingsFolder"]?.ToString();

			//    // Hedef json dosyasını oku
			//    if (!string.IsNullOrEmpty(ApiSendDataSettingsFolder))
			//    {
			//        var targetJson = System.IO.File.ReadAllText(ApiSendDataSettingsFolder);
			//        var targetJsonObj = JObject.Parse(targetJson);

			//        // Eşleşen ID'yi bul
			//        var targetItem = targetJsonObj["WTPartMaster"]?.FirstOrDefault(t => t["ID"]?.ToString() == ID);

			//        if (targetItem != null)
			//        {
			//            // Eğer eşleşen öğe varsa, sablonData ile değiştir

			//            targetItem["WTPartMaster"] = sablonData;
			//        }
			//        else
			//        {
			//            // Eşleşen öğe yoksa, WTPartMaster dizisini komple değiştir
			//            targetJsonObj["WTPartMaster"] = sablonData;
			//        }

			//        // Değişiklikleri kaydet
			//        System.IO.File.WriteAllText(ApiSendDataSettingsFolder, targetJsonObj.ToString());
			//    }

			//    TempData["SuccessMessage"] = "Data parçaları güncellendi.";
			//    return RedirectToAction("Index");
			//}
			//catch (Exception ex)
			//{
			//    TempData["ErrorMessage"] = $"Hata! {ex.Message}";
			//    return RedirectToAction("Index");
			//}
		}



		//public IActionResult ApiSendDataPartsSettings(string ID, JArray sablonData)
		//{
		//    try
		//    {
		//        // appsettings.json dosyasının yolunu al
		//        var appSettingsPath = "appsettings.json";

		//        // appsettings.json dosyasını oku
		//        var json = System.IO.File.ReadAllText(appSettingsPath);
		//        var jsonObj = JObject.Parse(json);

		//        // Dosya yolu ve dosya adını al
		//        var ApiSendDataSettingsFolder = jsonObj["ApiSendDataSettingsFolder"]?.ToString();

		//        // Hedef json dosyasını oku
		//        if (!string.IsNullOrEmpty(ApiSendDataSettingsFolder))
		//        {
		//            var targetJson = System.IO.File.ReadAllText(ApiSendDataSettingsFolder);
		//            var targetJsonObj = JObject.Parse(targetJson);

		//            // Eşleşen ID'yi bul
		//            var targetItem = targetJsonObj["WTPartMaster"]?.FirstOrDefault(t => t["ID"]?.ToString() == ID);

		//            if (targetItem != null)
		//            {
		//                // Eğer eşleşen öğe varsa, sablonData'yı güncelle
		//                targetItem["WTPartMaster"] = sablonData;
		//            }
		//            else
		//            {
		//                // Eşleşen öğe yoksa yeni bir öğe oluştur ve sablonData'yı ekle
		//                var newTargetItem = new JObject();
		//                //newTargetItem["ID"] = ID;
		//                //newTargetItem["WTPartMaster"] = sablonData;

		//                // Önceki kayıtları temizle ve yeni öğeyi ekle
		//                //targetJsonObj["WTPartMaster"] = new JArray(sablonData);

		//                if (targetJsonObj["WTPartMaster"] == null)
		//                {
		//                    targetJsonObj["WTPartMaster"] = new JArray(sablonData);
		//                }
		//                else
		//                {
		//                    // WTPartMaster dizisi varsa sadece yeni öğeyi ekle
		//                    ((JArray)targetJsonObj["WTPartMaster"]).Add(sablonData);
		//                }
		//            }

		//            // Değişiklikleri kaydet
		//            System.IO.File.WriteAllText(ApiSendDataSettingsFolder, targetJsonObj.ToString());
		//        }

		//        TempData["SuccessMessage"] = "Data parçaları güncellendi.";
		//        return RedirectToAction("Index");
		//    }
		//    catch (Exception ex)
		//    {
		//        TempData["ErrorMessage"] = $"Hata! {ex.Message}";
		//        return RedirectToAction("Index");
		//    }
		//}





		//    public IActionResult apiSablonsSendSettings(IFormCollection jsonData)
		//    {
		//        try
		//        {
		//var WTPartDataSettings = "WTPartDataSettings.json";
		//var json = System.IO.File.ReadAllText(WTPartDataSettings);
		//var jsonObj = JObject.Parse(json);




		//var newTemplate = new
		//{
		//	ID = Guid.NewGuid().ToString(),
		//	sablonName = jsonData.FirstOrDefault().Value.ToString(),
		//	sablonData = jsonData.Where(x => x.Key != jsonData.FirstOrDefault().Key).Select(item => new
		//	{
		//		ID = Guid.NewGuid().ToString(),
		//		Name = item.Key,
		//		SQLName = item.Key,
		//		IsActive = item.Value.ToString()
		//	}).ToList()
		//};

		//// Eski veriye yeni veriyi ekle
		//var sablons = jsonObj["sablons"] as JArray;
		//sablons.Add(JObject.FromObject(newTemplate));

		//// Dosyaya yaz
		//System.IO.File.WriteAllText(WTPartDataSettings, JsonConvert.SerializeObject(jsonObj, Formatting.Indented));


		//return RedirectToAction("Index");
		//        }
		//        catch (Exception)
		//        {
		//        return RedirectToAction("Index");
		//        }

		//    }


	}
}
