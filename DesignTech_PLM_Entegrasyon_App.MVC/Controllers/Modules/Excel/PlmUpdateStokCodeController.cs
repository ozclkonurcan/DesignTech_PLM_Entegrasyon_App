using Dapper;
using DesignTech_PLM_Entegrasyon_App.MVC.Helper;
using DesignTech_PLM_Entegrasyon_App.MVC.ViewModels;
using ExcelDataReader;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using SqlKata.Execution;
using System.Data;
using System.Text.RegularExpressions;

namespace DesignTech_PLM_Entegrasyon_App.MVC.Controllers.Modules.Excel
{
    [Authorize]
    public class PlmUpdateStokCodeController : Controller
    {
        private readonly IConfiguration _configuration;
        public QueryFactory _plm2;

        public PlmUpdateStokCodeController(IConfiguration configuration, QueryFactory plm2)
        {
            _configuration = configuration;
            _plm2 = plm2;
        }

        public IActionResult Index(int page = 1, string search = "")
        {
            try
            {
                string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ExcelUpdateStockCode");
                string[] filePaths = Directory.GetFiles(directoryPath, "*.*")
                                            .Select(name => Path.GetFileName(name))
                                            .OrderByDescending(f => System.IO.File.GetCreationTime(Path.Combine(directoryPath, f)))
                                            .ToArray();

                // Arama filtresini uygula
                if (!string.IsNullOrEmpty(search))
                {
                    filePaths = filePaths.Where(f => f.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0).ToArray();
                }


                int pageSize = 8; // Sayfa başına dosya sayısı
                int totalCount = filePaths.Length;
                int pageCount = (int)Math.Ceiling((double)totalCount / pageSize);

                // Geçerli sayfayı kontrol et
                if (page < 1)
                    page = 1;
                else if (page > pageCount)
                    page = pageCount;

                // Sayfaya göre dosyaları al
                var paginatedFiles = filePaths.Skip((page - 1) * pageSize).Take(pageSize).ToArray();

                ViewBag.filelist = paginatedFiles;
                ViewBag.fileCount = totalCount;
                ViewBag.pageSize = pageSize; // Sayfa boyutunu view'a ekleyin
                ViewBag.page = page; // Sayfa numarasını view'a ekleyin
                ViewBag.search = search; // Arama terimini view'a ekleyin
                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "HATA!" + ex.Message;
                Log.Error("List Excel File Message : " + ex.Message + "Kullanıcı : Onur ÖZÇELİK" + "Kullanıcı ID : 1");
                return RedirectToAction("Index");
            }
        }


        //Excel Klasör Döküman Sayfası



        [HttpPost]
        public async Task<IActionResult> ExcelUpload(IFormFileCollection formFile)
        {
            try
            {

                var randomName = "";
                foreach (var formData in formFile)
                {
                    if (formData.Length > 0)
                    {
                        // Dosya uzantısını al
                        var fileExtension = Path.GetExtension(formData.FileName).ToLower();

                        // İzin verilen dosya uzantıları
                        string[] allowedExtensions = { ".xls", ".xlsx" }; // Sadece Excel dosyalarına izin veriyoruz

                        if (allowedExtensions.Contains(fileExtension))
                        {
                            var originalFileName = Path.GetFileNameWithoutExtension(formData.FileName);
                            var timeStamp = DateTime.Now.ToString("yyyy-MM-dd-HH-mm");
                            randomName = $"{originalFileName}_{timeStamp}{fileExtension}";
                            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\ExcelUpdateStockCode", randomName);

                            using (var stream = new FileStream(path, FileMode.Create))
                            {
                                await formData.CopyToAsync(stream);
                            }
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Yalnızca Excel dosyalarına izin verilmektedir.";
                            return RedirectToAction("Index", "PlmUpdateStokCode");
                        }
                    }
                }

                TempData["SuccessMessage"] = "Excel dosyaları başarıyla yüklendi.";
                LogService logService = new LogService(_configuration);
                var loggedInUsername = HttpContext.User.Identity.Name;
                logService.AddNewLogEntry("Excel dosyası başarıyla yüklendi.", randomName, "Yüklendi", loggedInUsername);
                Log.Error("Upload Excel File Message: Başarılı - Kullanıcı : Onur ÖZÇELİK" + " Kullanıcı ID : 1");
                return RedirectToAction("Index", "PlmUpdateStokCode");
            }
            catch (Exception ex)
            {
                TempData["SystemErrorMessage"] = ex.Message;
                Log.Error("Upload Excel File Message: " + ex.Message + " Kullanıcı : Onur ÖZÇELİK" + " Kullanıcı ID : 1");
                return RedirectToAction("Index", "PlmUpdateStokCode");
            }
        }



