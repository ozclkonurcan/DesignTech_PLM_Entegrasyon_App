using Dapper;
using DesignTech_PLM_Entegrasyon_App.MVC.Dtos;
using DesignTech_PLM_Entegrasyon_App.MVC.Helper;
using DesignTech_PLM_Entegrasyon_App.MVC.Models;
using DesignTech_PLM_Entegrasyon_App.MVC.ViewModels;
using ExcelDataReader;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Serilog;
using SqlKata.Execution;
using System.Data;
using System.Text.RegularExpressions;
using static DesignTech_PLM_Entegrasyon_App.MVC.Controllers.Modules.Excel.PlmUpdateStokCodeController;

namespace DesignTech_PLM_Entegrasyon_App.MVC.Controllers.Modules.Excel
{
	[Authorize]
	public class PlmLoadExcelRelatePartsToDocumentController : Controller
	{
		private readonly IConfiguration _configuration;
		public QueryFactory _plm2;
		public PlmLoadExcelRelatePartsToDocumentController(QueryFactory queryFactory, IConfiguration configuration)
		{
			_configuration = configuration;
			_plm2 = new PlmDatabase(configuration).Connect();
		}
		public IActionResult Index(int page = 1, string search = "")
		{
			try
			{
				string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ExcelRelatePartsToDocumentFolder");
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
							var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\ExcelRelatePartsToDocumentFolder", randomName);

							using (var stream = new FileStream(path, FileMode.Create))
							{
								await formData.CopyToAsync(stream);
							}
						}
						else
						{
							TempData["ErrorMessage"] = "Yalnızca Excel dosyalarına izin verilmektedir.";
							return RedirectToAction("Index");
						}
					}
				}
				LogService logService = new LogService(_configuration);
				var loggedInUsername = HttpContext.User.Identity.Name;
				logService.AddNewLogEntry("Excel dosyaları başarıyla yüklendi.", randomName, "Yüklendi", loggedInUsername);
				logService.AddNewLogEntry2("Excel dosyaları başarıyla yüklendi.", randomName, "Yüklendi");
				TempData["SuccessMessage"] = "Excel dosyaları başarıyla yüklendi.";
				//Log.Information("Excel dosyaları başarıyla yüklendi.FileName:"+randomName+"Operation:Yüklendi") ;
				return RedirectToAction("Index");
			}
			catch (Exception ex)
			{
				TempData["SystemErrorMessage"] = ex.Message;
				Log.Error("Upload Excel File Message: " + ex.Message + " Kullanıcı : Onur ÖZÇELİK" + " Kullanıcı ID : 1");
				return RedirectToAction("Index");
			}
		}


		[HttpPost]
		public IActionResult DeleteFile(string fileName)
		{
			try
			{

				string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ExcelRelatePartsToDocumentFolder");
				string filePath = Path.Combine(directoryPath, fileName);

				if (System.IO.File.Exists(filePath))
				{
					System.IO.File.Delete(filePath);
					LogService logService = new LogService(_configuration);
					var loggedInUsername = HttpContext.User.Identity.Name;
					logService.AddNewLogEntry("Excel dosyası silindi.", fileName, "Silindi", loggedInUsername);
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



		public async Task<IActionResult> ExcelIntegrateIndex(string file)
		{
			try
			{

				var fileName = Directory.GetCurrentDirectory() + "\\wwwroot\\ExcelRelatePartsToDocumentFolder\\" + file;
				var schema = _configuration["Catalog"];
				var connectionString = _configuration.GetConnectionString("Plm");
				using IDbConnection connection = new SqlConnection(connectionString);

				var IntegrationResult = connection.Query<IntegrationDefinitionListViewModel>($"SELECT * FROM {schema}.IntegrationDefinitionList").ToList();

				// Eşleşecek deseni belirtin
				string pattern = @"wt\.iba\.definition\.(.*?)Definition";

				foreach (var item in IntegrationResult)
				{
					string cellValue = item.classnameA2A2;

					Match match = Regex.Match(cellValue, pattern);
					if (match.Success)
					{
						string matchedValue = match.Groups[1].Value;

						// Ayıklanan değeri "type" özelliğine atayın
						item.type = matchedValue.ToLower();
					}
				}

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
					try
					{


						var Anaparca = excelRow["Number"].ToString();
						var Name = excelRow["NAME"].ToString();
						var catalogValue = _configuration["Catalog"];
						if (!string.IsNullOrEmpty(Name))
						{
							using (SqlConnection conn3Sel = new SqlConnection(connectionString))
							{
								conn3Sel.Open();

								// Eski Name değerini al
								var getOldName = $"SELECT Name FROM {catalogValue}.EPMDocumentMaster WHERE documentNumber = @Anaparca";
								var getOldNameWTPart = $"SELECT Name FROM {catalogValue}.WTPartMaster WHERE WTPartNumber = @Anaparca";

								using (SqlCommand getNameCmd = new SqlCommand(getOldName, conn3Sel))
								{
									getNameCmd.Parameters.AddWithValue("@Anaparca", Anaparca);
									var oldName = (string)getNameCmd.ExecuteScalar();


									if (oldName != null)
									{
										// Eğer EPMDocumentMaster tablosunda bulunduysa
										excelRow["ESKI_NAME"] = oldName;
									}
									else
									{
										// Eğer bulunamazsa, WTPartMaster tablosunda ara
										using (SqlCommand getNameCmdWTPart = new SqlCommand(getOldNameWTPart, conn3Sel))
										{
											getNameCmdWTPart.Parameters.AddWithValue("@Anaparca", Anaparca);
											var oldNameWTPart = (string)getNameCmdWTPart.ExecuteScalar();
											excelRow["ESKI_NAME"] = oldNameWTPart;
										}
									}
									// Eski Name değerini Excel'e yaz
								}
							}

						}
					}
					catch (Exception)
					{
						continue;
					}

				}
				ViewBag.AttrList = IntegrationResult;
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




        //public IActionResult ExcelProcPart(IFormCollection data)
        public class excelProcPartErrorManagement
        {
            public string value { get; set; }
            public string status { get; set; }
        }
		
        public IActionResult ExcelProcPart(string excelFilePath)
        {

            try
            {
                List<string> failedUpdates = new List<string>(); 
                List<string> successfulUpdates = new List<string>();
                List<string> emptyDataFailedUpdates = new List<string>();
                List<excelProcPartErrorManagement> stokCodeProcessStatus = new List<excelProcPartErrorManagement>();
                //string excelFilePath = data["excel"].ToString();
                var fileName = Directory.GetCurrentDirectory() + "\\wwwroot\\ExcelRelatePartsToDocumentFolder\\" + excelFilePath;
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

                List<GenericPartObject> RecordList = new List<GenericPartObject>();

                foreach (DataRow excelRow in excelData.Tables[0].Rows)
                {
                    var Number = excelRow[2].ToString();
                    var Version = excelRow[3].ToString();

                    foreach (DataColumn excelHeader in excelData.Tables[0].Columns)
                    {
                        if (excelHeader.ColumnName.ToString().ToUpper().Contains("DOC"))
                        {
                            GenericPartObject GenericPartObjectData = new GenericPartObject();
                            GenericPartObjectData.Number = Number;
                            GenericPartObjectData.Version = Version;
                            GenericPartObjectData.PartNumber = excelRow[excelHeader.ColumnName.ToString()].ToString();
                            RecordList.Add(GenericPartObjectData);
                        }
                    }





                }
                foreach (GenericPartObject PlmDbPartPRoc in RecordList)
                {
                    var catalogValue = _configuration["Catalog"];
                    var ParcaNo = _plm2.Query(catalogValue + ".WTPartNoList").Where(new
                    {
                        WTPartNumber = PlmDbPartPRoc.Number,
                        versionIdA2versionInfo = PlmDbPartPRoc.Version
                    }).Get().ToList();



                    var WTDocNo = _plm2.Query(catalogValue + ".WTDocList").Where(new
                    {
                        WTDocumentNumber = PlmDbPartPRoc.PartNumber
                    }).Get().ToList();
                    var IdSeq = _plm2.Query(catalogValue + ".id_sequence").OrderByDesc("value").FirstOrDefault();
                    LogService logService = new LogService(_configuration);
                    var loggedInUsername = HttpContext.User.Identity.Name;
                    try
					{
                       
                        if (ParcaNo.Count() > 0 && PlmDbPartPRoc.PartNumber.Length > 0)
                        {
                            WTPartDescribeLink DescribData = new WTPartDescribeLink();
                            DescribData.classnamekeyroleAObjectRef = "wt.part.WTPart";
                            DescribData.idA3A5 = Convert.ToInt64(ParcaNo[0].WTPartNo);
                            DescribData.classnamekeyroleBObjectRef = "wt.doc.WTDocument";
                            DescribData.idA3B5 = Convert.ToInt64(WTDocNo[0].WTDocNo);
                            DescribData.createStampA2 = DateTime.Now.Date;
                            DescribData.markForDeleteA2 = 0;
                            DescribData.modifyStampA2 = DateTime.Now.Date;
                            DescribData.classnameA2A2 = "wt.part.WTPartDescribeLink";
                            DescribData.idA2A2 = Convert.ToInt64(Convert.ToInt64(IdSeq.value) + 100);
                            DescribData.updateCountA2 = 1;
                            DescribData.updateStampA2 = DateTime.Now.Date;
                            DescribData.branchIdA2typeDefinitionRefe = 0;
                            DescribData.idA2typeDefinitionReference = 0;
                            var insert = _plm2.Query(catalogValue + ".WTPartDescribeLink").Insert(DescribData);
                            if (insert == 1)
                            {
                                _plm2.Query(catalogValue + ".id_sequence").Insert(new { dummy = "x" });

                            }
							
                            logService.AddNewLogEntry(PlmDbPartPRoc.Number + " => " + PlmDbPartPRoc.PartNumber+" ile ilişkilendirildi.", null, "İlişkilendirildi", loggedInUsername);
							successfulUpdates.Add(PlmDbPartPRoc.Number + " => " + PlmDbPartPRoc.PartNumber);
							stokCodeProcessStatus.Add(new excelProcPartErrorManagement
							{
								value = PlmDbPartPRoc.Number + " => " + PlmDbPartPRoc.PartNumber,
								status = "İlişkilendirildi"
                            });

                        }
                        else
						{
                            if (PlmDbPartPRoc.PartNumber == "" || PlmDbPartPRoc.PartNumber == null)
                            {
                                logService.AddNewLogEntry(PlmDbPartPRoc.Number + " X  Veri Yok  ile ilişkilendirilemedi.", null, "ilişkilendirilemedi", loggedInUsername);
                                failedUpdates.Add(PlmDbPartPRoc.Number + " X  Veri Yok  ile ilişkilendirilemedi.");
                                stokCodeProcessStatus.Add(new excelProcPartErrorManagement
                                {
                                    value = PlmDbPartPRoc.Number + " X  Veri Yok  ile ilişkilendirilemedi.",
                                    status = "ilişkilendirilemedi"
                                });

                            }
                            else
							{ 
                            logService.AddNewLogEntry(PlmDbPartPRoc.Number + " X " + PlmDbPartPRoc.PartNumber+ " ile ilişkilendirilemedi.", null, "ilişkilendirilemedi", loggedInUsername);
                                failedUpdates.Add(PlmDbPartPRoc.Number + " X" + PlmDbPartPRoc.PartNumber);
                                stokCodeProcessStatus.Add(new excelProcPartErrorManagement
                                {
                                    value = PlmDbPartPRoc.Number + " X " + PlmDbPartPRoc.PartNumber,
                                    status = "ilişkilendirilemedi"
                                });

                            }
                        }
                    }
					catch (Exception ex)
					{
                        if (PlmDbPartPRoc.PartNumber == "" || PlmDbPartPRoc.PartNumber == null)
                        {
                            logService.AddNewLogEntry(PlmDbPartPRoc.Number + " X  Veri Yok  ile ilişkilendirilemedi.", null, "ilişkilendirilemedi", loggedInUsername);
                            failedUpdates.Add(PlmDbPartPRoc.Number + " X  Veri Yok  ile ilişkilendirilemedi.");
                            stokCodeProcessStatus.Add(new excelProcPartErrorManagement
                            {
                                value = PlmDbPartPRoc.Number + " X  Veri Yok  ile ilişkilendirilemedi.",
                                status = "ilişkilendirilemedi"
                            });

                        }
                        else
                        {
                            logService.AddNewLogEntry(PlmDbPartPRoc.Number + " X" + PlmDbPartPRoc.PartNumber + " ile ilişkilendirilemedi.", null, "ilişkilendirilemedi", loggedInUsername);
							failedUpdates.Add(PlmDbPartPRoc.Number + " X" + PlmDbPartPRoc.PartNumber);
                            stokCodeProcessStatus.Add(new excelProcPartErrorManagement
                            {
                                value = PlmDbPartPRoc.Number + " X" + PlmDbPartPRoc.PartNumber,
                                status = "ilişkilendirilemedi"
                            });

                        }
                    }
				
                }
                ViewBag.exceldata = excelData.Tables[0];
                ViewBag.exceldataCount = excelData.Tables[0].Rows.Count;
                ViewBag.excelfile = excelFilePath;
                ViewBag.successfulUpdates = successfulUpdates;
                ViewBag.successfulUpdatesCount = successfulUpdates.Count;
                ViewBag.failedUpdates = failedUpdates;
                ViewBag.failedUpdatesCount = failedUpdates.Count;
                ViewBag.stokCodeProcessStatus = stokCodeProcessStatus;
                return View();
                //return Ok(new { status = true, message = "Data Sync Success" });
                //return Ok(RecordList);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "HATA!" + ex.Message;

                return View();
                //return RedirectToAction("Index");
            }
        }



        public IActionResult ExcelProc(IFormCollection data, string importType)
		{
			try
			{
				//string connectionString = _configuration.GetConnectionString("Plm");
				//string schema = _configuration["Catalog"];

				//using IDbConnection connection = new SqlConnection(connectionString);


				string excelFileData = data["data"].ToString(); // data verisini çek

				string[] keyValuePairs = excelFileData.Split('&'); // & işaretine göre bölelim

				string excelFileName = null;

				foreach (var pair in keyValuePairs)
				{
					string[] parts = pair.Split('=');
					if (parts.Length == 2 && parts[0] == "excel")
					{
						excelFileName = parts[1];
						break;
					}
				}

				var fileName = Directory.GetCurrentDirectory() + "\\wwwroot\\ExcelRelatePartsToDocumentFolder\\" + excelFileName;
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
				List<GenericObjectViewModel> RecordList = new List<GenericObjectViewModel>();

				foreach (DataRow excelRow in excelData.Tables[0].Rows)
				{
					var Number = excelRow[0].ToString();
					var Version = excelRow[1].ToString();
					foreach (var item in data["selected_headers[]"])
					{
						if (item != "0")
						{
							var epmCollection = item.Split("|");
							var excelColumnName = epmCollection[0];
							var plmIdeHierId = epmCollection[1];
							var definitionType = epmCollection[2];
							var idA2A2 = epmCollection[3];

							GenericObjectViewModel GenericRowObject = new GenericObjectViewModel();
							GenericRowObject.Number = Number;
							GenericRowObject.Version = Version;
							GenericRowObject.HierId = plmIdeHierId;
							GenericRowObject.DefinitionType = definitionType;
							GenericRowObject.idA2A2 = idA2A2;
							GenericRowObject.AttrValue = excelRow[excelColumnName.ToString()].ToString();
							RecordList.Add(GenericRowObject);
						}
					}

					// Şartları burada ekleyin
					var Anaparca = excelRow["Number"].ToString();
					var Alternatif = excelRow["ALTERNATIF"].ToString();
					var CIFTYON = excelRow["CIFTYON"].ToString();
					var Name = excelRow["NAME"].ToString();
					var eskiName = excelRow["ESKI_NAME"].ToString();
					var connectionString = _configuration.GetConnectionString("Plm");
					var catalogValue = _configuration["Catalog"];


					using (SqlConnection conn1Sel = new SqlConnection(connectionString))
					{
						conn1Sel.Open();
						var alternatifDeger2Sel = "";
						var idA3domainRef = "";
						var sqlQuery1Sel = $"SELECT name, idA2A2, idA3containerReference FROM {catalogValue}.WTPartMaster WHERE WTPartNumber = @Anaparca";
						var sqlQuery1SelAlternatif = $"SELECT idA2A2 FROM {catalogValue}.WTPartMaster WHERE WTPartNumber = @Alternatif";

						using (SqlCommand cmd1SelAlt = new SqlCommand(sqlQuery1SelAlternatif, conn1Sel))
						{
							cmd1SelAlt.Parameters.AddWithValue("@Anaparca", Anaparca);
							cmd1SelAlt.Parameters.AddWithValue("@Alternatif", Alternatif);

							using (SqlDataReader reader2SelAlt = cmd1SelAlt.ExecuteReader())
							{
								while (reader2SelAlt.Read())
								{
									alternatifDeger2Sel = reader2SelAlt["idA2A2"].ToString();
								}
							}
						}

						using (SqlCommand cmd1Sel = new SqlCommand(sqlQuery1Sel, conn1Sel))
						{
							cmd1Sel.Parameters.AddWithValue("@Anaparca", Anaparca);
							cmd1Sel.Parameters.AddWithValue("@Alternatif", Alternatif);

							using (SqlDataReader reader1Sel = cmd1Sel.ExecuteReader())
							{
								while (reader1Sel.Read())
								{
									var name = reader1Sel["name"].ToString();
									var idA2A2 = reader1Sel["idA2A2"].ToString();
									var idA3containerReference = reader1Sel["idA3containerReference"].ToString();

									using (SqlConnection conn1GetDomainRef = new SqlConnection(connectionString))
									{
										conn1GetDomainRef.Open();
										var domainRefQuery = $"SELECT idA3D2containerInfo FROM {catalogValue}.PDMLinkProduct WHERE idA2A2 = {idA3containerReference}";
										using (SqlCommand domainRefCmd = new SqlCommand(domainRefQuery, conn1GetDomainRef))
										{
											domainRefCmd.Parameters.AddWithValue("@idA2A2", idA2A2);

											using (SqlDataReader domainRefReader = domainRefCmd.ExecuteReader())
											{
												if (domainRefReader.Read())
												{
													idA3domainRef = domainRefReader["idA3D2containerInfo"].ToString();
												}
											}
										}
									}

									using (SqlConnection conn1Ins = new SqlConnection(connectionString))
									{
										conn1Ins.Open();
										var insertQuery1Ins = $"INSERT INTO {catalogValue}.WTPartAlternateLink (idA3A5, idA3B5, classnameA2A2, classnamekeydomainRef,idA3domainRef, inheritedDomain, replacementType, classnamekeyroleBObjectRef, classnamekeyroleAObjectRef, updateCountA2, markForDeleteA2, idA2A2, createStampA2, modifyStampA2, updateStampA2) VALUES (@idA3A5, @idA3B5, @classnameA2A2, @classnamekeydomainRef, @idA3domainRef, @inheritedDomain, @replacementType, @classnamekeyroleBObjectRef, @classnamekeyroleAObjectRef, @updateCountA2, @markForDeleteA2, @idA2A2, @createStampA2, @modifyStampA2, @updateStampA2)";
										var IdSeq = _plm2.Query(catalogValue + ".id_sequence").OrderByDesc("value").FirstOrDefault();

										var ID = Convert.ToInt64(Convert.ToInt64(IdSeq.value) + 100);

										using (SqlCommand insertCmd1Ins = new SqlCommand(insertQuery1Ins, conn1Ins))
										{

											insertCmd1Ins.Parameters.AddWithValue("@idA3A5", idA2A2);
											insertCmd1Ins.Parameters.AddWithValue("@idA3B5", idA2A2);
											insertCmd1Ins.Parameters.AddWithValue("@classnamekeydomainRef", "wt.admin.AdministrativeDomain");
											insertCmd1Ins.Parameters.AddWithValue("@idA3domainRef", idA3domainRef);
											insertCmd1Ins.Parameters.AddWithValue("@inheritedDomain", 0);
											insertCmd1Ins.Parameters.AddWithValue("@replacementType", "a");
											insertCmd1Ins.Parameters.AddWithValue("@classnamekeyroleAObjectRef", "wt.part.WTPartMaster");
											insertCmd1Ins.Parameters.AddWithValue("@classnamekeyroleBObjectRef", "wt.part.WTPartMaster");
											insertCmd1Ins.Parameters.AddWithValue("@classnameA2A2", "wt.part.WTPartAlternateLink");
											insertCmd1Ins.Parameters.AddWithValue("@updateCountA2", 1);
											insertCmd1Ins.Parameters.AddWithValue("@markForDeleteA2", 0);
											insertCmd1Ins.Parameters.AddWithValue("@idA2A2", ID);
											insertCmd1Ins.Parameters.AddWithValue("@createStampA2", DateTime.Now);
											insertCmd1Ins.Parameters.AddWithValue("@modifyStampA2", DateTime.Now);
											insertCmd1Ins.Parameters.AddWithValue("@updateStampA2", DateTime.Now);
											try
											{
												insertCmd1Ins.ExecuteNonQuery();
												_plm2.Query(catalogValue + ".id_sequence").Insert(new { dummy = "x" });
											}
											catch (SqlException ex)
											{
												TempData["ErrorMessage"] = "HATA!" + ex.Message;

											}

										}




									}
								}
							}
						}
					}

				}


				/* foreach (DataColumn excelHeader in excelData.Tables[0].Columns)
		            {
		                //smartData += excelRow[excelHeader.ColumnName.ToString()].ToString();
		            }
		        */
				return Ok(new { status = true, message = "Data Sync Success" });

			}
			catch (Exception ex)
			{
				TempData["ErrorMessage"] = "HATA!" + ex.Message;
				return RedirectToAction("Index");
			}
		}

	}
}
