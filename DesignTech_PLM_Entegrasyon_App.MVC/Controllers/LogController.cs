using ExcelDataReader;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Abstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Text.Json.Nodes;
using System.Web;

namespace DesignTech_PLM_Entegrasyon_App.MVC.Controllers
{
    [Authorize]
    public class LogController : Controller
	{
        //public IActionResult Index()
        //{
        //          try
        //          {
        //              string logsPath = "wwwroot\\Logs"; // Logs klasörünüzün yolu
        //              string[] dateFolders = Directory.GetDirectories(logsPath);

        //              // Tarih klasörlerini ViewBag'e aktarın
        //              ViewBag.DateFolders = dateFolders;
        //              return View();


        //          }
        //          catch (Exception)
        //          {
        //              return View();

        //          }


        //      }

        public IActionResult Index()
        {
            try
            {
                string logsPath = "wwwroot\\Logs"; // Logs klasörünüzün yolu
                if (!Directory.Exists(logsPath))
                {
                    // Logs klasörü yoksa oluşturun
                    Directory.CreateDirectory(logsPath);
                }

                string[] dateFolders = Directory.GetDirectories(logsPath);

                // Tarih klasörlerini ViewBag'e aktarın
                ViewBag.DateFolders = dateFolders;
                return View();
            }
            catch (Exception)
            {
                return View();
            }
        }

		public IActionResult Log2()
		{
			try
			{
				string logsPath = "wwwroot\\Logs"; // Logs klasörünüzün yolu
				if (!Directory.Exists(logsPath))
				{
					// Logs klasörü yoksa oluşturun
					Directory.CreateDirectory(logsPath);
				}

				string[] dateFolders = Directory.GetDirectories(logsPath);

				// Filter folders containing "ProcessLog"
				var filteredFolders = dateFolders.Where(folder => folder.Contains("ProcessLog")).ToArray();

				// Tarih klasörlerini ViewBag'e aktarın
				ViewBag.DateFolders = filteredFolders;
				return View();
			}
			catch (Exception)
			{
				return View();
			}
		}
		
        public ActionResult ViewJsonFiles(string dateFolder)
        {

            try {

                dateFolder = "TakvimFile";


				string logsPath = "wwwroot\\Logs"; // Logs klasörünüzün yolu
                string dateFolderPath = Path.Combine(logsPath, dateFolder);
                string[] jsonFiles = Directory.GetFiles(dateFolderPath, "*.json");

                // JSON dosyalarını ViewBag'e aktarın
                ViewBag.DateFolder = dateFolderPath;
                ViewBag.JsonFiles = jsonFiles;
                return View(); } catch (Exception) { return View(); }

         

           
        }


		public ActionResult ViewJsonFiles2(string dateFolder)
		{

			try
			{

				string logsPath = "wwwroot\\Logs"; // Logs klasörünüzün yolu
				string dateFolderPath = Path.Combine(logsPath, "ProcessLog");
				string[] jsonFiles = Directory.GetFiles(dateFolderPath, "*.json");

				// JSON dosyalarını ViewBag'e aktarın
				ViewBag.DateFolder = dateFolderPath;
				ViewBag.JsonFiles = jsonFiles;
				return View();
			}
			catch (Exception) { return View(); }




		}
     

        //public ActionResult ViewLogFile(string dateFolder, string jsonFile)
        //{
        //    string logsPath = "wwwroot\\Logs"; // Logs klasörünüzün yolu
        //    string dateFolderPath = Path.Combine(logsPath, dateFolder);
        //    string jsonFilePath = Path.Combine(dateFolderPath, jsonFile);

        //    string jsonContent = System.IO.File.ReadAllText(jsonFilePath);

        //    // JSON içeriği ViewBag'e aktarın
        //    ViewBag.JsonContent = jsonContent;

        //    return View();
        //}


        public class LogEntry
        {
            public string Timestamp { get; set; }
            public LogMessage Message { get; set; }
            public Dictionary<string, string> Properties { get; set; }
        }