        [HttpPost]
        public IActionResult DeleteFile(string fileName)
        {
            try
            {
                string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ExcelUpdateStockCode");
                string filePath = Path.Combine(directoryPath, fileName);

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                    LogService logService = new LogService(_configuration);
                    var loggedInUsername = HttpContext.User.Identity.Name;
                    logService.AddNewLogEntry("Excel dosyası silindi.", fileName, "Silindi", loggedInUsername);
                    TempData["SuccessMessage"] = "Dosya başarıyla silindi.";


                }
                else
                {
                    TempData["ErrorMessage"] = "Dosya bulunamadı.";
                    Log.Warning("Delete Excel File Message : Dosya başarıyla silindi." + "Kullanıcı : Onur ÖZÇELİK" + "Kullanıcı ID : 1");

                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "HATA!" + ex.Message;
                Log.Error("Delete Excel File Message : " + ex.Message + "Kullanıcı : Onur ÖZÇELİK" + "Kullanıcı ID : 1");
            }

            return RedirectToAction("Index");
        }



        //Integrate

        public async Task<IActionResult> ExcelUpdateIntegrateIndex(string file)
        {
            try
            {


                var fileName = Directory.GetCurrentDirectory() + "\\wwwroot\\ExcelUpdateStockCode\\" + file;
                var schema = _configuration["Catalog"];
                var connectionString = _configuration.GetConnectionString("Plm");
                using IDbConnection connection = new SqlConnection(connectionString);



                DataSet excelData;
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                await using (var stream = System.IO.File.Open(fileName, FileMode.Open, FileAccess.Read))
                {
                    using (IExcelDataReader reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        excelData = reader.AsDataSet(new ExcelDataSetConfiguration()
                        {
                            ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                            {
                                UseHeaderRow = true
                            }
                        });
                    }
                }

                foreach (DataRow excelRow in excelData.Tables[0].Rows)
                {
                    var Number = excelRow["Number"].ToString();
                    var Stock_Code = excelRow["Stock_Code"].ToString();
                    var Status = excelRow["Status"].ToString();


                    var catalogValue = _configuration["Catalog"];

                    using (SqlConnection conn3Sel = new SqlConnection(connectionString))
                    {
                        conn3Sel.Open();

                        // Eski Name değerini al
                        var getOldName = $"SELECT Name FROM {catalogValue}.WTPartMaster WHERE WTPartNumber = @Number";

                        var getOldName2 = $"SELECT Name FROM {catalogValue}.WTPartMaster WHERE WTPartNumber = @Stock_Code";


						//var getOldName = $"SELECT " +
						//	 $"CASE " +
						//	 $"WHEN WTPartNumber != @Number AND WTPartNumber != @Stock_Code THEN 1 " +
						//	 $"WHEN WTPartNumber != @Number AND WTPartNumber = @Stock_Code THEN 2  " +
						//	 $"WHEN WTPartNumber = @Number AND WTPartNumber != @Stock_Code THEN 3 " +
						//	 $"ELSE 0  " +
						//	 $"END " +
						//	 $"Sonuc FROM {catalogValue}.WTPartMaster WHERE WTPartNumber IN (@Number, @Stock_Code)";

						using (SqlCommand getNameCmd = new SqlCommand(getOldName, conn3Sel))
                        {
                            getNameCmd.Parameters.AddWithValue("@Number", Number);
                            getNameCmd.Parameters.AddWithValue("@Stock_Code", Stock_Code);
                            var oldName = (string)getNameCmd.ExecuteScalar();

							using (SqlCommand getNameCmd2 = new SqlCommand(getOldName2, conn3Sel))
							{

								getNameCmd2.Parameters.AddWithValue("@Stock_Code", Stock_Code);

								string oldName2 = (string)getNameCmd2.ExecuteScalar();

								if (oldName != null || oldName2 != null)
                            {
                                if(oldName2 != null)
                                {
									excelRow["Status"] = "Mükerrer Kayıt";
                                }
                                else
                                {
									excelRow["Status"] = oldName;
								}
                                // Eğer EPMDocumentMaster tablosunda bulunduysa


                            }
                            else
                            {
                                excelRow["Status"] = "PLM'de kaydı yok.";
                            }
							}



							// Eski Name değerini Excel'e yaz
						}


                    }

                }



                ViewBag.exceldata = excelData.Tables[0];
                ViewBag.excelfile = file;
                return View();



            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "HATA!" + ex.Message;
                return RedirectToAction("Index");
            }
        }





