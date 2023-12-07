using DesignTech_PLM_Entegrasyon_App.MVC.Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace DesignTech_PLM_Entegrasyon_App.MVC.Controllers.Modules.Excel
{
    [Authorize]
    public class PlmUploadExcelController : Controller
	{
        private readonly IConfiguration _configuration;

		public PlmUploadExcelController(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public IActionResult Index(int page = 1, string search = "")
		{
			try
			{
				string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ExcelInformation");
				string[] filePaths = Directory.GetFiles(directoryPath, "*.*")
											.Select(name => Path.GetFileName(name))
											.OrderByDescending(f => System.IO.File.GetCreationTime(Path.Combine(directoryPath, f)))
											.ToArray();

				// Arama filtresini uygula
				if (!string.IsNullOrEmpty(search))
				{
					filePaths = filePaths.Where(f => f.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0).ToArray();
				}


				int pageSize = 12; // Sayfa başına dosya sayısı
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
                            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\ExcelInformation", randomName);

                            using (var stream = new FileStream(path, FileMode.Create))
                            {
                                await formData.CopyToAsync(stream);
                            }
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Yalnızca Excel dosyalarına izin verilmektedir.";
                            return RedirectToAction("Index", "PlmUploadExcel");
                        }
                    }
                }
                LogService logService = new LogService(_configuration);
				var loggedInUsername = HttpContext.User.Identity.Name;
				logService.AddNewLogEntry("Excel dosyaları başarıyla yüklendi." , randomName , "Yüklendi",loggedInUsername);
                logService.AddNewLogEntry2("Excel dosyaları başarıyla yüklendi." , randomName , "Yüklendi");
                TempData["SuccessMessage"] = "Excel dosyaları başarıyla yüklendi.";
                //Log.Information("Excel dosyaları başarıyla yüklendi.FileName:"+randomName+"Operation:Yüklendi") ;
                return RedirectToAction("Index", "PlmUploadExcel");
            }
            catch (Exception ex)
            {
                TempData["SystemErrorMessage"] = ex.Message;
                Log.Error("Upload Excel File Message: " + ex.Message + " Kullanıcı : Onur ÖZÇELİK" + " Kullanıcı ID : 1");
                return RedirectToAction("Index", "PlmUploadExcel");
            }
        }

        //[HttpPost]
        //public async Task<IActionResult> ExcelUpload(IFormFileCollection formFile)
        //{
        //	try
        //	{
        //		foreach (var formData in formFile)
        //		{
        //			if (formData.Length > 0)
        //			{
        //				var extent = Path.GetExtension(formData.FileName);
        //				string originalFileName = Path.GetFileNameWithoutExtension(formData.FileName);
        //				string timeStamp = DateTime.Now.ToString("yyyy-MM-dd-HH-mm");

        //				string randomName = $"{originalFileName}_{timeStamp}{extent}";

        //				var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\ExcelInformation", randomName);

        //				using (var stream = new FileStream(path, FileMode.Create))
        //				{
        //					await formData.CopyToAsync(stream);
        //				}
        //			}
        //		}

        //		TempData["SuccessMessage"] = "Excel dosyaları başarıyla yüklendi.";
        //		Log.Error("Upload Excel File Message :  Başarılı - Kullanıcı : Onur ÖZÇELİK" + "Kullanıcı ID : 1");
        //		return RedirectToAction("Index", "PlmUploadExcel");
        //	}
        //	catch (Exception ex)
        //	{
        //		TempData["SystemErrorMessage"] = ex.Message;
        //		Log.Error("Upload Excel File Message : " + ex.Message + "Kullanıcı : Onur ÖZÇELİK" + "Kullanıcı ID : 1");
        //		return RedirectToAction("Index", "PlmUploadExcel");
        //	}
        //}

        [HttpPost]
		public IActionResult DeleteFile(string fileName)
		{
			try
			{

				string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ExcelInformation");
				string filePath = Path.Combine(directoryPath, fileName);

				if (System.IO.File.Exists(filePath))
				{
					System.IO.File.Delete(filePath);
                    LogService logService = new LogService(_configuration);
					var loggedInUsername = HttpContext.User.Identity.Name;
					logService.AddNewLogEntry("Excel dosyası silindi.", fileName, "Silindi",loggedInUsername);
                    logService.AddNewLogEntry2("Excel dosyası silindi.", fileName, "Silindi");
					//Log.Information("Excel dosyası silindi.FileName:"+fileName+"Operation:Silme");
					TempData["SuccessMessage"] = "Dosya başarıyla silindi.";


					//İşlem başarılı olduğunda loglama
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




		//Excel Klasör Döküman Sayfası


	}
}