		public class ExcelLogEntry
        {
            public string ExcelDosya { get; set; } = string.Empty;
            public string Hata { get; set; } = string.Empty;
			public string Text { get; set; } = string.Empty;
            public string Operation { get; set; } = string.Empty;
            public int Satir { get; set; }
            public int Sutun { get; set; }
            public bool Durum { get; set; }
            public string KullaniciAdi { get; set; }
            public string islemTarihi { get; set; }

            public ExcelLogEntry(string hata)
            {
                Hata = hata;
            }
        }



        public class LogMessage
        {
            public string ExcelDosya { get; set; } = string.Empty;
            public string Text { get; set; } = string.Empty;
            public string Operation { get; set; } = string.Empty;
            public string kullaniciAdi { get; set; } = string.Empty;
            public string Hata { get; set; } = string.Empty;
            public int Satir { get; set; }
            public int Sutun { get; set; }
            public string islemTarihi { get; set; } = string.Empty;
            public bool Durum { get; set; }
        }

        public class LogData
        {
            public string ExcelDosya { get; set; } = string.Empty;
			public string Text { get; set; } = string.Empty;
			public string Operation { get; set; } = string.Empty;
            public string Hata { get; set; } = string.Empty;
            public int Satir { get; set; }
            public int Sutun { get; set; }
            public string islemTarihi { get; set; } = string.Empty;
            public bool Durum { get; set; }
        }