        //      [HttpPost]
        //      public JsonResult updatePlmExcelFileData(string file)
        //      {
        //          var fileName = Directory.GetCurrentDirectory() + "\\wwwroot\\ExcelUpdateStockCode\\" + file;
        //          var schema = _configuration["Catalog"];
        //          var connectionString = _configuration.GetConnectionString("Plm");
        //          using IDbConnection connection = new SqlConnection(connectionString);



        //          DataSet excelData;
        //          System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        //           using (var stream = System.IO.File.Open(fileName, FileMode.Open, FileAccess.Read))
        //          {
        //              using (IExcelDataReader reader = ExcelReaderFactory.CreateReader(stream))
        //              {
        //                  excelData = reader.AsDataSet(new ExcelDataSetConfiguration()
        //                  {
        //                      ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
        //                      {
        //                          UseHeaderRow = true
        //                      }
        //                  });
        //              }
        //          }

        //          try
        //	{
        //              foreach (DataRow excelRow in excelData.Tables[0].Rows)
        //              {
        //                  var Number = excelRow["Number"].ToString();
        //                  var Stock_Code = excelRow["Stock_Code"].ToString();
        //                  var Status = excelRow["Status"].ToString();

        //                  var catalogValue = _configuration["Catalog"];

        //                  using (SqlConnection conn3Sel = new SqlConnection(connectionString))
        //                  {
        //                      conn3Sel.Open();

        //                      // Güncelleme işlemini yap
        //                      var updateQuery = $"UPDATE {catalogValue}.WTPartMaster SET WTPartNumber = @Stock_Code WHERE WTPartNumber = @Number";

        //                      using (SqlCommand updateCmd = new SqlCommand(updateQuery, conn3Sel))
        //                      {
        //                          updateCmd.Parameters.AddWithValue("@Stock_Code", Stock_Code);
        //                          updateCmd.Parameters.AddWithValue("@Number", Number);

        //                          updateCmd.ExecuteNonQuery();
        //                      }
        //                  }
        //              }





        //              ViewBag.exceldata = excelData.Tables[0];
        //              ViewBag.excelfile = file;
        //              return Json(new { success = true });

        //          }
        //          catch (Exception)
        //	{
        //              return Json(new { success = false });

        //	}
        //          // Burada yapılacak işlemleri ekleyin



        //}

