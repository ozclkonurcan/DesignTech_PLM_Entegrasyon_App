﻿using Dapper;
using Designtech.Models;
using DesignTech_PLM_Entegrasyon_App.MVC.Dtos;
using DesignTech_PLM_Entegrasyon_App.MVC.Helper;
using DesignTech_PLM_Entegrasyon_App.MVC.ViewModels;
using ExcelDataReader;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Serilog;
using SqlKata.Execution;
using System.Data;
using System.Text.RegularExpressions;
using static DesignTech_PLM_Entegrasyon_App.MVC.Controllers.LogController;
using static DesignTech_PLM_Entegrasyon_App.MVC.Helper.LogService;

namespace DesignTech_PLM_Entegrasyon_App.MVC.Controllers.Modules.Excel
{
    public class PlmExcelIntegrateController : Controller
	{
		private readonly IConfiguration _configuration;
        public QueryFactory _plm2;
        public PlmExcelIntegrateController(QueryFactory queryFactory,IConfiguration configuration)
		{
			_configuration = configuration;
            _plm2 = new PlmDatabase(configuration).Connect();
        }



		public async Task<IActionResult> ExcelIntegrateIndex(string file)
		{
			try
			{

				var fileName = Directory.GetCurrentDirectory() + "\\wwwroot\\ExcelInformation\\" + file;
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

        public IActionResult ExcelProc(IFormCollection data)
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
              
                var fileName = Directory.GetCurrentDirectory() + "\\wwwroot\\ExcelInformation\\" + excelFileName;
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



        List<ExcelDetailResult> detailData = new List<ExcelDetailResult>();

        public async Task<JsonResult> ExcelFileControl(IFormCollection data,string[] HataListesi, string[] Hatasizlar,string excelFile)
	    {
            try
            {
                // HataListesi ve Hatasizlar JSON dizilerini çözümle
                var hataListesi = Newtonsoft.Json.JsonConvert.DeserializeObject<string[]>(HataListesi[0]);
                var hatasizlar = Newtonsoft.Json.JsonConvert.DeserializeObject<string[]>(Hatasizlar[0]);
                ExcelDetailResult2 rest = new ExcelDetailResult2();
                string logFileDataClear = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "errorFile", "errorFile.json");
                System.IO.File.WriteAllText(logFileDataClear, string.Empty);
                // Hatalı verileri loglama
                if (hataListesi != null && hataListesi.Length > 0)
                {
                    LogService logService = new LogService();
                    foreach (var hata in hataListesi)
                    {
                        logService.AddNewLogEntry(hata);
                        logService.ErrorFileManagement(hata, "errorFile.json");
                        //Log.Information(hata);
                    }
                    var excelLogEntries = logService.GetConvertedDataFromJson();
                    //List<ExcelDetailResult> dataList = new List<ExcelDetailResult>();

                    //foreach (var excel in excelLogEntries)
                    //{
                    //    ExcelDetailResult result = logService.GetExcelDetails(excelFile, excel.Satir, excel.Sutun, excel.Hata);
                    //    dataList.Add(result);
                    //}


                    //ViewBag.ExcelDetail = dataList.Distinct().ToList();
                    var resp = excelLogEntries.Distinct().ToList();

                    // İlk başlık ve veri listelerini oluşturun
                    rest.basliklar = new List<string>();
                    rest.veriler = new List<object>();
                    rest.hataNo = new List<string>();

                    foreach (var item in resp)
                    {
                        var result = logService.GetExcelDetails(item.ExcelDosya, item.Sutun, item.Hata);

                        // Başlıkları sadece bir kez ekleyin
                        if (rest.basliklar.Count == 0)
                        {
                            rest.basliklar = result.Basliklar;
                        }

                        // Verileri biriktirin
                        rest.veriler.AddRange(result.Veriler);
                        rest.hataNo.AddRange(new List<string> { item.Hata });

                    }





                }
                else
                {
                    //ExcelProc(data);
                }


                // JSON formatında verileri döndürün


                var successData = new { success = true, HataListesi = hataListesi, Hatasizlar = hatasizlar,veriler = rest.veriler, basliklar = rest.basliklar,hataNo = rest.hataNo};
			return Json(successData);
		}
		catch (Exception ex)
		{
			// Hata durumunda hata mesajını JSON formatında döndürün
			var errorData = new { success = false, message = ex.Message };
			return Json(errorData);
		}
	}

        public class ExcelDetailResult2
        {
            public List<string> basliklar { get; set; }
            public List<object> veriler { get; set; }
            public List<string> hataNo { get; set; }
        }





        public JsonResult ExcelIntegrateIndexControl()
		{
			Log.Information("");
			return Json("");
		}
	}
}
