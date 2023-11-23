using Dapper;
using DesignTech_PLM_Entegrasyon_App.MVC.ViewModels;
using ExcelDataReader;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Serilog;
using SqlKata.Execution;
using System.Data;
using System.Text.RegularExpressions;

namespace DesignTech_PLM_Entegrasyon_App.MVC.Controllers.Modules.Excel
{
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
							var randomName = $"{originalFileName}_{timeStamp}{fileExtension}";
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

                            using (SqlCommand getNameCmd = new SqlCommand(getOldName, conn3Sel))
                            {
                                getNameCmd.Parameters.AddWithValue("@Number", Number);
                                var oldName = (string)getNameCmd.ExecuteScalar();

                                if (oldName != null)
                                {
                                    // Eğer EPMDocumentMaster tablosunda bulunduysa
                                    excelRow["Status"] = oldName;
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
		[HttpPost]
		public JsonResult updatePlmExcelFileData(string file)
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
                List<string> failedUpdates = new List<string>(); // List to store failed updates

                foreach (DataRow excelRow in excelData.Tables[0].Rows)
                {
                    var Number = excelRow["Number"].ToString();
                    var Stock_Code = excelRow["Stock_Code"].ToString();
                    var Status = excelRow["Status"].ToString();

                    var catalogValue = _configuration["Catalog"];

                    using (SqlConnection conn3Sel = new SqlConnection(connectionString))
                    {
                        conn3Sel.Open();

                        var checkExistingQuery = $"SELECT COUNT(*) FROM {catalogValue}.WTPartMaster WHERE WTPartNumber = @Stock_Code";

                        using (SqlCommand checkCmd = new SqlCommand(checkExistingQuery, conn3Sel))
                        {
                            checkCmd.Parameters.AddWithValue("@Stock_Code", Stock_Code);

                            int existingCount = (int)checkCmd.ExecuteScalar();

                            if (existingCount == 0)
                            {
                                // Güncelleme işlemini yap
                                var updateQuery = $"UPDATE {catalogValue}.WTPartMaster SET WTPartNumber = @Stock_Code WHERE WTPartNumber = @Number";

                                using (SqlCommand updateCmd = new SqlCommand(updateQuery, conn3Sel))
                                {
                                    updateCmd.Parameters.AddWithValue("@Stock_Code", Stock_Code);
                                    updateCmd.Parameters.AddWithValue("@Number", Number);

                                    updateCmd.ExecuteNonQuery();
                                }
                            }
                            else
                            {
                                failedUpdates.Add(Number); 
                            }
                        }
                    }
                }

                ViewBag.exceldata = excelData.Tables[0];
                ViewBag.excelfile = file;

                if (failedUpdates.Count > 0)
                {


                    string message = $"Aşağıdaki kayıtlar zaten sistem de mevcut: {string.Join(", ", failedUpdates)}";

					string message2 = "İşlem tamamlandı. Hatalı olanlar işlenmedi. Lütfen excel dosyasını kontrol edin";
					TempData["UpdateStockCodeErrorMessage"] = message;

					TempData["UpdateStockCodeToastrErrorMessage"] = message2;
					return Json(new { success = false, message });
               
				
				}
                else
                {
					TempData["UpdateStockCodeToastrSuccessMessage"] = "Güncelleme işlemi başarılı bir şekilde gerçekleştirildi.";

					return Json(new { success = true });
                }
            }
            catch (Exception)
            {
                return Json(new { success = false });
            }
            // Burada yapılacak işlemleri ekleyin



        }




	}
}