        //Bunda kayıtlı veri var ise güncelleme iptal ediliyor.
        public class stokCodeErrorManagement
        {
            public string oldNumber { get; set; }
            public string newNumber { get; set; }
            public string status { get; set; }
        }
        public IActionResult updatePlmExcelFileData(string file)
        {
            var fileName = Directory.GetCurrentDirectory() + "\\wwwroot\\ExcelUpdateStockCode\\" + file;
            var schema = _configuration["Catalog"];
            var connectionString = _configuration.GetConnectionString("Plm");
            using IDbConnection connection = new SqlConnection(connectionString);



            DataSet excelData;
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            using (var stream = System.IO.File.Open(fileName, FileMode.Open, FileAccess.Read))
            {
                using (IExcelDataReader reader = ExcelReaderFactory.CreateReader(stream))
                {
                    excelData = reader.AsDataSet(new ExcelDataSetConfiguration()
                    {
                        ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                        {
                            UseHeaderRow = true
                        }
                    });
                }
            }


      

            try
            {
                if (excelData != null && excelData.Tables.Count > 0 && excelData.Tables[0].Rows.Count > 0)
                {

                    List<string> failedUpdates = new List<string>(); // List to store failed updates
                    List<string> successfulUpdates = new List<string>();
                    List<string> emptyDataFailedUpdates = new List<string>();
                    List<stokCodeErrorManagement> stokCodeProcessStatus = new List<stokCodeErrorManagement>();

                    foreach (DataRow excelRow in excelData.Tables[0].Rows)
                    {
                        var Number = excelRow["Number"].ToString();
                        var Stock_Code = excelRow["Stock_Code"].ToString();
                        var Status = excelRow["Status"].ToString();

                        var catalogValue = _configuration["Catalog"];

                        using (SqlConnection conn3Sel = new SqlConnection(connectionString))
                        {
                            conn3Sel.Open();

                            //var checkExistingQuery = $"SELECT COUNT(*) FROM {catalogValue}.WTPartMaster WHERE WTPartNumber = @Stock_Code";
                            var checkExistingQuery = $"SELECT " +
                                $"CASE " +
                                $"WHEN WTPartNumber != @Number AND WTPartNumber != @Stock_Code THEN 1 " +
                                $"WHEN WTPartNumber != @Number AND WTPartNumber = @Stock_Code THEN 2  " +
                                $"WHEN WTPartNumber = @Number AND WTPartNumber != @Stock_Code THEN 3 " +
                                $"ELSE 0  " +
                                $"END " +
                                $"Sonuc FROM {catalogValue}.WTPartMaster WHERE WTPartNumber IN (@Number, @Stock_Code)";


                            


                            using (SqlCommand checkCmd = new SqlCommand(checkExistingQuery, conn3Sel))
                            {
                                checkCmd.Parameters.AddWithValue("@Stock_Code", Stock_Code);
                                checkCmd.Parameters.AddWithValue("@Number", Number);
                                try
                                {

                                int existingCount = (int)(checkCmd.ExecuteScalar() ?? 1);
                                var checkNumberQuery = $"SELECT COUNT(*) FROM {catalogValue}.WTPartMaster WHERE WTPartNumber = @Number and WTPartNumber = @Stock_Code";
                                using (var checkNumberCmd = new SqlCommand(checkNumberQuery, conn3Sel))
                                {

                                    checkNumberCmd.Parameters.AddWithValue("@Number", Number);
                                    checkNumberCmd.Parameters.AddWithValue("@Stock_Code", Stock_Code);

                                    int numberCount = (int)checkNumberCmd.ExecuteScalar();

                                            LogService logService = new LogService(_configuration);
                                            var loggedInUsername = HttpContext.User.Identity.Name;
                                    
                                    if (existingCount == 2)
                                    {

                                        failedUpdates.Add(Stock_Code);
                                            stokCodeProcessStatus.Add(
   new stokCodeErrorManagement
   {
       oldNumber = Number,
       newNumber = Stock_Code,
       status = Stock_Code + " Mükerrer kayıt (değişiklik yapılmadı)."
   });

                                            logService.AddNewLogEntry(Number + " ==> " + Stock_Code + " [" + Stock_Code + " Sistem de zaten mevcut.] ", file, "Güncellenmedi",loggedInUsername);
                                        }
                                    
                                    if(existingCount == 1)
                                    {
                                        emptyDataFailedUpdates.Add(Number);

                                            logService.AddNewLogEntry(Number + " ==> " + Stock_Code + " ["+Number+ " Sistem de bulunamadı.] ", file, "Güncellenmedi",loggedInUsername);
                                            stokCodeProcessStatus.Add(
new stokCodeErrorManagement
{
oldNumber = Number,
newNumber = Stock_Code,
status = Number + " PLM'de kaydı yok."
});
                                        }
                                    
                                    if(existingCount == 3)
                                    {
                                        // Güncelleme işlemini yap
                                        var updateQuery = $"UPDATE {catalogValue}.WTPartMaster SET WTPartNumber = @Stock_Code WHERE WTPartNumber = @Number";

                                        using (SqlCommand updateCmd = new SqlCommand(updateQuery, conn3Sel))
                                        {
                                            updateCmd.Parameters.AddWithValue("@Stock_Code", Stock_Code);
                                            updateCmd.Parameters.AddWithValue("@Number", Number);

                                            updateCmd.ExecuteNonQuery();
                                            logService.AddNewLogEntry(Number + " ==> " + Stock_Code, file, "Güncellendi", loggedInUsername);
                                            successfulUpdates.Add(Number + " ==> " + Stock_Code);

                                                stokCodeProcessStatus.Add(
new stokCodeErrorManagement
{
oldNumber = Number,
newNumber = Stock_Code,
status = Number + " Güncellendi."
});
                                            }
                                    }


                                }

                                }
                                catch (Exception)
                                {
                                    LogService logService = new LogService(_configuration);
                                    var loggedInUsername = HttpContext.User.Identity.Name;
                                    emptyDataFailedUpdates.Add(Number);
                                    logService.AddNewLogEntry(Number + " ==> " + Stock_Code + " [" + Number + " Sistem de bulunamadı.] ", file, "Güncellenmedi", loggedInUsername);
                                    stokCodeProcessStatus.Add(
new stokCodeErrorManagement
{
oldNumber = Number,
newNumber = Stock_Code,
status = Number + " PLM'de kaydı yok."
});
                                    continue;
                                }

                            }
                        }



                    }

             

                    ViewBag.exceldata = excelData.Tables[0];
                    ViewBag.exceldataCount = excelData.Tables[0].Rows.Count;
                    ViewBag.excelfile = file;
                    ViewBag.successfulUpdates = successfulUpdates;
                    ViewBag.successfulUpdatesCount = successfulUpdates.Count;
                    ViewBag.emptyDataFailedUpdates = emptyDataFailedUpdates;
                    ViewBag.emptyDataFailedUpdatesCount = emptyDataFailedUpdates.Count;
                    ViewBag.failedUpdates = failedUpdates;
                    ViewBag.failedUpdatesCount = failedUpdates.Count;
                    ViewBag.stokCodeProcessStatus = stokCodeProcessStatus;

                    if (failedUpdates.Count > 0 || emptyDataFailedUpdates.Count > 0 || successfulUpdates.Count > 0)
                    {


                        //string message = $"Aşağıda bulunan Stok Kod kayıtları zaten sisteme daha önce eklenmiş lütfen excel dosyasını kontrol edin.: [ {string.Join(", ", failedUpdates)} ] , Başarılı Kayıtlar : [ {string.Join(", ", successfulUpdates)} ] , Number değeri SQL de bulunmayan kayıtalr : [ {string.Join(", ", emptyDataFailedUpdates)} ]";

                        //string message2 = "İşlem tamamlandı. Hatalı olanlar işlenmedi. Lütfen excel dosyasını kontrol edin";
                        //TempData["UpdateStockCodeErrorMessage"] = message;

                        //TempData["UpdateStockCodeToastrErrorMessage"] = message2;

                        //return RedirectToAction("updatePlmExcelFileData", "PlmUpdateStokCode");

                        return View();

                        //return Json(new { success = false, message });


                    }
                    else
                    {

                        TempData["UpdateStockCodeToastrSuccessMessage"] = "Güncelleme işlemi başarılı bir şekilde gerçekleştirildi.";

                        return View();
                        //return Json(new { success = true });
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Boş değer hatası: Tablo boş.";


                    return View();
                }
            }
            catch (Exception)
            {

                return View();
            }
            // Burada yapılacak işlemleri ekleyin



        }




    }
}