        public ActionResult ViewLogFile(string dateFolder, string jsonFile)
        {
            try
            {
                if (string.IsNullOrEmpty(dateFolder) || string.IsNullOrEmpty(jsonFile))
                {
                    TempData["ErrorMessage"] = "Hata oluştu";
                    return RedirectToAction("Index");
                }

                string logsPath = "wwwroot\\Logs";
                string dateFolderPath = Path.Combine(dateFolder);
                string jsonFilePath = Path.Combine(dateFolderPath, jsonFile);

                if (!System.IO.File.Exists(jsonFilePath))
                {
                    TempData["ErrorMessage"] = "Belirtilen JSON dosyası bulunamadı.";
                    return RedirectToAction("Index");
                }

                var excelLogEntries = new List<ExcelLogEntry>();

                // Dosyayı okuma işlemi için try-catch kullanarak hataları ele alın
                // Dosyayı okuma işlemi için try-catch kullanarak hataları ele alın
                try
                {
                    using (var fileStream = System.IO.File.Open(jsonFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    using (var streamReader = new StreamReader(fileStream))
                    {
                        string jsonContent = streamReader.ReadToEnd();



                        JArray jsonArray = JArray.Parse("[" + jsonContent + "]");
                        foreach (var jsonEntry in jsonArray.Children())
                        {
                            try
                            {
                                var logEntry = jsonEntry.ToObject<LogEntry>();

                                // LogEntry sınıfını ExcelLogEntry sınıfına dönüştür
                                var excelLogEntry = new ExcelLogEntry(logEntry.Message.Hata)
                                {
                                    ExcelDosya = logEntry.Message.ExcelDosya,
                                    Text = logEntry.Message.Text,
                                    Operation = logEntry.Message.Operation,
                                    Hata = logEntry.Message.Hata,
                                    Satir = logEntry.Message.Satir,
                                    Sutun = logEntry.Message.Sutun,
                                    islemTarihi = logEntry.Timestamp,
                                    KullaniciAdi = logEntry.Message.kullaniciAdi,
                                    Durum = logEntry.Message.Durum
                                };

                                excelLogEntries.Add(excelLogEntry);
                            }
                            catch (JsonReaderException ex)
                            {
                                TempData["ErrorMessage"] = "UYARI ! " + ex.Message;
                                return RedirectToAction("Index");
                            }
                            finally
                            {
                                streamReader.Close();
                                fileStream.Close();
                            }
                        }

                    }
                }
                catch (System.IO.IOException ex)
                {
                    // Dosya erişimi hatası oluştuğunda yapılacak işlemler
                }


                // Verileri tazelemek için aynı sayfayı tekrar yükle
                ViewBag.jsonFilePath = jsonFilePath;
                ViewBag.excelLogEntries = excelLogEntries;
                return View(excelLogEntries);
            }
            catch (Exception)
            {
                return View();
            }
         
        }

		public ActionResult ViewLogFile2(string dateFolder, string jsonFile)
		{
			try
			{
				//if (string.IsNullOrEmpty(dateFolder) || string.IsNullOrEmpty(jsonFile))
				//{
				//	TempData["ErrorMessage"] = "Hata oluştu";
				//	return RedirectToAction("Index");
				//}

				string logsPath = "wwwroot\\Logs2\\ProcessLog";
				//string dateFolderPath = Path.Combine(dateFolder);
				string jsonFilePath = Path.Combine(logsPath, "standartLogFile.json");

				if (!System.IO.File.Exists(jsonFilePath))
				{
					TempData["ErrorMessage"] = "Belirtilen JSON dosyası bulunamadı.";
					return RedirectToAction("Index");
				}

				var excelLogEntries = new List<ExcelLogEntry>();

				// Dosyayı okuma işlemi için try-catch kullanarak hataları ele alın
				// Dosyayı okuma işlemi için try-catch kullanarak hataları ele alın
				try
				{
					using (var fileStream = System.IO.File.Open(jsonFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
					using (var streamReader = new StreamReader(fileStream))
					{
						string jsonContent = streamReader.ReadToEnd();

                 

						JArray jsonArray = JArray.Parse("[" + jsonContent + "]");
						foreach (var jsonEntry in jsonArray.Children())
						{
							try
							{
								var logEntry = jsonEntry.ToObject<LogEntry>();

								// LogEntry sınıfını ExcelLogEntry sınıfına dönüştür
								var excelLogEntry = new ExcelLogEntry(logEntry.Message.Hata)
								{
									ExcelDosya = logEntry.Message.ExcelDosya,
									Text = logEntry.Message.Text,
									Operation = logEntry.Message.Operation,
									Hata = logEntry.Message.Hata,
									Satir = logEntry.Message.Satir,
									Sutun = logEntry.Message.Sutun,
									islemTarihi = logEntry.Timestamp,
									Durum = logEntry.Message.Durum
								};

								excelLogEntries.Add(excelLogEntry);
							}
							catch (JsonReaderException ex)
							{
								TempData["ErrorMessage"] = "UYARI ! " + ex.Message;
								return RedirectToAction("Index");
							}
							finally
							{
								streamReader.Close();
								fileStream.Close();
							}
						}

					}
				}
				catch (System.IO.IOException ex)
				{
					// Dosya erişimi hatası oluştuğunda yapılacak işlemler
				}


				// Verileri tazelemek için aynı sayfayı tekrar yükle
				ViewBag.jsonFilePath = jsonFilePath;
				ViewBag.excelLogEntries = excelLogEntries;
				return View(excelLogEntries);
			}
			catch (Exception)
			{
				return View();
			}

		}




        //LOG3  
        public IActionResult Log3()
        {
            try
            {

                var appSettingsPath = "appsettings.json";

                // appsettings.json dosyasını oku
                var json = System.IO.File.ReadAllText(appSettingsPath);
                var jsonObj = JObject.Parse(json);

                // Dosya yolu ve dosya adını al
                var windowsFormFileUrl = jsonObj["LogFileFolderPathSettings"]?.ToString();

                //string logsPath = "C:\\Users\\tronu\\source\\repos\\designtechkamil\\Designtech_PLM_Entegrasyon_AutoPost_V2\\Designtech_PLM_Entegrasyon_AutoPost_V2\\bin\\Debug\\net8.0-windows\\Configuration\\logs"; // Logs klasörünüzün yolu
                //if (!Directory.Exists(logsPath))
                //{
                //    // Logs klasörü yoksa oluşturun
                //    Directory.CreateDirectory(logsPath);
                //}

                //string[] dateFolders = Directory.GetDirectories(logsPath);

                //// Filter folders containing "ProcessLog"
                //var filteredFolders = dateFolders.Where(folder => folder.Contains("TakvimFile")).ToArray();

                // Tarih klasörlerini ViewBag'e aktarın
                ViewBag.DateFolders = windowsFormFileUrl;
                return View();
            }
            catch (Exception)
            {
                return View();
            }
        }

        public ActionResult ViewJsonFiles3(string dateFolder)
        {

            try
            {
                var appSettingsPath = "appsettings.json";

                // appsettings.json dosyasını oku
                var json = System.IO.File.ReadAllText(appSettingsPath);
                var jsonObj = JObject.Parse(json);

                // Dosya yolu ve dosya adını al
                var windowsFormFileUrl = jsonObj["LogFileFolderPathSettings"]?.ToString();

                //dateFolder = "TakvimFile";


                //string logsPath = "C:\\Users\\tronu\\source\\repos\\designtechkamil\\Designtech_PLM_Entegrasyon_AutoPost_V2\\Designtech_PLM_Entegrasyon_AutoPost_V2\\bin\\Debug\\net8.0-windows\\Configuration\\logs"; // Logs klasörünüzün yolu
                //string dateFolderPath = Path.Combine(logsPath, dateFolder);
                string[] jsonFiles = Directory.GetFiles(windowsFormFileUrl, "*.json");

                // JSON dosyalarını ViewBag'e aktarın
                ViewBag.DateFolder = windowsFormFileUrl;
                ViewBag.JsonFiles = jsonFiles;
                return View();
            }
            catch (Exception) { return View(); }




        }


  

   


        public class TakvimJsonoLogClass
        {
            public string TransferID { get; set; } = string.Empty;
            public string ID { get; set; } = string.Empty;
            public string Number { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string Mesaj { get; set; } = string.Empty;
            public string Version { get; set; } = string.Empty;
            public string? VersionID { get; set; }
            public string? islemTarihi { get; set; }

        }
        public class TakvimJsonoLogClass2
        {
            public string TransferID { get; set; } = string.Empty;
            public string ID { get; set; } = string.Empty;
            public string Number { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string Mesaj { get; set; } = string.Empty;
            public string Version { get; set; } = string.Empty;
            public string? VersionID { get; set; }
            public string? islemTarihi { get; set; }
        }


        public ActionResult ViewLogFile3(string dateFolder, string jsonFile)
        {
            try
            {

                var appSettingsPath = "appsettings.json";

                // appsettings.json dosyasını oku
                var json = System.IO.File.ReadAllText(appSettingsPath);
                var jsonObj = JObject.Parse(json);

                // Dosya yolu ve dosya adını al
                var windowsFormFileUrl = jsonObj["LogFileFolderPathSettings"]?.ToString();



                if (string.IsNullOrEmpty(dateFolder) || string.IsNullOrEmpty(jsonFile))
                {
                    TempData["ErrorMessage"] = "Hata oluştu";
                    return RedirectToAction("Index");
                }

                //string logsPath = "C:\\Users\\tronu\\source\\repos\\designtechkamil\\Designtech_PLM_Entegrasyon_AutoPost_V2\\Designtech_PLM_Entegrasyon_AutoPost_V2\\bin\\Debug\\net8.0-windows\\Configuration\\logs";
                //string dateFolderPath = Path.Combine(dateFolder);
                string jsonFilePath = Path.Combine(windowsFormFileUrl, jsonFile);

                if (!System.IO.File.Exists(jsonFilePath))
                {
                    TempData["ErrorMessage"] = "Belirtilen JSON dosyası bulunamadı.";
                    return RedirectToAction("Index");
                }

                var excelLogEntries = new List<TakvimJsonoLogClass2>();

                // Dosyayı okuma işlemi için try-catch kullanarak hataları ele alın
                // Dosyayı okuma işlemi için try-catch kullanarak hataları ele alın
                try
                {
                    using (var fileStream = System.IO.File.Open(jsonFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    using (var streamReader = new StreamReader(fileStream))
                    {
                        string jsonContent = streamReader.ReadToEnd();


                        JArray jsonArray = JArray.Parse(jsonContent);
                        foreach (var jsonEntry in jsonArray.Children())
                        {
                            try
                            {
                                var logEntry = jsonEntry.ToObject<TakvimJsonoLogClass>();

                                // LogEntry sınıfını ExcelLogEntry sınıfına dönüştür
                                var excelLogEntry = new TakvimJsonoLogClass2
                                {
                                       TransferID = logEntry.TransferID,
                                       ID = logEntry.ID,
                                       Number = logEntry.Number,
                                       Name = logEntry.Name,
                                    Mesaj = logEntry.Mesaj,
                                       Version = logEntry.Version,
                                       VersionID = logEntry.VersionID,
                                       islemTarihi = logEntry.islemTarihi,
                                };

                                excelLogEntries.Add(excelLogEntry);
                            }
                            catch (JsonReaderException ex)
                            {
                                TempData["ErrorMessage"] = "UYARI ! " + ex.Message;
                                return RedirectToAction("Index");
                            }
                            finally
                            {
                                streamReader.Close();
                                fileStream.Close();
                            }
                        }

                    }
                }
                catch (System.IO.IOException ex)
                {
                    // Dosya erişimi hatası oluştuğunda yapılacak işlemler
                }


                // Verileri tazelemek için aynı sayfayı tekrar yükle
                ViewBag.jsonFilePath = jsonFilePath;
                ViewBag.excelLogEntries = excelLogEntries;
                return View(excelLogEntries);
            }
            catch (Exception)
            {
                return View();
            }

        }

        //LOG3  




        public ActionResult UpdateLogControlJson(LogUpdateRequest logUpdateRequest)
        {
            try
            {
                // JSON dosyasının yolu
                string jsonFilePath = Path.Combine(logUpdateRequest.logFileName);

                if (!System.IO.File.Exists(jsonFilePath))
                {
                    TempData["ErrorMessage"] = "Belirtilen JSON dosyası bulunamadı.";
                    return RedirectToAction("Index");
                }

                // JSON içeriğini okuma ve güncelleme
                try
                {
                    string jsonContent = System.IO.File.ReadAllText(jsonFilePath);

                    // JSON içeriğini doğrudan List<LogEntry> olarak okuyun
                    var jArray = JArray.Parse("[" + jsonContent + "]");
                    var logEntries = jArray.ToObject<List<LogEntry>>();


                    var entryToModify = logEntries.FirstOrDefault(entry =>
                        entry.Timestamp == logUpdateRequest.islemTarihi &&
                        entry.Message.ExcelDosya == logUpdateRequest.ExcelDosya &&
                        entry.Message.Hata == logUpdateRequest.Hata &&
                        entry.Message.Satir == logUpdateRequest.Satir &&
                        entry.Message.Sutun == logUpdateRequest.Sutun);

                    if (entryToModify != null)
                    {
                        entryToModify.Message.Durum = logUpdateRequest.Durum;

                        // JSON dosyasına geri yazın
                        string updatedJsonContent = JsonConvert.SerializeObject(logEntries);
                        updatedJsonContent = updatedJsonContent.TrimStart('[').TrimEnd(']');
                        updatedJsonContent += ",";
                        System.IO.File.WriteAllText(jsonFilePath, updatedJsonContent);
                    }

                    TempData["SuccessMessage"] = "Log düzeltme işlemi başarılı";
                    return RedirectToAction("Index");
                }
                catch (System.IO.IOException ex)
                {
                    TempData["ErrorMessage"] = ex.Message;
                    // Dosya erişimi hatası oluştuğunda yapılacak işlemler
                    return RedirectToAction("Index");
                }
            }
            catch (Exception)
            {
                return View();
            }

        }


        public JsonResult GetExcelDetail(string excelFileName, int satirNo, int sutunNo, string hataNo)
        {
            try
            {
                var fileName = Directory.GetCurrentDirectory() + "\\wwwroot\\ExcelInformation\\" + excelFileName;
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                using (var stream = System.IO.File.Open(fileName, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        var dataSet = reader.AsDataSet();
                        var dataTable = dataSet.Tables[0];

                        List<object[]> eslesenSatirlar = new List<object[]>();
                        List<string> basliklar = new List<string>();

                        // Excel dosyasının 1. satırını başlıklar olarak al
                        var row = dataTable.Rows[0];
                        for (int i = 0; i < dataTable.Columns.Count; i++)
                        {
                            basliklar.Add(row[i].ToString());
                        }

                        // 1. satırı başlık olarak kabul ettiğimiz için, bu satırı atlayalım
                        for (int i = 1; i < dataTable.Rows.Count; i++)
                        {
                            row = dataTable.Rows[i];
                            if (row[sutunNo - 1].ToString() == hataNo)
                            {
                                eslesenSatirlar.Add(row.ItemArray);
                            }
                        }

                        if (eslesenSatirlar.Any())
                        {
                            return Json(new
                            {
                                basliklar = basliklar,
                                veriler = eslesenSatirlar,
                            }
                            );
                        }
                        else
                        {
                            return Json("Detay bulunamadı");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json("HATA!" + ex.Message);
            }
        }



        //public JsonResult GetExcelDetail(string excelFileName, int satirNo, int sutunNo, string hataNo)
        //{
        //    try
        //    {
        //        var fileName = Directory.GetCurrentDirectory() + "\\wwwroot\\ExcelInformation\\" + excelFileName;
        //        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

        //        using (var stream = System.IO.File.Open(fileName, FileMode.Open, FileAccess.Read))
        //        {
        //            using (var reader = ExcelReaderFactory.CreateReader(stream))
        //            {
        //                var dataSet = reader.AsDataSet();
        //                var dataTable = dataSet.Tables[0];

        //                List<object> eslesenVeriler = new List<object>();

        //                for (satirNo = 0; satirNo < dataTable.Rows.Count; satirNo++)
        //                {
        //                    if (dataTable.Rows[satirNo][sutunNo - 1].ToString() == hataNo)
        //                    {
        //                        var satirData = dataTable.Rows[satirNo].ItemArray;
        //                        eslesenVeriler.Add(new { SatirNo = satirNo + 1, Veri = satirData });
        //                    }
        //                }

        //                if (eslesenVeriler.Any())
        //                {
        //                    return Json(eslesenVeriler);
        //                }
        //                else
        //                {
        //                    return Json("Detay bulunamadı");
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json("HATA!" + ex.Message);
        //    }
        //}

        public class LogUpdateRequest
        {
           public string ExcelDosya { get; set; }
            public string Hata { get; set; }
            public int Satir { get; set; }
            public int Sutun { get; set; }
            public bool Durum { get; set; }
            public string islemTarihi { get; set; }
            public string logFileName { get; set; }

        }


        //public ActionResult BulkUpdateLogControlJson(List<LogUpdateRequest> updateRequests)
        //{
        //    try
        //    {
        //        // JSON dosyasının yolu
        //        string jsonFilePath = "jsonDosyaYolu"; // JSON dosyasının yolu burada belirtilmelidir.

        //        if (!System.IO.File.Exists(jsonFilePath))
        //        {
        //            TempData["ErrorMessage"] = "Belirtilen JSON dosyası bulunamadı.";
        //            return RedirectToAction("Index");
        //        }

        //        // JSON içeriğini okuma ve güncelleme
        //        try
        //        {
        //            string jsonContent = System.IO.File.ReadAllText(jsonFilePath);

        //            // JSON içeriğini doğrudan List<LogEntry> olarak okuyun
        //            var jArray = JArray.Parse("[" + jsonContent + "]");
        //            var logEntries = jArray.ToObject<List<LogEntry>>();

        //            foreach (var updateRequest in updateRequests)
        //            {
        //                var entryToModify = logEntries.FirstOrDefault(entry =>
        //                    entry.Timestamp == updateRequest.islemTarihi &&
        //                    entry.Message.ExcelDosya == updateRequest.excelDosya &&
        //                    entry.Message.Hata == updateRequest.hata &&
        //                    entry.Message.Satir == updateRequest.satir &&
        //                    entry.Message.Sutun == updateRequest.sutun);

        //                if (entryToModify != null)
        //                {
        //                    entryToModify.Message.Durum = updateRequest.durum;
        //                }
        //            }

        //            // JSON dosyasına geri yazın
        //            string updatedJsonContent = JsonConvert.SerializeObject(logEntries);
        //            updatedJsonContent = updatedJsonContent.TrimStart('[').TrimEnd(']');
        //            System.IO.File.WriteAllText(jsonFilePath, updatedJsonContent);

        //            TempData["SuccessMessage"] = "Toplu güncelleme işlemi başarılı";
        //            return RedirectToAction("Index");
        //        }
        //        catch (System.IO.IOException ex)
        //        {
        //            TempData["ErrorMessage"] = ex.Message;
        //            // Dosya erişimi hatası oluştuğunda yapılacak işlemler
        //            return RedirectToAction("Index");
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        return View();
        //    }
        //}



        //public ActionResult UpdateLogControlJson(string excelDosya, string hata, int satir, int sutun, bool durum, string logFileName)
        //{
        //    // JSON dosyasının yolu
        //    string jsonFilePath = Path.Combine(logFileName);

        //    if (!System.IO.File.Exists(jsonFilePath))
        //    {
        //        TempData["ErrorMessage"] = "Belirtilen JSON dosyası bulunamadı.";
        //        return RedirectToAction("Index");
        //    }

        //    // JSON içeriğini okuma ve güncelleme
        //    try
        //    {
        //        string jsonContent = System.IO.File.ReadAllText(jsonFilePath);

        //        var jArray = JArray.Parse("[" + jsonContent + "]");
        //        var logEntries = jArray.ToObject<List<LogEntry>>();

        //        var entryToModify = logEntries.FirstOrDefault(entry =>
        //            entry.Message.ExcelDosya == excelDosya &&
        //            entry.Message.Hata == hata &&
        //            entry.Message.Satir == satir &&
        //            entry.Message.Sutun == sutun);

        //        if (entryToModify != null)
        //        {
        //            entryToModify.Message.Durum = durum;
        //            System.IO.File.WriteAllText(jsonFilePath, jArray[0].ToString());
        //        }

        //        return RedirectToAction("Index");
        //    }
        //    catch (System.IO.IOException ex)
        //    {
        //        TempData["ErrorMessage"] = ex.Message;
        //        // Dosya erişimi hatası oluştuğunda yapılacak işlemler
        //        return RedirectToAction("Index");
        //    }
        //}



        //public ActionResult UpdateLogControlJson(string excelDosya, string hata, int satir, int sutun, bool durum, string logFileName)
        //{
        //    // JSON dosyasının yolu
        //    string jsonFilePath = Path.Combine(logFileName);

        //    if (!System.IO.File.Exists(jsonFilePath))
        //    {
        //        TempData["ErrorMessage"] = "Belirtilen JSON dosyası bulunamadı.";
        //        return RedirectToAction("Index");
        //    }

        //    // JSON içeriğini okuma ve güncelleme
        //    try
        //    {
        //        string jsonContent = System.IO.File.ReadAllText(jsonFilePath);

        //        // JSON dizisini doğrudan JArray olarak işlemek yerine JObject kullanın
        //        //var jArray = JArray.Parse(jsonContent);
        //        JArray jsonArray = JArray.Parse("[" + jsonContent + "]");

        //        var jsonAr = jsonArray.ToObject<List<LogEntry>>();

        //        var kekw = jsonAr.FirstOrDefault(x => x.Message.ExcelDosya == excelDosya && x.Message.Hata == hata && x.Message.Satir == satir && x.Message.Sutun == sutun).Message;

        //        kekw.Durum = durum;



        //        System.IO.File.WriteAllText(jsonFilePath, jsonAr.ToString());
        //                return RedirectToAction("Index");

        //    }
        //    catch (System.IO.IOException ex)
        //    {
        //        TempData["ErrorMessage"] = ex.Message;
        //        // Dosya erişimi hatası oluştuğunda yapılacak işlemler
        //        return RedirectToAction("Index");
        //    }
        //}


        //public ActionResult UpdateLogControlJson(string excelDosya, string hata, int satir, int sutun, bool durum, string logFileName)
        //{



        //    string jsonFilePath = Path.Combine(logFileName);

        //    if (!System.IO.File.Exists(jsonFilePath))
        //    {
        //        TempData["ErrorMessage"] = "Belirtilen JSON dosyası bulunamadı.";
        //        return RedirectToAction("Index");
        //    }

        //    var excelLogEntries = new List<ExcelLogEntry>();

        //    try
        //    {
        //        using (var fileStream = System.IO.File.Open(jsonFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
        //        using (var streamReader = new StreamReader(fileStream))
        //        {
        //            string jsonContent = streamReader.ReadToEnd();

        //            // JSON verilerini ayrıştırma
        //            JArray jsonArray = JArray.Parse("[" + jsonContent + "]");


        //            foreach (var jsonEntry in jsonArray.Children())
        //            {

        //                try
        //                {
        //                    var logEntry = jsonEntry.ToObject<LogEntry>();

        //                    // LogEntry sınıfını ExcelLogEntry sınıfına dönüştür
        //                    var excelLogEntry = new ExcelLogEntry(logEntry.Message.Hata)
        //                    {
        //                        ExcelDosya = logEntry.Message.ExcelDosya,
        //                        Hata = logEntry.Message.Hata,
        //                        Satir = logEntry.Message.Satir,
        //                        Sutun = logEntry.Message.Sutun,
        //                        Durum = logEntry.Message.Durum
        //                    };

        //                    if (excelDosya == logEntry.Message.ExcelDosya && hata == logEntry.Message.Hata && satir == logEntry.Message.Satir && sutun == logEntry.Message.Sutun)
        //                    {
        //                        logEntry.Message.Durum = durum;
        //                        System.IO.File.WriteAllText(logFileName, jsonArray.ToString());
        //                        return RedirectToAction("Index");
        //                    }

        //                    excelLogEntries.Add(excelLogEntry);
        //                }
        //                catch (JsonReaderException ex)
        //                {
        //                    TempData["ErrorMessage"] = "UYARI ! " + ex.Message;
        //                    return RedirectToAction("Index");
        //                }
        //                finally
        //                {
        //                    streamReader.Close();
        //                    fileStream.Close();
        //                }

        //            }



        //        }
        //    }
        //    catch (System.IO.IOException ex)
        //    {
        //        TempData["ErrorMessage"] = ex.Message;
        //        // Dosya erişimi hatası oluştuğunda yapılacak işlemler
        //    }
        //    return RedirectToAction("Index");
        //}



        //public ActionResult ViewLogFile(string dateFolder, string jsonFile)
        //{
        //    if (string.IsNullOrEmpty(dateFolder) || string.IsNullOrEmpty(jsonFile))
        //    {
        //        // İstenen işlemi burada işleyin veya hata mesajı döndürün
        //        TempData["ErrorMessage"] = "Hata oluştu";
        //        return RedirectToAction("Index"); // Örnek bir hata görünümü
        //    }

        //    // Geri kalan kod
        //    string logsPath = "wwwroot\\Logs";
        //    string dateFolderPath = Path.Combine(dateFolder);
        //    string jsonFilePath = Path.Combine(dateFolderPath, jsonFile);
        //    string jsonContent = System.IO.File.ReadAllText(jsonFilePath);

        //    // JSON içeriğini uygun bir C# veri yapısına çözmek için
        //    // Uygun JSON veri aralığını seçme
        //    var startIndex = jsonContent.IndexOf("\"Hata\":");
        //    var endIndex = jsonContent.IndexOf("\"Properties\": {}");

        //    // JSON verisini aralığa göre çıkartma
        //    string extractedJson = jsonContent.Substring(startIndex, endIndex - startIndex + 13); // 13, "Properties": {}" uzunluğunu temsil eder

        //    // Çıkartılan JSON verisini uygun bir C# veri yapısına çözme
        //    var logEntry = Newtonsoft.Json.JsonConvert.DeserializeObject<LogEntry>(extractedJson);

        //    // Bu C# veri yapısını View'de görüntülemek için kullanın
        //    return View(logEntry);
        //}










    }
}
