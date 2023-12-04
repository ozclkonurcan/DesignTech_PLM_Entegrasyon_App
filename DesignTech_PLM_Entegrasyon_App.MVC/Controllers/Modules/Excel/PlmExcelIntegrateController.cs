using Dapper;
using Designtech.Models;
using DesignTech_PLM_Entegrasyon_App.MVC.Dtos;
using DesignTech_PLM_Entegrasyon_App.MVC.Helper;
using DesignTech_PLM_Entegrasyon_App.MVC.Models;
using DesignTech_PLM_Entegrasyon_App.MVC.ViewModels;
using ExcelDataReader;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Primitives;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Serilog;
using SqlKata.Execution;
using System.Collections.Generic;
using System.Data;
using System.Drawing.Imaging;
using System.Text.RegularExpressions;
using static DesignTech_PLM_Entegrasyon_App.MVC.Controllers.LogController;
using static DesignTech_PLM_Entegrasyon_App.MVC.Helper.LogService;

namespace DesignTech_PLM_Entegrasyon_App.MVC.Controllers.Modules.Excel
{
    [Authorize]
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

        //public IActionResult ExcelProc(IFormCollection data,string importType)
        //{
        //    try
        //    {
        //        //string connectionString = _configuration.GetConnectionString("Plm");
        //        //string schema = _configuration["Catalog"];

        //        //using IDbConnection connection = new SqlConnection(connectionString);


        //        string excelFileData = data["data"].ToString(); // data verisini çek

        //        string[] keyValuePairs = excelFileData.Split('&'); // & işaretine göre bölelim

        //        string excelFileName = null;

        //        foreach (var pair in keyValuePairs)
        //        {
        //            string[] parts = pair.Split('=');
        //            if (parts.Length == 2 && parts[0] == "excel")
        //            {
        //                excelFileName = parts[1];
        //                break;
        //            }
        //        }
              
        //        var fileName = Directory.GetCurrentDirectory() + "\\wwwroot\\ExcelInformation\\" + excelFileName;
        //        DataSet excelData;
        //        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        //        using (var stream = System.IO.File.Open(fileName, FileMode.Open, FileAccess.Read))
        //        {
        //            using (IExcelDataReader reader = ExcelReaderFactory.CreateReader(stream))
        //            {
        //                excelData = reader.AsDataSet(new ExcelDataSetConfiguration()
        //                {
        //                    ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
        //                    {
        //                        UseHeaderRow = true
        //                    }
        //                });
        //            }
        //        }
        //        List<GenericObjectViewModel> RecordList = new List<GenericObjectViewModel>();

        //        foreach (DataRow excelRow in excelData.Tables[0].Rows)
        //        {
        //            var Number = excelRow[0].ToString();
        //            var Version = excelRow[1].ToString();
        //            foreach (var item in data["selected_headers[]"])
        //            {
        //                if (item != "0")
        //                {
        //                    var epmCollection = item.Split("|");
        //                    var excelColumnName = epmCollection[0];
        //                    var plmIdeHierId = epmCollection[1];
        //                    var definitionType = epmCollection[2];
        //                    var idA2A2 = epmCollection[3];

        //                    GenericObjectViewModel GenericRowObject = new GenericObjectViewModel();
        //                    GenericRowObject.Number = Number;
        //                    GenericRowObject.Version = Version;
        //                    GenericRowObject.HierId = plmIdeHierId;
        //                    GenericRowObject.DefinitionType = definitionType;
        //                    GenericRowObject.idA2A2 = idA2A2;
        //                    GenericRowObject.AttrValue = excelRow[excelColumnName.ToString()].ToString();
        //                    RecordList.Add(GenericRowObject);
        //                }
        //            }

        //            // Şartları burada ekleyin
        //            var Anaparca = excelRow["Number"].ToString();
        //            var Alternatif = excelRow["ALTERNATIF"].ToString();
        //            var CIFTYON = excelRow["CIFTYON"].ToString();
        //            var Name = excelRow["NAME"].ToString();
        //            var eskiName = excelRow["ESKI_NAME"].ToString();
        //            var connectionString = _configuration.GetConnectionString("Plm");
        //            var catalogValue = _configuration["Catalog"];

                 
        //                using (SqlConnection conn1Sel = new SqlConnection(connectionString))
        //                {
        //                    conn1Sel.Open();
        //                    var alternatifDeger2Sel = "";
        //                    var idA3domainRef = "";
        //                    var sqlQuery1Sel = $"SELECT name, idA2A2, idA3containerReference FROM {catalogValue}.WTPartMaster WHERE WTPartNumber = @Anaparca";
        //                    var sqlQuery1SelAlternatif = $"SELECT idA2A2 FROM {catalogValue}.WTPartMaster WHERE WTPartNumber = @Alternatif";

        //                    using (SqlCommand cmd1SelAlt = new SqlCommand(sqlQuery1SelAlternatif, conn1Sel))
        //                    {
        //                        cmd1SelAlt.Parameters.AddWithValue("@Anaparca", Anaparca);
        //                        cmd1SelAlt.Parameters.AddWithValue("@Alternatif", Alternatif);

        //                        using (SqlDataReader reader2SelAlt = cmd1SelAlt.ExecuteReader())
        //                        {
        //                            while (reader2SelAlt.Read())
        //                            {
        //                                alternatifDeger2Sel = reader2SelAlt["idA2A2"].ToString();
        //                            }
        //                        }
        //                    }

        //                    using (SqlCommand cmd1Sel = new SqlCommand(sqlQuery1Sel, conn1Sel))
        //                    {
        //                        cmd1Sel.Parameters.AddWithValue("@Anaparca", Anaparca);
        //                        cmd1Sel.Parameters.AddWithValue("@Alternatif", Alternatif);

        //                        using (SqlDataReader reader1Sel = cmd1Sel.ExecuteReader())
        //                        {
        //                            while (reader1Sel.Read())
        //                            {
        //                                var name = reader1Sel["name"].ToString();
        //                                var idA2A2 = reader1Sel["idA2A2"].ToString();
        //                                var idA3containerReference = reader1Sel["idA3containerReference"].ToString();

        //                                using (SqlConnection conn1GetDomainRef = new SqlConnection(connectionString))
        //                                {
        //                                    conn1GetDomainRef.Open();
        //                                    var domainRefQuery = $"SELECT idA3D2containerInfo FROM {catalogValue}.PDMLinkProduct WHERE idA2A2 = {idA3containerReference}";
        //                                    using (SqlCommand domainRefCmd = new SqlCommand(domainRefQuery, conn1GetDomainRef))
        //                                    {
        //                                        domainRefCmd.Parameters.AddWithValue("@idA2A2", idA2A2);

        //                                        using (SqlDataReader domainRefReader = domainRefCmd.ExecuteReader())
        //                                        {
        //                                            if (domainRefReader.Read())
        //                                            {
        //                                                idA3domainRef = domainRefReader["idA3D2containerInfo"].ToString();
        //                                            }
        //                                        }
        //                                    }
        //                                }

        //                                using (SqlConnection conn1Ins = new SqlConnection(connectionString))
        //                                {
        //                                    conn1Ins.Open();
        //                                    var insertQuery1Ins = $"INSERT INTO {catalogValue}.WTPartAlternateLink (idA3A5, idA3B5, classnameA2A2, classnamekeydomainRef,idA3domainRef, inheritedDomain, replacementType, classnamekeyroleBObjectRef, classnamekeyroleAObjectRef, updateCountA2, markForDeleteA2, idA2A2, createStampA2, modifyStampA2, updateStampA2) VALUES (@idA3A5, @idA3B5, @classnameA2A2, @classnamekeydomainRef, @idA3domainRef, @inheritedDomain, @replacementType, @classnamekeyroleBObjectRef, @classnamekeyroleAObjectRef, @updateCountA2, @markForDeleteA2, @idA2A2, @createStampA2, @modifyStampA2, @updateStampA2)";
        //                                    var IdSeq = _plm2.Query(catalogValue + ".id_sequence").OrderByDesc("value").FirstOrDefault();

        //                                    var ID = Convert.ToInt64(Convert.ToInt64(IdSeq.value) + 100);
                                        
        //                                        using (SqlCommand insertCmd1Ins = new SqlCommand(insertQuery1Ins, conn1Ins))
        //                                        {

        //                                            insertCmd1Ins.Parameters.AddWithValue("@idA3A5", idA2A2);
        //                                            insertCmd1Ins.Parameters.AddWithValue("@idA3B5", idA2A2);
        //                                            insertCmd1Ins.Parameters.AddWithValue("@classnamekeydomainRef", "wt.admin.AdministrativeDomain");
        //                                            insertCmd1Ins.Parameters.AddWithValue("@idA3domainRef", idA3domainRef);
        //                                            insertCmd1Ins.Parameters.AddWithValue("@inheritedDomain", 0);
        //                                            insertCmd1Ins.Parameters.AddWithValue("@replacementType", "a");
        //                                            insertCmd1Ins.Parameters.AddWithValue("@classnamekeyroleAObjectRef", "wt.part.WTPartMaster");
        //                                            insertCmd1Ins.Parameters.AddWithValue("@classnamekeyroleBObjectRef", "wt.part.WTPartMaster");
        //                                            insertCmd1Ins.Parameters.AddWithValue("@classnameA2A2", "wt.part.WTPartAlternateLink");
        //                                            insertCmd1Ins.Parameters.AddWithValue("@updateCountA2", 1);
        //                                            insertCmd1Ins.Parameters.AddWithValue("@markForDeleteA2", 0);
        //                                            insertCmd1Ins.Parameters.AddWithValue("@idA2A2", ID);
        //                                            insertCmd1Ins.Parameters.AddWithValue("@createStampA2", DateTime.Now);
        //                                            insertCmd1Ins.Parameters.AddWithValue("@modifyStampA2", DateTime.Now);
        //                                            insertCmd1Ins.Parameters.AddWithValue("@updateStampA2", DateTime.Now);
        //                                            try
        //                                            {
        //                                                insertCmd1Ins.ExecuteNonQuery();
        //                                                _plm2.Query(catalogValue + ".id_sequence").Insert(new { dummy = "x" });
        //                                            }
        //                                            catch (SqlException ex)
        //                                            {
        //                                                TempData["ErrorMessage"] = "HATA!" + ex.Message;

        //                                            }

        //                                        }




        //                                }
        //                            }
        //                        }
        //                    }
        //                }

        //        }


        //        /* foreach (DataColumn excelHeader in excelData.Tables[0].Columns)
        //            {
        //                //smartData += excelRow[excelHeader.ColumnName.ToString()].ToString();
        //            }
        //        */
        //        return Ok(new { status = true, message = "Data Sync Success" });

        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["ErrorMessage"] = "HATA!" + ex.Message;
        //        return RedirectToAction("Index");
        //    }
        //}



        public IActionResult ExcelProc([FromForm] IFormCollection data,string importType)
        {
            try
            {


                string excelFileData = data["data"].ToString(); // data verisini çek

                string[] keyValuePairs = excelFileData.Split('&'); // & işaretine göre bölelim

                string excelFileName = null;

                var selectedHeaders = new List<string>();
               
                selectedHeaders.Add("Number");
                selectedHeaders.Add("CIFTYON");
                selectedHeaders.Add("ALTERNATIF");
                selectedHeaders.Add("ESKI_NAME");
                selectedHeaders.Add("NAME");
                foreach (var pair in keyValuePairs)
                {
                    if (pair.StartsWith("selected_headers%5B%5D=") && !pair.StartsWith("selected_headers%5B%5D=0"))
                    {
                        var headerParts = pair.Split('=');
                        var firstPart = headerParts[1].Split('%')[0];

                        selectedHeaders.Add(firstPart);
                    
                    }
                }

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


				DataTable filteredDataTable = new DataTable();
				DataTable filteredDataTable2 = new DataTable();

				if (excelData.Tables.Contains(importType))
                {
					DataTable originalDataTable = excelData.Tables[importType]; // Burada 0, Excel dosyasındaki ilk sayfayı temsil eder

					foreach (string header in selectedHeaders)
                    {
                        if (originalDataTable.Columns.Contains(header))
                        {
                            filteredDataTable.Columns.Add(header);
                        }
                    }


					foreach (DataRow row in originalDataTable.Rows)
					{
						DataRow newRow = filteredDataTable.NewRow();

						foreach (string header in selectedHeaders)
						{
							if (originalDataTable.Columns.Contains(header))
							{
								newRow[header] = row[header];
							}
						}

						filteredDataTable.Rows.Add(newRow);
					}


				}

				//if (excelData.Tables.Contains("WTPart"))
    //            {
				//	DataTable originalDataTable2 = excelData.Tables["WTPart"];
				//	// Burada 0, Excel dosyasındaki ilk sayfayı temsil eder

				//	foreach (string header in selectedHeaders)
				//	{

				//		if (originalDataTable2.Columns.Contains(header))
				//		{
				//			filteredDataTable.Columns.Add(header);
				//		}
				//	}


				//	foreach (DataRow row in originalDataTable2.Rows)
				//	{
				//		DataRow newRow = filteredDataTable.NewRow();

				//		foreach (string header in selectedHeaders)
				//		{
				//			if (originalDataTable2.Columns.Contains(header))
				//			{
				//				newRow[header] = row[header];
				//			}
				//		}

				//		filteredDataTable.Rows.Add(newRow);
				//	}




				//	// Filtrelenmiş verileri kullanarak işlemleri gerçekleştir
				//	// Örneğin, ViewBag kullanarak view'e veriyi aktarabilirsiniz
				//}



				//ViewBag.FilteredData = filteredDataTable2;
				ViewBag.FilteredData = filteredDataTable;

				// Excel'den okunan tüm başlıklar

				List<GenericObjectViewModel> RecordList = new List<GenericObjectViewModel>();



				var catalogValue = _configuration["Catalog"];

				foreach (DataRow excelRow in filteredDataTable.Rows)
                {
                   
                   

                    var Number = excelRow["Number"].ToString();
                    // Şartları burada ekleyin
                    var Anaparca = excelRow["Number"].ToString();
					var Name = "";
             

                    if (excelRow.Table.Columns.Contains("ALTERNATIF"))
                    {
                        var Alternatif = excelRow["ALTERNATIF"].ToString();
                    }

                    if (excelRow.Table.Columns.Contains("CIFTYON"))
                    {
                        var CIFTYON = excelRow["CIFTYON"].ToString();
                    }
                    if (excelRow.Table.Columns.Contains("NAME"))
                    { 
                         Name = excelRow["NAME"].ToString();
                    }
			

					var connectionString = _configuration.GetConnectionString("Plm");

                    var seenMaterials = new HashSet<string>();

                    foreach (var item in data["colHeadFullDataList[]"])
                    {
                        if (item != "0" && item != "")
                        {
                            var epmCollection = item.Split("|");
                            var excelColumnName = epmCollection[0];
                            var plmIdeHierId = epmCollection[1];
                            var definitionType = epmCollection[2];
                            var idA2A2 = epmCollection[3];

                            // Malzeme kontrolü
                            var materialKey = $"{excelColumnName}|{plmIdeHierId}|{definitionType}|{idA2A2}";
                            if (seenMaterials.Contains(materialKey))
                            {
                                // Bu malzeme daha önce işlendi, bu durumu atla
                                continue;
                            }

                            seenMaterials.Add(materialKey); // Malzeme kümesine ekle

                            GenericObjectViewModel GenericRowObject = new GenericObjectViewModel();
                            GenericRowObject.Number = Number;
                            GenericRowObject.Name = Name;
                            GenericRowObject.HierId = plmIdeHierId;
                            GenericRowObject.DefinitionType = definitionType;
                            GenericRowObject.idA2A2 = idA2A2;
                            GenericRowObject.AttrValue = excelRow[excelColumnName.ToString()].ToString();
                            RecordList.Add(GenericRowObject);
                        }
                    }


                    //if (CIFTYON == "2")
                    //{

                    //    if(importType == "WTPart")
                    //    {

                    //    using (SqlConnection conn2Sel = new SqlConnection(connectionString))
                    //    {
                    //        string alternatifDeger2Sel = "";
                    //        conn2Sel.Open();
                    //        var sqlQuery2Sel = $"SELECT name, idA2A2, idA3containerReference FROM {catalogValue}.WTPartMaster WHERE WTPartNumber = @Anaparca";



                    //        var sqlQuery2SelAlternatif = $"SELECT idA2A2 FROM {catalogValue}.WTPartMaster WHERE WTPartNumber = @Alternatif";

                    //        using (SqlCommand cmd2SelAlt = new SqlCommand(sqlQuery2SelAlternatif, conn2Sel))
                    //        {
                    //            cmd2SelAlt.Parameters.AddWithValue("@Anaparca", Anaparca);
                    //            cmd2SelAlt.Parameters.AddWithValue("@Alternatif", Alternatif);

                    //            using (SqlDataReader reader2SelAlt = cmd2SelAlt.ExecuteReader())
                    //            {
                    //                while (reader2SelAlt.Read())
                    //                {
                    //                    alternatifDeger2Sel = reader2SelAlt["idA2A2"].ToString();


                    //                }
                    //            }
                    //        }

                    //        using (SqlCommand cmd2Sel = new SqlCommand(sqlQuery2Sel, conn2Sel))
                    //        {
                    //            cmd2Sel.Parameters.AddWithValue("@Anaparca", Anaparca);
                    //            cmd2Sel.Parameters.AddWithValue("@Alternatif", Alternatif);

                    //            using (SqlDataReader reader2Sel = cmd2Sel.ExecuteReader())
                    //            {
                    //                while (reader2Sel.Read())
                    //                {
                    //                    var name = reader2Sel["name"].ToString();
                    //                    var idA2A2 = reader2Sel["idA2A2"].ToString();
                    //                    var idA3containerReference = reader2Sel["idA3containerReference"].ToString();
                    //                    var idA3domainRef = "";

                    //                    using (SqlConnection conn1GetDomainRef = new SqlConnection(connectionString))
                    //                    {
                    //                        conn1GetDomainRef.Open();
                    //                        var domainRefQuery = $"SELECT idA3D2containerInfo FROM {catalogValue}.PDMLinkProduct WHERE idA2A2 = {idA3containerReference}";
                    //                        using (SqlCommand domainRefCmd = new SqlCommand(domainRefQuery, conn1GetDomainRef))
                    //                        {
                    //                            domainRefCmd.Parameters.AddWithValue("@idA2A2", idA2A2);

                    //                            using (SqlDataReader domainRefReader = domainRefCmd.ExecuteReader())
                    //                            {
                    //                                if (domainRefReader.Read())
                    //                                {
                    //                                    idA3domainRef = domainRefReader["idA3D2containerInfo"].ToString();
                    //                                }
                    //                            }
                    //                        }
                    //                    }

                    //                    // CIFTYON 2 - Insert
                    //                    using (SqlConnection conn2Ins = new SqlConnection(connectionString))
                    //                    {
                    //                        conn2Ins.Open();
                    //                        var insertQuery2Ins = $"INSERT INTO {catalogValue}.WTPartAlternateLink (idA3A5, idA3B5, classnameA2A2, classnamekeydomainRef,idA3domainRef, inheritedDomain, replacementType, classnamekeyroleBObjectRef, classnamekeyroleAObjectRef, updateCountA2, markForDeleteA2, idA2A2, createStampA2, modifyStampA2, updateStampA2) VALUES (@idA3A5, @idA3B5, @classnameA2A2, @classnamekeydomainRef, @idA3domainRef, @inheritedDomain, @replacementType, @classnamekeyroleBObjectRef, @classnamekeyroleAObjectRef, @updateCountA2, @markForDeleteA2, @idA2A2, @createStampA2, @modifyStampA2, @updateStampA2)";
                    //                        // İlk ekleme işlemi
                    //                        using (SqlCommand insertCmd2Ins = new SqlCommand(insertQuery2Ins, conn2Ins))
                    //                        {
                    //                            var IdSeq = _plm2.Query(catalogValue + ".id_sequence").OrderByDesc("value").FirstOrDefault();
                    //                            var ID = Convert.ToInt64(Convert.ToInt64(IdSeq.value) + 100);

                    //                            insertCmd2Ins.Parameters.AddWithValue("@idA3A5", idA2A2);
                    //                            insertCmd2Ins.Parameters.AddWithValue("@idA3B5", alternatifDeger2Sel);
                    //                            insertCmd2Ins.Parameters.AddWithValue("@classnamekeydomainRef", "wt.admin.AdministrativeDomain");
                    //                            insertCmd2Ins.Parameters.AddWithValue("@idA3domainRef", idA3domainRef);
                    //                            insertCmd2Ins.Parameters.AddWithValue("@inheritedDomain", 0);
                    //                            insertCmd2Ins.Parameters.AddWithValue("@replacementType", "a");
                    //                            insertCmd2Ins.Parameters.AddWithValue("@classnamekeyroleAObjectRef", "wt.part.WTPartMaster");
                    //                            insertCmd2Ins.Parameters.AddWithValue("@classnamekeyroleBObjectRef", "wt.part.WTPartMaster");
                    //                            insertCmd2Ins.Parameters.AddWithValue("@classnameA2A2", "wt.part.WTPartAlternateLink");
                    //                            insertCmd2Ins.Parameters.AddWithValue("@updateCountA2", 1);
                    //                            insertCmd2Ins.Parameters.AddWithValue("@markForDeleteA2", 0);
                    //                            insertCmd2Ins.Parameters.AddWithValue("@idA2A2", ID);
                    //                            insertCmd2Ins.Parameters.AddWithValue("@createStampA2", DateTime.Now);
                    //                            insertCmd2Ins.Parameters.AddWithValue("@modifyStampA2", DateTime.Now);
                    //                            insertCmd2Ins.Parameters.AddWithValue("@updateStampA2", DateTime.Now);
                    //                            try
                    //                            {
                    //                                insertCmd2Ins.ExecuteNonQuery();
                    //                                _plm2.Query(catalogValue + ".id_sequence").Insert(new { dummy = "x" });
                    //                            }
                    //                            catch (SqlException ex)
                    //                            {
                    //                                TempData["ErrorMessage"] = "HATA!" + ex.Message;

                    //                            }


                    //                            var insertQuery3Ins = $"INSERT INTO {catalogValue}.WTPartAlternateHistory (eventType,principal,replacementTypeAfter,replacementTypeBefore,classnamekeyroleAObjectRef,idA3A5,classnamekeyroleBObjectRef,idA3B5,createStampA2,markForDeleteA2,modifyStampA2,classnameA2A2,idA2A2,updateCountA2,updateStampA2) VALUES (@eventType,@principal,@replacementTypeAfter,@replacementTypeBefore,@classnamekeyroleAObjectRef,@idA3A5,@classnamekeyroleBObjectRef,@idA3B5,@createStampA2,@markForDeleteA2,@modifyStampA2,@classnameA2A2,@idA2A2,@updateCountA2,@updateStampA2)";


                    //                            using (SqlCommand insertCmd3Ins = new SqlCommand(insertQuery3Ins, conn2Ins))
                    //                            {

                    //                                insertCmd3Ins.Parameters.AddWithValue("@eventType", "Add");
                    //                                insertCmd3Ins.Parameters.AddWithValue("@principal", 12);
                    //                                insertCmd3Ins.Parameters.AddWithValue("@replacementTypeAfter", "a");
                    //                                insertCmd3Ins.Parameters.AddWithValue("@replacementTypeBefore", "");
                    //                                insertCmd3Ins.Parameters.AddWithValue("@classnamekeyroleAObjectRef", "wt.part.WTPartMaster");
                    //                                insertCmd3Ins.Parameters.AddWithValue("@idA3A5", idA2A2);
                    //                                insertCmd3Ins.Parameters.AddWithValue("@classnamekeyroleBObjectRef", "wt.part.WTPartMaster");
                    //                                insertCmd3Ins.Parameters.AddWithValue("@idA3B5", alternatifDeger2Sel);
                    //                                insertCmd3Ins.Parameters.AddWithValue("@createStampA2", DateTime.Now);
                    //                                insertCmd3Ins.Parameters.AddWithValue("@markForDeleteA2", 0);
                    //                                insertCmd3Ins.Parameters.AddWithValue("@modifyStampA2", DateTime.Now);
                    //                                insertCmd3Ins.Parameters.AddWithValue("@classnameA2A2", "wt.part.WTPartAlternateHistory");
                    //                                insertCmd3Ins.Parameters.AddWithValue("@idA2A2", ID);
                    //                                insertCmd3Ins.Parameters.AddWithValue("@updateCountA2", 1);
                    //                                insertCmd3Ins.Parameters.AddWithValue("@updateStampA2", DateTime.Now);

                    //                                try
                    //                                {
                    //                                    insertCmd3Ins.ExecuteNonQuery();
                    //                                }
                    //                                catch (SqlException ex)
                    //                                {
                    //                                    TempData["ErrorMessage"] = "HATA!" + ex.Message;

                    //                                }


                    //                            }



                    //                        }

                    //                        // İkinci ekleme işlemi
                    //                        using (SqlCommand insertCmd2Ins2 = new SqlCommand(insertQuery2Ins, conn2Ins))
                    //                        {
                    //                            var IdSeq = _plm2.Query(catalogValue + ".id_sequence").OrderByDesc("value").FirstOrDefault();
                    //                            var ID = Convert.ToInt64(Convert.ToInt64(IdSeq.value) + 100);

                    //                            insertCmd2Ins2.Parameters.AddWithValue("@idA3A5", alternatifDeger2Sel);
                    //                            insertCmd2Ins2.Parameters.AddWithValue("@idA3B5", idA2A2);
                    //                            insertCmd2Ins2.Parameters.AddWithValue("@classnamekeydomainRef", "wt.admin.AdministrativeDomain");
                    //                            insertCmd2Ins2.Parameters.AddWithValue("@idA3domainRef", idA3domainRef);
                    //                            insertCmd2Ins2.Parameters.AddWithValue("@inheritedDomain", 0);
                    //                            insertCmd2Ins2.Parameters.AddWithValue("@replacementType", "a");
                    //                            insertCmd2Ins2.Parameters.AddWithValue("@classnamekeyroleAObjectRef", "wt.part.WTPartMaster");
                    //                            insertCmd2Ins2.Parameters.AddWithValue("@classnamekeyroleBObjectRef", "wt.part.WTPartMaster");
                    //                            insertCmd2Ins2.Parameters.AddWithValue("@classnameA2A2", "wt.part.WTPartAlternateLink");
                    //                            insertCmd2Ins2.Parameters.AddWithValue("@updateCountA2", 1);
                    //                            insertCmd2Ins2.Parameters.AddWithValue("@markForDeleteA2", 0);
                    //                            insertCmd2Ins2.Parameters.AddWithValue("@idA2A2", ID);
                    //                            insertCmd2Ins2.Parameters.AddWithValue("@createStampA2", DateTime.Now);
                    //                            insertCmd2Ins2.Parameters.AddWithValue("@modifyStampA2", DateTime.Now);
                    //                            insertCmd2Ins2.Parameters.AddWithValue("@updateStampA2", DateTime.Now);
                    //                            try
                    //                            {
                    //                                insertCmd2Ins2.ExecuteNonQuery();
                    //                                _plm2.Query(catalogValue + ".id_sequence").Insert(new { dummy = "x" });
                    //                            }
                    //                            catch (SqlException ex)
                    //                            {
                    //                                TempData["ErrorMessage"] = "HATA!" + ex.Message;
                    //                                throw;
                    //                            }


                    //                            var insertQuery3Ins = $"INSERT INTO {catalogValue}.WTPartAlternateHistory (eventType,principal,replacementTypeAfter,replacementTypeBefore,classnamekeyroleAObjectRef,idA3A5,classnamekeyroleBObjectRef,idA3B5,createStampA2,markForDeleteA2,modifyStampA2,classnameA2A2,idA2A2,updateCountA2,updateStampA2) VALUES (@eventType,@principal,@replacementTypeAfter,@replacementTypeBefore,@classnamekeyroleAObjectRef,@idA3A5,@classnamekeyroleBObjectRef,@idA3B5,@createStampA2,@markForDeleteA2,@modifyStampA2,@classnameA2A2,@idA2A2,@updateCountA2,@updateStampA2)";


                    //                            using (SqlCommand insertCmd3Ins = new SqlCommand(insertQuery3Ins, conn2Ins))
                    //                            {

                    //                                insertCmd3Ins.Parameters.AddWithValue("@eventType", "Add");
                    //                                insertCmd3Ins.Parameters.AddWithValue("@principal", 12);
                    //                                insertCmd3Ins.Parameters.AddWithValue("@replacementTypeAfter", "a");
                    //                                insertCmd3Ins.Parameters.AddWithValue("@replacementTypeBefore", "");
                    //                                insertCmd3Ins.Parameters.AddWithValue("@classnamekeyroleAObjectRef", "wt.part.WTPartMaster");
                    //                                insertCmd3Ins.Parameters.AddWithValue("@idA3A5", alternatifDeger2Sel);
                    //                                insertCmd3Ins.Parameters.AddWithValue("@classnamekeyroleBObjectRef", "wt.part.WTPartMaster");
                    //                                insertCmd3Ins.Parameters.AddWithValue("@idA3B5", idA2A2);
                    //                                insertCmd3Ins.Parameters.AddWithValue("@createStampA2", DateTime.Now);
                    //                                insertCmd3Ins.Parameters.AddWithValue("@markForDeleteA2", 0);
                    //                                insertCmd3Ins.Parameters.AddWithValue("@modifyStampA2", DateTime.Now);
                    //                                insertCmd3Ins.Parameters.AddWithValue("@classnameA2A2", "wt.part.WTPartAlternateHistory");
                    //                                insertCmd3Ins.Parameters.AddWithValue("@idA2A2", ID);
                    //                                insertCmd3Ins.Parameters.AddWithValue("@updateCountA2", 1);
                    //                                insertCmd3Ins.Parameters.AddWithValue("@updateStampA2", DateTime.Now);

                    //                                try
                    //                                {
                    //                                    insertCmd3Ins.ExecuteNonQuery();
                    //                                }
                    //                                catch (SqlException ex)
                    //                                {
                    //                                    TempData["ErrorMessage"] = "HATA!" + ex.Message;

                    //                                }

                    //                            }


                    //                        }



                    //                    }
                    //                    break;
                    //                }
                    //            }
                    //        }
                    //    }


                    //    }

                    //    if(importType == "EPM")
                    //    {

                    //    }



                    //    //var insert = _plm2.Query(catalogValue + ".WTPartAlternateLink").Insert(result);
                    //    // CIFTYON 2 ise ve Alternatif değeri varsa ilgili işlemi yapın
                    //    // Örnek olarak yeni bir tabloya post edin veya başka bir işlem yapın.
                    //}
                    //else if (CIFTYON == "1")
                    //{

                    //    if(importType == "WTPart")
                    //    {

                    //    using (SqlConnection conn1Sel = new SqlConnection(connectionString))
                    //    {
                    //        conn1Sel.Open();
                    //        var alternatifDeger2Sel = "";
                    //        var idA3domainRef = "";
                    //        var sqlQuery1Sel = $"SELECT name, idA2A2, idA3containerReference FROM {catalogValue}.WTPartMaster WHERE WTPartNumber = @Anaparca";
                    //        var sqlQuery1SelAlternatif = $"SELECT idA2A2 FROM {catalogValue}.WTPartMaster WHERE WTPartNumber = @Alternatif";

                    //        using (SqlCommand cmd1SelAlt = new SqlCommand(sqlQuery1SelAlternatif, conn1Sel))
                    //        {
                    //            cmd1SelAlt.Parameters.AddWithValue("@Anaparca", Anaparca);
                    //            cmd1SelAlt.Parameters.AddWithValue("@Alternatif", Alternatif);

                    //            using (SqlDataReader reader2SelAlt = cmd1SelAlt.ExecuteReader())
                    //            {
                    //                while (reader2SelAlt.Read())
                    //                {
                    //                    alternatifDeger2Sel = reader2SelAlt["idA2A2"].ToString();
                    //                }
                    //            }
                    //        }

                    //        using (SqlCommand cmd1Sel = new SqlCommand(sqlQuery1Sel, conn1Sel))
                    //        {
                    //            cmd1Sel.Parameters.AddWithValue("@Anaparca", Anaparca);
                    //            cmd1Sel.Parameters.AddWithValue("@Alternatif", Alternatif);

                    //            using (SqlDataReader reader1Sel = cmd1Sel.ExecuteReader())
                    //            {
                    //                while (reader1Sel.Read())
                    //                {
                    //                    var name = reader1Sel["name"].ToString();
                    //                    var idA2A2 = reader1Sel["idA2A2"].ToString();
                    //                    var idA3containerReference = reader1Sel["idA3containerReference"].ToString();

                    //                    using (SqlConnection conn1GetDomainRef = new SqlConnection(connectionString))
                    //                    {
                    //                        conn1GetDomainRef.Open();
                    //                        var domainRefQuery = $"SELECT idA3D2containerInfo FROM {catalogValue}.PDMLinkProduct WHERE idA2A2 = {idA3containerReference}";
                    //                        using (SqlCommand domainRefCmd = new SqlCommand(domainRefQuery, conn1GetDomainRef))
                    //                        {
                    //                            domainRefCmd.Parameters.AddWithValue("@idA2A2", idA2A2);

                    //                            using (SqlDataReader domainRefReader = domainRefCmd.ExecuteReader())
                    //                            {
                    //                                if (domainRefReader.Read())
                    //                                {
                    //                                    idA3domainRef = domainRefReader["idA3D2containerInfo"].ToString();
                    //                                }
                    //                            }
                    //                        }
                    //                    }

                    //                    using (SqlConnection conn1Ins = new SqlConnection(connectionString))
                    //                    {
                    //                        conn1Ins.Open();
                    //                        var insertQuery1Ins = $"INSERT INTO {catalogValue}.WTPartAlternateLink (idA3A5, idA3B5, classnameA2A2, classnamekeydomainRef,idA3domainRef, inheritedDomain, replacementType, classnamekeyroleBObjectRef, classnamekeyroleAObjectRef, updateCountA2, markForDeleteA2, idA2A2, createStampA2, modifyStampA2, updateStampA2) VALUES (@idA3A5, @idA3B5, @classnameA2A2, @classnamekeydomainRef, @idA3domainRef, @inheritedDomain, @replacementType, @classnamekeyroleBObjectRef, @classnamekeyroleAObjectRef, @updateCountA2, @markForDeleteA2, @idA2A2, @createStampA2, @modifyStampA2, @updateStampA2)";
                    //                        var IdSeq = _plm2.Query(catalogValue + ".id_sequence").OrderByDesc("value").FirstOrDefault();

                    //                        var ID = Convert.ToInt64(Convert.ToInt64(IdSeq.value) + 100);
                    //                        try
                    //                        {

                    //                            using (SqlCommand insertCmd1Ins = new SqlCommand(insertQuery1Ins, conn1Ins))
                    //                            {

                    //                                insertCmd1Ins.Parameters.AddWithValue("@idA3A5", idA2A2);
                    //                                insertCmd1Ins.Parameters.AddWithValue("@idA3B5", alternatifDeger2Sel);
                    //                                insertCmd1Ins.Parameters.AddWithValue("@classnamekeydomainRef", "wt.admin.AdministrativeDomain");
                    //                                insertCmd1Ins.Parameters.AddWithValue("@idA3domainRef", idA3domainRef);
                    //                                insertCmd1Ins.Parameters.AddWithValue("@inheritedDomain", 0);
                    //                                insertCmd1Ins.Parameters.AddWithValue("@replacementType", "a");
                    //                                insertCmd1Ins.Parameters.AddWithValue("@classnamekeyroleAObjectRef", "wt.part.WTPartMaster");
                    //                                insertCmd1Ins.Parameters.AddWithValue("@classnamekeyroleBObjectRef", "wt.part.WTPartMaster");
                    //                                insertCmd1Ins.Parameters.AddWithValue("@classnameA2A2", "wt.part.WTPartAlternateLink");
                    //                                insertCmd1Ins.Parameters.AddWithValue("@updateCountA2", 1);
                    //                                insertCmd1Ins.Parameters.AddWithValue("@markForDeleteA2", 0);
                    //                                insertCmd1Ins.Parameters.AddWithValue("@idA2A2", ID);
                    //                                insertCmd1Ins.Parameters.AddWithValue("@createStampA2", DateTime.Now);
                    //                                insertCmd1Ins.Parameters.AddWithValue("@modifyStampA2", DateTime.Now);
                    //                                insertCmd1Ins.Parameters.AddWithValue("@updateStampA2", DateTime.Now);
                    //                                try
                    //                                {
                    //                                    insertCmd1Ins.ExecuteNonQuery();
                    //                                    _plm2.Query(catalogValue + ".id_sequence").Insert(new { dummy = "x" });
                    //                                }
                    //                                catch (SqlException ex)
                    //                                {
                    //                                    TempData["ErrorMessage"] = "HATA!" + ex.Message;

                    //                                }

                    //                            }



                    //                            var insertQuery2Ins = $"INSERT INTO {catalogValue}.WTPartAlternateHistory (eventType,principal,replacementTypeAfter,replacementTypeBefore,classnamekeyroleAObjectRef,idA3A5,classnamekeyroleBObjectRef,idA3B5,createStampA2,markForDeleteA2,modifyStampA2,classnameA2A2,idA2A2,updateCountA2,updateStampA2) VALUES (@eventType,@principal,@replacementTypeAfter,@replacementTypeBefore,@classnamekeyroleAObjectRef,@idA3A5,@classnamekeyroleBObjectRef,@idA3B5,@createStampA2,@markForDeleteA2,@modifyStampA2,@classnameA2A2,@idA2A2,@updateCountA2,@updateStampA2)";


                    //                            using (SqlCommand insertCmd2Ins = new SqlCommand(insertQuery2Ins, conn1Ins))
                    //                            {

                    //                                insertCmd2Ins.Parameters.AddWithValue("@eventType", "Add");
                    //                                insertCmd2Ins.Parameters.AddWithValue("@principal", 12);
                    //                                insertCmd2Ins.Parameters.AddWithValue("@replacementTypeAfter", "a");
                    //                                insertCmd2Ins.Parameters.AddWithValue("@replacementTypeBefore", "");
                    //                                insertCmd2Ins.Parameters.AddWithValue("@classnamekeyroleAObjectRef", "wt.part.WTPartMaster");
                    //                                insertCmd2Ins.Parameters.AddWithValue("@idA3A5", idA2A2);
                    //                                insertCmd2Ins.Parameters.AddWithValue("@classnamekeyroleBObjectRef", "wt.part.WTPartMaster");
                    //                                insertCmd2Ins.Parameters.AddWithValue("@idA3B5", alternatifDeger2Sel);
                    //                                insertCmd2Ins.Parameters.AddWithValue("@createStampA2", DateTime.Now);
                    //                                insertCmd2Ins.Parameters.AddWithValue("@markForDeleteA2", 0);
                    //                                insertCmd2Ins.Parameters.AddWithValue("@modifyStampA2", DateTime.Now);
                    //                                insertCmd2Ins.Parameters.AddWithValue("@classnameA2A2", "wt.part.WTPartAlternateHistory");
                    //                                insertCmd2Ins.Parameters.AddWithValue("@idA2A2", ID);
                    //                                insertCmd2Ins.Parameters.AddWithValue("@updateCountA2", 1);
                    //                                insertCmd2Ins.Parameters.AddWithValue("@updateStampA2", DateTime.Now);

                    //                                try
                    //                                {
                    //                                    insertCmd2Ins.ExecuteNonQuery();
                    //                                }
                    //                                catch (SqlException ex)
                    //                                {
                    //                                    TempData["ErrorMessage"] = "HATA!" + ex.Message;

                    //                                }


                    //                            }

                    //                            _plm2.Query(catalogValue + ".id_sequence").Insert(new { dummy = "x" });

                    //                        }
                    //                        catch (SqlException ex)
                    //                        {

                    //                            TempData["ErrorMessage"] = "HATA!" + ex.Message;
                    //                        }

                    //                    }
                    //                }
                    //            }
                    //        }
                    //    }


                    //    }

                    //    if(importType == "EPM")
                    //    {

                    //    }

                    //}
                    //if (!string.IsNullOrEmpty(Name))
                    //{

                    //    if(importType == "WTPart")
                    //    {

                    //    }

                    //    if(importType == "EPM") { 
                    //    using (SqlConnection conn3Sel = new SqlConnection(connectionString))
                    //    {
                    //        conn3Sel.Open();

                    //        //// Eski Name değerini sakla
                    //        //var getOldName = $"SELECT Name FROM {catalogValue}.EPMDocumentMaster WHERE documentNumber = @Anaparca";
                    //        //using (SqlCommand getNameCmd = new SqlCommand(getOldName, conn3Sel))
                    //        //{
                    //        //    getNameCmd.Parameters.AddWithValue("@Anaparca", Anaparca);
                    //        //    var oldName = (string)getNameCmd.ExecuteScalar();

                    //        //    // Eski Name değerini Excel'e yaz
                    //        //    excelRow["ESKI_NAME"] = oldName;


                    //        //}

                    //        // Name alanını güncelle
                    //        var updateName = $"UPDATE {catalogValue}.EPMDocumentMaster SET name = @Name WHERE documentNumber = @Anaparca";
                    //        using (SqlCommand updateCmd = new SqlCommand(updateName, conn3Sel))
                    //        {
                    //            updateCmd.Parameters.AddWithValue("@Name", Name);
                    //            updateCmd.Parameters.AddWithValue("@Anaparca", Anaparca);
                    //            updateCmd.ExecuteNonQuery();
                    //        }
                    //    }
                    //    }
                    //}


                }


				//Tahminimce verisyon kodu olmadığı için eklenmiyor bu kontrol edilecek


				foreach (GenericObjectViewModel PlmDbPRoc in RecordList)
                {

                    var connectionString = _configuration.GetConnectionString("Plm");
                    var RowControl = _plm2.Query($"{catalogValue}.dbo.WTPartNoList").Where(new
                    {
                        WTPartNumber = PlmDbPRoc.Number.Trim(),
                        //versionIdA2versionInfo = PlmDbPRoc.Version
                    }).Get().ToList();

                    // Son veriyi dynamic türünde bir değişkene atıyoruz
                    dynamic lastRowControl = RowControl.LastOrDefault();

                    // lastRowList adında yeni bir liste oluşturuyoruz ve dynamic türündeki veriyi ekliyoruz
                    var lastRowList = new List<dynamic>();
                    if (lastRowControl != null)
                    {
                        lastRowList.Add(lastRowControl);
                    }


                    ////WTPartMaster Name Güncelleme sorgusu
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        var updateQueryName = $@"
						 UPDATE {catalogValue}.dbo.WTPartNoList
							SET
								name = @Queryname
							WHERE
								idA2A2 = @ida2a2ID";

                        var updateQueryNameparameters = new
                        {
                            ida2a2ID = lastRowList[0].idA2A2,
                            Queryname = PlmDbPRoc.Name,
                        };


                        conn.Execute(updateQueryName, updateQueryNameparameters);
                    }

                    try
                    {

                        if (PlmDbPRoc.DefinitionType.Contains("String"))
						{
							
							documentFunction(catalogValue, "StringValue", lastRowList, PlmDbPRoc, importType, connectionString);
                        }
                        if (PlmDbPRoc.DefinitionType.Contains("Integer"))
                        {
                            documentFunction(catalogValue, "IntegerValue", lastRowList, PlmDbPRoc, importType, connectionString);
                        }
                        if (PlmDbPRoc.DefinitionType.Contains("Float"))
                        {
                            documentFunction(catalogValue, "FloatValue", lastRowList, PlmDbPRoc, importType, connectionString);
                        }
                        if (PlmDbPRoc.DefinitionType.Contains("Unit"))
                        {
                            documentFunction(catalogValue, "UnitValue", lastRowList, PlmDbPRoc, importType, connectionString);
                        }
                        if (PlmDbPRoc.DefinitionType.Contains("Boolean"))
                        {
                            documentFunction(catalogValue, "BooleanValue", lastRowList, PlmDbPRoc, importType, connectionString);
                        }
                        if (PlmDbPRoc.DefinitionType.Contains("Timestamp"))
                        {
                            documentFunction(catalogValue, "TimestampValue", lastRowList, PlmDbPRoc, importType, connectionString);
                        }
                    }
                    catch (Exception)
                    {
                        
                        throw;
                    }

			

     //               if (importType == "EPM Document")
     //               {
					//	try
					//	{
					
					//	if (PlmDbPRoc.DefinitionType.Contains("String"))
					//	{
						

     //                           var IdSeq = _plm2.Query($"{catalogValue}.id_sequence").OrderByDesc("value").FirstOrDefault();
					//		if (lastRowList.Count() > 0)
					//		{
					//			var ID = lastRowList[0].EPMDocNumber;
					//			StringValue NewRecord = new StringValue();
					//			NewRecord.hierarchyIDA6 = PlmDbPRoc.HierId;
					//			NewRecord.idA2A2 = Convert.ToInt64(IdSeq.value) + 100;
					//			NewRecord.idA3A4 = Convert.ToInt64(ID);
					//			NewRecord.classnameA2A2 = "wt.iba.value.StringValue";
					//			NewRecord.idA3A5 = 0;
					//			NewRecord.idA3A6 = Convert.ToInt64(PlmDbPRoc.idA2A2);
					//			NewRecord.markForDeleteA2 = 0;
					//			NewRecord.modifyStampA2 = DateTime.Now;
					//			NewRecord.updateCountA2 = 1;
					//			NewRecord.updateStampA2 = DateTime.Now;
					//			NewRecord.createStampA2 = DateTime.Now;
					//			if (importType == "EPM Document")
					//			{
					//				NewRecord.classnamekeyA4 = "wt.epm.EPMDocument";
					//			}
					//			if (importType == "WTPart")
					//			{
					//				NewRecord.classnamekeyA4 = "wt.part.WTPart";
					//			}
					//			NewRecord.classnamekeyA6 = PlmDbPRoc.DefinitionType;
					//			NewRecord.value = PlmDbPRoc.AttrValue.ToUpper();
					//			NewRecord.value2 = PlmDbPRoc.AttrValue;
					//			if (PlmDbPRoc.AttrValue != "")
					//			{
					//					var existingRecord = _plm2.Query($"{catalogValue}.StringValue").Where(new
					//					{
					//						idA3A4 = ID,
					//						idA3A6 = PlmDbPRoc.idA2A2,
					//					}).Get<IntegerValue>().FirstOrDefault();

					//					if (existingRecord != null)
					//					{
					//						using (SqlConnection conn = new SqlConnection(connectionString))
					//						{
					//							conn.Open();

					//							// Güncelleme sorgusu
					//							var updateQuery = $@"
     //   UPDATE {catalogValue}.StringValue
     //   SET
     //       value = @AttrValueUpper,
     //       value2 = @AttrValue,
     //       updateStampA2 = @UpdateStamp
     //   WHERE
     //       idA3A4 = @ID
     //       AND idA3A6 = @PlmIdA2A2";

					//							// Güncelleme için parametreler
					//							var parameters = new
					//							{
					//								ID = existingRecord.idA3A4,
					//								PlmIdA2A2 = existingRecord.idA3A6,
					//								AttrValueUpper = PlmDbPRoc.AttrValue.ToUpper(),
					//								AttrValue = PlmDbPRoc.AttrValue,
					//								UpdateStamp = DateTime.Now
					//							};

					//							// Güncelleme sorgusunu çalıştır
					//							conn.Execute(updateQuery, parameters);
					//						}
					//					}
					//					else
					//					{


					//				var insert = _plm2.Query($"{catalogValue}.StringValue").Insert(NewRecord);

					//				if (insert == 1)
					//				{
					//					_plm2.Query($"{catalogValue}.id_sequence").Insert(new { dummy = "x" });
					//				}
					//					}
					//			}
					//		}
					//	}
					//	if (PlmDbPRoc.DefinitionType.Contains("Integer"))
					//	{
							

     //                           var IdSeq = _plm2.Query($"{catalogValue}.id_sequence").OrderByDesc("value").FirstOrDefault();
					//			if (lastRowList.Count() > 0)
					//			{
					//				var ID = lastRowList[0].EPMDocNumber;
					//				IntegerValue NewRecord = new IntegerValue();
					//				NewRecord.hierarchyIDA6 = PlmDbPRoc.HierId;
					//				NewRecord.idA2A2 = Convert.ToInt64(IdSeq.value) + 100;
					//				NewRecord.idA3A4 = Convert.ToInt64(ID);
					//				NewRecord.classnameA2A2 = "wt.iba.value.IntegerValue";
					//				NewRecord.idA3A5 = 0;
					//				NewRecord.idA3A6 = Convert.ToInt64(PlmDbPRoc.idA2A2);
					//				NewRecord.markForDeleteA2 = 0;
					//				NewRecord.modifyStampA2 = DateTime.Now;
					//				NewRecord.updateCountA2 = 1;
					//				NewRecord.updateStampA2 = DateTime.Now;
					//				NewRecord.createStampA2 = DateTime.Now;
					//				if (importType == "EPM Document")
					//				{
					//					NewRecord.classnamekeyA4 = "wt.epm.EPMDocument";
					//				}
					//				if (importType == "WTPart")
					//				{
					//					NewRecord.classnamekeyA4 = "wt.part.WTPart";
					//				}
					//				NewRecord.classnamekeyA6 = PlmDbPRoc.DefinitionType;
					//				NewRecord.value = Convert.ToInt64(PlmDbPRoc.AttrValue.ToUpper());
					//				if (PlmDbPRoc.AttrValue != "")
					//				{
					//					var existingRecord = _plm2.Query($"{catalogValue}.IntegerValue").Where(new
					//					{
					//						idA3A4 = ID,
					//						idA3A6 = PlmDbPRoc.idA2A2,
					//					}).Get<IntegerValue>().FirstOrDefault();

					//					if (existingRecord != null)
					//					{
					//						using (SqlConnection conn = new SqlConnection(connectionString))
					//						{
					//							conn.Open();

					//							// Güncelleme sorgusu
					//							var updateQuery = $@"
     //   UPDATE {catalogValue}.IntegerValue
     //   SET
     //       value = @AttrValueUpper,
     //       updateStampA2 = @UpdateStamp
     //   WHERE
     //       idA3A4 = @ID
     //       AND idA3A6 = @PlmIdA2A2";

					//							// Güncelleme için parametreler
					//							var parameters = new
					//							{
					//								ID = existingRecord.idA3A4,
					//								PlmIdA2A2 = existingRecord.idA3A6,
					//								AttrValueUpper = PlmDbPRoc.AttrValue.ToUpper(),
					//								AttrValue = PlmDbPRoc.AttrValue,
					//								UpdateStamp = DateTime.Now
					//							};

					//							// Güncelleme sorgusunu çalıştır
					//							conn.Execute(updateQuery, parameters);
					//						}
					//					}
					//					else
					//					{

					//						var insert = _plm2.Query($"{catalogValue}.IntegerValue").Insert(NewRecord);
					//					if (insert == 1)
					//					{
					//						_plm2.Query($"{catalogValue}.id_sequence").Insert(new { dummy = "x" });
					//					}
					//				}
					//			}
					//		}
					//	}
					//	if (PlmDbPRoc.DefinitionType.Contains("Float"))
					//	{
							

     //                           var IdSeq = _plm2.Query($"{catalogValue}.id_sequence").OrderByDesc("value").FirstOrDefault();
					//		if (lastRowList.Count() > 0)
					//		{
					//			var ID = lastRowList[0].EPMDocNumber;
					//			FloatValue NewRecord = new FloatValue();
					//			NewRecord.hierarchyIDA6 = PlmDbPRoc.HierId;
					//			NewRecord.idA2A2 = Convert.ToInt64(IdSeq.value) + 100;
					//			NewRecord.idA3A4 = Convert.ToInt64(ID);
					//			NewRecord.classnameA2A2 = "wt.iba.value.FloatValue";
					//			NewRecord.idA3A5 = 0;
					//			NewRecord.wtprecision = -1;
					//			NewRecord.idA3A6 = Convert.ToInt64(PlmDbPRoc.idA2A2);
					//			NewRecord.markForDeleteA2 = 0;
					//			NewRecord.modifyStampA2 = DateTime.Now;
					//			NewRecord.updateCountA2 = 1;
					//			NewRecord.updateStampA2 = DateTime.Now;
					//			NewRecord.createStampA2 = DateTime.Now;
					//			if (importType == "EPM Document")
					//			{
					//				NewRecord.classnamekeyA4 = "wt.epm.EPMDocument";
					//			}
					//			if (importType == "WTPart")
					//			{
					//				NewRecord.classnamekeyA4 = "wt.part.WTPart";
					//			}
					//			NewRecord.classnamekeyA6 = PlmDbPRoc.DefinitionType;
					//			NewRecord.value = float.Parse(PlmDbPRoc.AttrValue);
					//				if (PlmDbPRoc.AttrValue != "")
					//				{

					//					var existingRecord = _plm2.Query($"{catalogValue}.FloatValue").Where(new
					//					{
					//						idA3A4 = ID,
					//						idA3A6 = PlmDbPRoc.idA2A2,
					//					}).Get<IntegerValue>().FirstOrDefault();

					//					if (existingRecord != null)
					//					{
					//						using (SqlConnection conn = new SqlConnection(connectionString))
					//						{
					//							conn.Open();

					//							// Güncelleme sorgusu
					//							var updateQuery = $@"
     //   UPDATE {catalogValue}.FloatValue
     //   SET
     //       value = @AttrValueUpper,
     //       updateStampA2 = @UpdateStamp
     //   WHERE
     //       idA3A4 = @ID
     //       AND idA3A6 = @PlmIdA2A2";

					//							// Güncelleme için parametreler
					//							var parameters = new
					//							{
					//								ID = existingRecord.idA3A4,
					//								PlmIdA2A2 = existingRecord.idA3A6,
					//								AttrValueUpper = PlmDbPRoc.AttrValue.ToUpper(),
					//								AttrValue = PlmDbPRoc.AttrValue,
					//								UpdateStamp = DateTime.Now
					//							};

					//							// Güncelleme sorgusunu çalıştır
					//							conn.Execute(updateQuery, parameters);
					//						}
					//					}
					//					else
					//					{


					//						var insert = _plm2.Query($"{catalogValue}.FloatValue").Insert(NewRecord);
					//					if (insert == 1)
					//					{
					//						_plm2.Query($"{catalogValue}.id_sequence").Insert(new { dummy = "x" });
					//					}
					//				}
					//			}
					//		}
					//	}
					//	if (PlmDbPRoc.DefinitionType.Contains("Unit"))
					//	{
						

     //                           var IdSeq = _plm2.Query($"{catalogValue}.id_sequence").OrderByDesc("value").FirstOrDefault();
					//		if (lastRowList.Count() > 0)
					//		{
					//			var ID = lastRowList[0].EPMDocNumber;
					//			UnitValue NewRecord = new UnitValue();
					//			NewRecord.hierarchyIDA6 = PlmDbPRoc.HierId;
					//			NewRecord.idA2A2 = Convert.ToInt64(IdSeq.value) + 100;
					//			NewRecord.idA3A4 = Convert.ToInt64(ID);
					//			NewRecord.classnameA2A2 = "wt.iba.value.FloatValue";
					//			NewRecord.idA3A5 = 0;
					//			NewRecord.wtprecision = -1;
					//			NewRecord.idA3A6 = Convert.ToInt64(PlmDbPRoc.idA2A2);
					//			NewRecord.markForDeleteA2 = 0;
					//			NewRecord.modifyStampA2 = DateTime.Now;
					//			NewRecord.updateCountA2 = 1;
					//			NewRecord.updateStampA2 = DateTime.Now;
					//			NewRecord.createStampA2 = DateTime.Now;
					//			if (importType == "EPM Document")
					//			{
					//				NewRecord.classnamekeyA4 = "wt.epm.EPMDocument";
					//			}
					//			if (importType == "WTPart")
					//			{
					//				NewRecord.classnamekeyA4 = "wt.part.WTPart";
					//			}
					//			NewRecord.classnamekeyA6 = PlmDbPRoc.DefinitionType;
					//			NewRecord.value = float.Parse(PlmDbPRoc.AttrValue);
					//				if (PlmDbPRoc.AttrValue != "")
					//				{

					//					var existingRecord = _plm2.Query($"{catalogValue}.UnitValue").Where(new
					//					{
					//						idA3A4 = ID,
					//						idA3A6 = PlmDbPRoc.idA2A2,
					//					}).Get<IntegerValue>().FirstOrDefault();

					//					if (existingRecord != null)
					//					{
					//						using (SqlConnection conn = new SqlConnection(connectionString))
					//						{
					//							conn.Open();

					//							// Güncelleme sorgusu
					//							var updateQuery = $@"
     //   UPDATE {catalogValue}.UnitValue
     //   SET
     //       value = @AttrValueUpper,
     //       updateStampA2 = @UpdateStamp
     //   WHERE
     //       idA3A4 = @ID
     //       AND idA3A6 = @PlmIdA2A2";

					//							// Güncelleme için parametreler
					//							var parameters = new
					//							{
					//								ID = existingRecord.idA3A4,
					//								PlmIdA2A2 = existingRecord.idA3A6,
					//								AttrValueUpper = PlmDbPRoc.AttrValue.ToUpper(),
					//								AttrValue = PlmDbPRoc.AttrValue,
					//								UpdateStamp = DateTime.Now
					//							};

					//							// Güncelleme sorgusunu çalıştır
					//							conn.Execute(updateQuery, parameters);
					//						}
					//					}
					//					else
					//					{
					//						var insert = _plm2.Query($"{catalogValue}.UnitValue").Insert(NewRecord);
					//					if (insert == 1)
					//					{
					//						_plm2.Query($"{catalogValue}.id_sequence").Insert(new { dummy = "x" });
					//					}
					//				}
					//			}
					//		}
					//	}
					//	if (PlmDbPRoc.DefinitionType.Contains("Boolean"))
					//	{
						

     //                           var IdSeq = _plm2.Query($"{catalogValue}.id_sequence").OrderByDesc("value").FirstOrDefault();
					//		if (lastRowList.Count() > 0)
					//		{
					//			var ID = lastRowList[0].EPMDocNumber;
					//			bool BoolValue = false;
					//			if (PlmDbPRoc.AttrValue.ToLower() == "yes")
					//			{
					//				BoolValue = true;
					//			}
					//			else
					//			{
					//				BoolValue = false;
					//			}
					//			BooleanValue NewRecord = new BooleanValue();
					//			NewRecord.hierarchyIDA6 = PlmDbPRoc.HierId;
					//			NewRecord.idA2A2 = Convert.ToInt64(IdSeq.value) + 100;
					//			NewRecord.idA3A4 = Convert.ToInt64(ID);
					//			NewRecord.classnameA2A2 = "wt.iba.value.BooleanValue";
					//			NewRecord.idA3A5 = 0;
					//			NewRecord.idA3A6 = Convert.ToInt64(PlmDbPRoc.idA2A2);
					//			NewRecord.markForDeleteA2 = 0;
					//			NewRecord.modifyStampA2 = DateTime.Now;
					//			NewRecord.updateCountA2 = 1;
					//			NewRecord.updateStampA2 = DateTime.Now;
					//			NewRecord.createStampA2 = DateTime.Now;
					//			if (importType == "EPM Document")
					//			{
					//				NewRecord.classnamekeyA4 = "wt.epm.EPMDocument";
					//			}
					//			if (importType == "WTPart")
					//			{
					//				NewRecord.classnamekeyA4 = "wt.part.WTPart";
					//			}
					//			NewRecord.classnamekeyA6 = PlmDbPRoc.DefinitionType;
					//			NewRecord.value = Convert.ToByte(BoolValue);
					//			if (PlmDbPRoc.AttrValue != "")
					//			{
					//					var existingRecord = _plm2.Query($"{catalogValue}.BooleanValue").Where(new
					//					{
					//						idA3A4 = ID,
					//						idA3A6 = PlmDbPRoc.idA2A2,
					//					}).Get<IntegerValue>().FirstOrDefault();

					//					if (existingRecord != null)
					//					{
					//						using (SqlConnection conn = new SqlConnection(connectionString))
					//						{
					//							conn.Open();

					//							// Güncelleme sorgusu
					//							var updateQuery = $@"
     //   UPDATE {catalogValue}.BooleanValue
     //   SET
     //       value = @AttrValueUpper,
     //       updateStampA2 = @UpdateStamp
     //   WHERE
     //       idA3A4 = @ID
     //       AND idA3A6 = @PlmIdA2A2";

					//							// Güncelleme için parametreler
					//							var parameters = new
					//							{
					//								ID = existingRecord.idA3A4,
					//								PlmIdA2A2 = existingRecord.idA3A6,
					//								AttrValueUpper = PlmDbPRoc.AttrValue.ToUpper(),
					//								AttrValue = PlmDbPRoc.AttrValue,
					//								UpdateStamp = DateTime.Now
					//							};

					//							// Güncelleme sorgusunu çalıştır
					//							conn.Execute(updateQuery, parameters);
					//						}
					//					}
					//					else
					//					{
					//						var insert = _plm2.Query($"{catalogValue}.BooleanValue").Insert(NewRecord);
					//				if (insert == 1)
					//				{
					//					_plm2.Query($"{catalogValue}.id_sequence").Insert(new { dummy = "x" });
					//				}
					//				}
					//			}
					//		}
					//	}
					//	if (PlmDbPRoc.DefinitionType.Contains("Timestamp"))
					//	{
						

     //                           var IdSeq = _plm2.Query($"{catalogValue}.id_sequence").OrderByDesc("value").FirstOrDefault();
					//		if (lastRowList.Count() > 0)
					//		{
					//			var ID = lastRowList[0].EPMDocNumber;

					//			TimestampValue NewRecord = new TimestampValue();
					//			NewRecord.hierarchyIDA6 = PlmDbPRoc.HierId;
					//			NewRecord.idA2A2 = Convert.ToInt64(IdSeq.value) + 100;
					//			NewRecord.idA3A4 = Convert.ToInt64(ID);
					//			NewRecord.classnameA2A2 = "wt.iba.value.TimestampValue";
					//			NewRecord.idA3A5 = 0;
					//			NewRecord.idA3A6 = Convert.ToInt64(PlmDbPRoc.idA2A2);
					//			NewRecord.markForDeleteA2 = 0;
					//			NewRecord.modifyStampA2 = DateTime.Now;
					//			NewRecord.updateCountA2 = 1;
					//			NewRecord.updateStampA2 = DateTime.Now;
					//			NewRecord.createStampA2 = DateTime.Now;
					//			if (importType == "EPM Document")
					//			{
					//				NewRecord.classnamekeyA4 = "wt.epm.EPMDocument";
					//			}
					//			if (importType == "WTPart")
					//			{
					//				NewRecord.classnamekeyA4 = "wt.part.WTPart";
					//			}
					//			NewRecord.classnamekeyA6 = PlmDbPRoc.DefinitionType;
					//			NewRecord.value = Convert.ToDateTime(PlmDbPRoc.AttrValue).Date;
					//			if (PlmDbPRoc.AttrValue != "")
					//			{
					//					var existingRecord = _plm2.Query($"{catalogValue}.TimestampValue").Where(new
					//					{
					//						idA3A4 = ID,
					//						idA3A6 = PlmDbPRoc.idA2A2,
					//					}).Get<IntegerValue>().FirstOrDefault();

					//					if (existingRecord != null)
					//					{
					//						using (SqlConnection conn = new SqlConnection(connectionString))
					//						{
					//							conn.Open();

					//							// Güncelleme sorgusu
					//							var updateQuery = $@"
     //   UPDATE {catalogValue}.TimestampValue
     //   SET
     //       value = @AttrValueUpper,
     //       updateStampA2 = @UpdateStamp
     //   WHERE
     //       idA3A4 = @ID
     //       AND idA3A6 = @PlmIdA2A2";

					//							// Güncelleme için parametreler
					//							var parameters = new
					//							{
					//								ID = existingRecord.idA3A4,
					//								PlmIdA2A2 = existingRecord.idA3A6,
					//								AttrValueUpper = PlmDbPRoc.AttrValue.ToUpper(),
					//								AttrValue = PlmDbPRoc.AttrValue,
					//								UpdateStamp = DateTime.Now
					//							};

					//							// Güncelleme sorgusunu çalıştır
					//							conn.Execute(updateQuery, parameters);
					//						}
					//					}
					//					else
					//					{
					//						var insert = _plm2.Query($"{catalogValue}.TimestampValue").Insert(NewRecord);
					//				if (insert == 1)
					//				{
					//					_plm2.Query($"{catalogValue}.id_sequence").Insert(new { dummy = "x" });
					//				}
					//				}
					//			}
					//		}
					//	}
						
                        
     //                   }
					//	catch (Exception ex)
					//	{
					//		TempData["ErrorMessage"] = "HATA!" + ex.Message;
					//	}
					
     //               }

					//if (importType == "WTPart")
					//{
					//	try
					//	{

     //                       ////WTPartMaster Name Güncelleme sorgusu


     //                       if (PlmDbPRoc.DefinitionType.Contains("String"))
     //                       {
     //                           documentFunction(catalogValue, "StringValue", lastRowList, PlmDbPRoc, importType, connectionString);
     //                       }
     //                       if (PlmDbPRoc.DefinitionType.Contains("Integer"))
     //                       {
     //                           documentFunction(catalogValue, "IntegerValue", lastRowList, PlmDbPRoc, importType, connectionString);
     //                       }
     //                       if (PlmDbPRoc.DefinitionType.Contains("Float"))
     //                       {
     //                           documentFunction(catalogValue, "FloatValue", lastRowList, PlmDbPRoc, importType, connectionString);
     //                       }
     //                       if (PlmDbPRoc.DefinitionType.Contains("Unit"))
     //                       {
     //                           documentFunction(catalogValue, "UnitValue", lastRowList, PlmDbPRoc, importType, connectionString);
     //                       }
     //                       if (PlmDbPRoc.DefinitionType.Contains("Boolean"))
     //                       {
     //                           documentFunction(catalogValue, "BooleanValue", lastRowList, PlmDbPRoc, importType, connectionString);
     //                       }  
     //                       if (PlmDbPRoc.DefinitionType.Contains("Timestamp"))
     //                       {
     //                           documentFunction(catalogValue, "TimestampValue", lastRowList, PlmDbPRoc, importType, connectionString);
     //                       }

     //                       if (PlmDbPRoc.DefinitionType.Contains("String"))
     //                       {

     //                           var IdSeq = _plm2.Query($"{catalogValue}.id_sequence").OrderByDesc("value").FirstOrDefault();
     //                           if (lastRowList.Count() > 0)
     //                           {
     //                               var ID = lastRowList[0].WTPartNo;
     //                               StringValue NewRecord = new StringValue();
     //                               NewRecord.hierarchyIDA6 = PlmDbPRoc.HierId;
     //                               NewRecord.idA2A2 = Convert.ToInt64(IdSeq.value) + 100;
     //                               NewRecord.idA3A4 = Convert.ToInt64(ID);
     //                               NewRecord.classnameA2A2 = "wt.iba.value.StringValue";
     //                               NewRecord.idA3A5 = 0;
     //                               NewRecord.idA3A6 = Convert.ToInt64(PlmDbPRoc.idA2A2);
     //                               NewRecord.markForDeleteA2 = 0;
     //                               NewRecord.modifyStampA2 = DateTime.Now;
     //                               NewRecord.updateCountA2 = 1;
     //                               NewRecord.updateStampA2 = DateTime.Now;
     //                               NewRecord.createStampA2 = DateTime.Now;

     //                               if (importType == "EPM Document")
     //                               {
     //                                   NewRecord.classnamekeyA4 = "wt.epm.EPMDocument";
     //                               }
     //                               if (importType == "WTPart")
     //                               {
     //                                   NewRecord.classnamekeyA4 = "wt.part.WTPart";
     //                               }
     //                               NewRecord.classnamekeyA6 = PlmDbPRoc.DefinitionType;
     //                               NewRecord.value = PlmDbPRoc.AttrValue.ToUpper();
     //                               NewRecord.value2 = PlmDbPRoc.AttrValue;
     //                               if (PlmDbPRoc.AttrValue != "")
     //                               {
     //                                   //var existingRecord = _plm2.Query($"{catalogValue}.StringValue WHERE idA3A4 = {ID} AND value2 = {PlmDbPRoc.AttrValue.Trim()}").Get().FirstOrDefault();

     //                                   var existingRecord = _plm2.Query($"{catalogValue}.StringValue").Where(new
     //                                   {
     //                                       idA3A4 = ID,
     //                                       idA3A6 = PlmDbPRoc.idA2A2,
     //                                   }).Get<StringValue>().FirstOrDefault();

     //                                   if (existingRecord != null)
     //                                   {

     //                                       using (SqlConnection conn = new SqlConnection(connectionString))
     //                                       {
     //                                           conn.Open();




     //                                           // Güncelleme sorgusu
     //                                           var updateQuery = $@"
     //   UPDATE {catalogValue}.StringValue
     //   SET
     //       value = @AttrValueUpper,
     //       value2 = @AttrValue,
     //       updateStampA2 = @UpdateStamp
     //   WHERE
     //       idA3A4 = @ID
     //       AND idA3A6 = @PlmIdA2A2";

     //                                           // Güncelleme için parametreler
     //                                           var parameters = new
     //                                           {
     //                                               ID = existingRecord.idA3A4,
     //                                               PlmIdA2A2 = existingRecord.idA3A6,
     //                                               AttrValueUpper = PlmDbPRoc.AttrValue.ToUpper(),
     //                                               AttrValue = PlmDbPRoc.AttrValue,
     //                                               UpdateStamp = DateTime.Now
     //                                           };

     //                                           // Güncelleme sorgusunu çalıştır
     //                                           conn.Execute(updateQuery, parameters);
     //                                       }

     //                                       //                              existingRecord.idA3A4 = Convert.ToInt32(ID);
     //                                       //                              existingRecord.idA3A6 = Convert.ToInt32(PlmDbPRoc.idA2A2);
     //                                       //existingRecord.value = PlmDbPRoc.AttrValue.ToUpper();
     //                                       //existingRecord.value2 = PlmDbPRoc.AttrValue;
     //                                       //existingRecord.updateStampA2 = DateTime.Now;

     //                                       //var update = _plm2.Query($"{catalogValue}.StringValue").Update(existingRecord);

     //                                   }
     //                                   else
     //                                   {
     //                                       var insert = _plm2.Query($"{catalogValue}.StringValue").Insert(NewRecord);

     //                                       if (insert == 1)
     //                                       {
     //                                           _plm2.Query($"{catalogValue}.id_sequence").Insert(new { dummy = "x" });
     //                                       }
     //                                   }


     //                               }
     //                           }
     //                       }
     //                       if (PlmDbPRoc.DefinitionType.Contains("Integer"))
     //                       {


     //                           var IdSeq = _plm2.Query($"{catalogValue}.id_sequence").OrderByDesc("value").FirstOrDefault();
     //                           if (lastRowList.Count() > 0)
     //                           {
     //                               var ID = lastRowList[0].WTPartNo;
     //                               IntegerValue NewRecord = new IntegerValue();
     //                               NewRecord.hierarchyIDA6 = PlmDbPRoc.HierId;
     //                               NewRecord.idA2A2 = Convert.ToInt64(IdSeq.value) + 100;
     //                               NewRecord.idA3A4 = Convert.ToInt64(ID);
     //                               NewRecord.classnameA2A2 = "wt.iba.value.IntegerValue";
     //                               NewRecord.idA3A5 = 0;
     //                               NewRecord.idA3A6 = Convert.ToInt64(PlmDbPRoc.idA2A2);
     //                               NewRecord.markForDeleteA2 = 0;
     //                               NewRecord.modifyStampA2 = DateTime.Now;
     //                               NewRecord.updateCountA2 = 1;
     //                               NewRecord.updateStampA2 = DateTime.Now;
     //                               NewRecord.createStampA2 = DateTime.Now;
     //                               if (importType == "EPM Document")
     //                               {
     //                                   NewRecord.classnamekeyA4 = "wt.epm.EPMDocument";
     //                               }
     //                               if (importType == "WTPart")
     //                               {
     //                                   NewRecord.classnamekeyA4 = "wt.part.WTPart";
     //                               }
     //                               NewRecord.classnamekeyA6 = PlmDbPRoc.DefinitionType;
     //                               NewRecord.value = Convert.ToInt64(PlmDbPRoc.AttrValue.ToUpper());
     //                               if (PlmDbPRoc.AttrValue != "")
     //                               {
     //                                   var existingRecord = _plm2.Query($"{catalogValue}.IntegerValue").Where(new
     //                                   {
     //                                       idA3A4 = ID,
     //                                       idA3A6 = PlmDbPRoc.idA2A2,
     //                                   }).Get<IntegerValue>().FirstOrDefault();

     //                                   if (existingRecord != null)
     //                                   {
     //                                       using (SqlConnection conn = new SqlConnection(connectionString))
     //                                       {
     //                                           conn.Open();

     //                                           // Güncelleme sorgusu
     //                                           var updateQuery = $@"
     //   UPDATE {catalogValue}.IntegerValue
     //   SET
     //       value = @AttrValueUpper,
     //       updateStampA2 = @UpdateStamp
     //   WHERE
     //       idA3A4 = @ID
     //       AND idA3A6 = @PlmIdA2A2";

     //                                           // Güncelleme için parametreler
     //                                           var parameters = new
     //                                           {
     //                                               ID = existingRecord.idA3A4,
     //                                               PlmIdA2A2 = existingRecord.idA3A6,
     //                                               AttrValueUpper = PlmDbPRoc.AttrValue.ToUpper(),
     //                                               AttrValue = PlmDbPRoc.AttrValue,
     //                                               UpdateStamp = DateTime.Now
     //                                           };

     //                                           // Güncelleme sorgusunu çalıştır
     //                                           conn.Execute(updateQuery, parameters);
     //                                       }
     //                                   }
     //                                   else
     //                                   {
     //                                       var insert = _plm2.Query($"{catalogValue}.IntegerValue").Insert(NewRecord);
     //                                       if (insert == 1)
     //                                       {
     //                                           _plm2.Query($"{catalogValue}.id_sequence").Insert(new { dummy = "x" });
     //                                       }
     //                                   }
     //                               }
     //                           }
     //                       }
     //                       if (PlmDbPRoc.DefinitionType.Contains("Float"))
     //                       {


     //                           var IdSeq = _plm2.Query($"{catalogValue}.id_sequence").OrderByDesc("value").FirstOrDefault();
     //                           if (lastRowList.Count() > 0)
     //                           {
     //                               var ID = lastRowList[0].WTPartNo;
     //                               FloatValue NewRecord = new FloatValue();
     //                               NewRecord.hierarchyIDA6 = PlmDbPRoc.HierId;
     //                               NewRecord.idA2A2 = Convert.ToInt64(IdSeq.value) + 100;
     //                               NewRecord.idA3A4 = Convert.ToInt64(ID);
     //                               NewRecord.classnameA2A2 = "wt.iba.value.FloatValue";
     //                               NewRecord.idA3A5 = 0;
     //                               NewRecord.wtprecision = -1;
     //                               NewRecord.idA3A6 = Convert.ToInt64(PlmDbPRoc.idA2A2);
     //                               NewRecord.markForDeleteA2 = 0;
     //                               NewRecord.modifyStampA2 = DateTime.Now;
     //                               NewRecord.updateCountA2 = 1;
     //                               NewRecord.updateStampA2 = DateTime.Now;
     //                               NewRecord.createStampA2 = DateTime.Now;
     //                               if (importType == "EPM Document")
     //                               {
     //                                   NewRecord.classnamekeyA4 = "wt.epm.EPMDocument";
     //                               }
     //                               if (importType == "WTPart")
     //                               {
     //                                   NewRecord.classnamekeyA4 = "wt.part.WTPart";
     //                               }
     //                               NewRecord.classnamekeyA6 = PlmDbPRoc.DefinitionType;
     //                               NewRecord.value = float.Parse(PlmDbPRoc.AttrValue);
     //                               if (PlmDbPRoc.AttrValue != "")
     //                               {
     //                                   var existingRecord = _plm2.Query($"{catalogValue}.FloatValue").Where(new
     //                                   {
     //                                       idA3A4 = ID,
     //                                       idA3A6 = PlmDbPRoc.idA2A2,
     //                                   }).Get<FloatValue>().FirstOrDefault();

     //                                   if (existingRecord != null)
     //                                   {
     //                                       using (SqlConnection conn = new SqlConnection(connectionString))
     //                                       {
     //                                           conn.Open();

     //                                           // Güncelleme sorgusu
     //                                           var updateQuery = $@"
     //   UPDATE {catalogValue}.FloatValue
     //   SET
     //       value = @AttrValueUpper,
     //       updateStampA2 = @UpdateStamp
     //   WHERE
     //       idA3A4 = @ID
     //       AND idA3A6 = @PlmIdA2A2";

     //                                           // Güncelleme için parametreler
     //                                           var parameters = new
     //                                           {
     //                                               ID = existingRecord.idA3A4,
     //                                               PlmIdA2A2 = existingRecord.idA3A6,
     //                                               AttrValueUpper = PlmDbPRoc.AttrValue.ToUpper(),
     //                                               AttrValue = PlmDbPRoc.AttrValue,
     //                                               UpdateStamp = DateTime.Now
     //                                           };

     //                                           // Güncelleme sorgusunu çalıştır
     //                                           conn.Execute(updateQuery, parameters);
     //                                       }
     //                                   }
     //                                   else
     //                                   {
     //                                       var insert = _plm2.Query($"{catalogValue}.FloatValue").Insert(NewRecord);
     //                                       if (insert == 1)
     //                                       {
     //                                           _plm2.Query($"{catalogValue}.id_sequence").Insert(new { dummy = "x" });
     //                                       }
     //                                   }
     //                               }
     //                           }
     //                       }
     //                       if (PlmDbPRoc.DefinitionType.Contains("Unit"))
     //                       {

     //                           if (lastRowControl != null)
     //                           {
     //                               lastRowList.Add(lastRowControl);
     //                           }

     //                           var IdSeq = _plm2.Query($"{catalogValue}.id_sequence").OrderByDesc("value").FirstOrDefault();
     //                           if (lastRowList.Count() > 0)
     //                           {
     //                               var ID = lastRowList[0].WTPartNo;
     //                               UnitValue NewRecord = new UnitValue();
     //                               NewRecord.hierarchyIDA6 = PlmDbPRoc.HierId;
     //                               NewRecord.idA2A2 = Convert.ToInt64(IdSeq.value) + 100;
     //                               NewRecord.idA3A4 = Convert.ToInt64(ID);
     //                               NewRecord.classnameA2A2 = "wt.iba.value.FloatValue";
     //                               NewRecord.idA3A5 = 0;
     //                               NewRecord.wtprecision = -1;
     //                               NewRecord.idA3A6 = Convert.ToInt64(PlmDbPRoc.idA2A2);
     //                               NewRecord.markForDeleteA2 = 0;
     //                               NewRecord.modifyStampA2 = DateTime.Now;
     //                               NewRecord.updateCountA2 = 1;
     //                               NewRecord.updateStampA2 = DateTime.Now;
     //                               NewRecord.createStampA2 = DateTime.Now;
     //                               if (importType == "EPM Document")
     //                               {
     //                                   NewRecord.classnamekeyA4 = "wt.epm.EPMDocument";
     //                               }
     //                               if (importType == "WTPart")
     //                               {
     //                                   NewRecord.classnamekeyA4 = "wt.part.WTPart";
     //                               }
     //                               NewRecord.classnamekeyA6 = PlmDbPRoc.DefinitionType;
     //                               NewRecord.value = float.Parse(PlmDbPRoc.AttrValue);
     //                               if (PlmDbPRoc.AttrValue != "")
     //                               {
     //                                   var existingRecord = _plm2.Query($"{catalogValue}.UnitValue").Where(new
     //                                   {
     //                                       idA3A4 = ID,
     //                                       idA3A6 = PlmDbPRoc.idA2A2,
     //                                   }).Get<UnitValue>().FirstOrDefault();

     //                                   if (existingRecord != null)
     //                                   {
     //                                       using (SqlConnection conn = new SqlConnection(connectionString))
     //                                       {
     //                                           conn.Open();

     //                                           // Güncelleme sorgusu
     //                                           var updateQuery = $@"
     //   UPDATE {catalogValue}.UnitValue
     //   SET
     //       value = @AttrValueUpper,
     //       updateStampA2 = @UpdateStamp
     //   WHERE
     //       idA3A4 = @ID
     //       AND idA3A6 = @PlmIdA2A2";

     //                                           // Güncelleme için parametreler
     //                                           var parameters = new
     //                                           {
     //                                               ID = existingRecord.idA3A4,
     //                                               PlmIdA2A2 = existingRecord.idA3A6,
     //                                               AttrValueUpper = PlmDbPRoc.AttrValue.ToUpper(),
     //                                               AttrValue = PlmDbPRoc.AttrValue,
     //                                               UpdateStamp = DateTime.Now
     //                                           };

     //                                           // Güncelleme sorgusunu çalıştır
     //                                           conn.Execute(updateQuery, parameters);
     //                                       }
     //                                   }
     //                                   else
     //                                   {
     //                                       var insert = _plm2.Query($"{catalogValue}.UnitValue").Insert(NewRecord);
     //                                       if (insert == 1)
     //                                       {
     //                                           _plm2.Query($"{catalogValue}.id_sequence").Insert(new { dummy = "x" });
     //                                       }
     //                                   }
     //                               }
     //                           }
     //                       }
     //                       if (PlmDbPRoc.DefinitionType.Contains("Boolean"))
     //                       {

     //                           if (lastRowControl != null)
     //                           {
     //                               lastRowList.Add(lastRowControl);
     //                           }

     //                           var IdSeq = _plm2.Query($"{catalogValue}.id_sequence").OrderByDesc("value").FirstOrDefault();
     //                           if (lastRowList.Count() > 0)
     //                           {
     //                               var ID = lastRowList[0].WTPartNo;
     //                               bool BoolValue = false;
     //                               if (PlmDbPRoc.AttrValue.ToLower() == "yes")
     //                               {
     //                                   BoolValue = true;
     //                               }
     //                               else
     //                               {
     //                                   BoolValue = false;
     //                               }
     //                               BooleanValue NewRecord = new BooleanValue();
     //                               NewRecord.hierarchyIDA6 = PlmDbPRoc.HierId;
     //                               NewRecord.idA2A2 = Convert.ToInt64(IdSeq.value) + 100;
     //                               NewRecord.idA3A4 = Convert.ToInt64(ID);
     //                               NewRecord.classnameA2A2 = "wt.iba.value.BooleanValue";
     //                               NewRecord.idA3A5 = 0;
     //                               NewRecord.idA3A6 = Convert.ToInt64(PlmDbPRoc.idA2A2);
     //                               NewRecord.markForDeleteA2 = 0;
     //                               NewRecord.modifyStampA2 = DateTime.Now;
     //                               NewRecord.updateCountA2 = 1;
     //                               NewRecord.updateStampA2 = DateTime.Now;
     //                               NewRecord.createStampA2 = DateTime.Now;
     //                               if (importType == "EPM Document")
     //                               {
     //                                   NewRecord.classnamekeyA4 = "wt.epm.EPMDocument";
     //                               }
     //                               if (importType == "WTPart")
     //                               {
     //                                   NewRecord.classnamekeyA4 = "wt.part.WTPart";
     //                               }
     //                               NewRecord.classnamekeyA6 = PlmDbPRoc.DefinitionType;
     //                               NewRecord.value = Convert.ToByte(BoolValue);
     //                               if (PlmDbPRoc.AttrValue != "")
     //                               {
     //                                   var existingRecord = _plm2.Query($"{catalogValue}.BooleanValue").Where(new
     //                                   {
     //                                       idA3A4 = ID,
     //                                       idA3A6 = PlmDbPRoc.idA2A2,
     //                                   }).Get<BooleanValue>().FirstOrDefault();

     //                                   if (existingRecord != null)
     //                                   {
     //                                       using (SqlConnection conn = new SqlConnection(connectionString))
     //                                       {
     //                                           conn.Open();

     //                                           // Güncelleme sorgusu
     //                                           var updateQuery = $@"
     //   UPDATE {catalogValue}.BooleanValue
     //   SET
     //       value = @AttrValueUpper,
     //       updateStampA2 = @UpdateStamp
     //   WHERE
     //       idA3A4 = @ID
     //       AND idA3A6 = @PlmIdA2A2";

     //                                           // Güncelleme için parametreler
     //                                           var parameters = new
     //                                           {
     //                                               ID = existingRecord.idA3A4,
     //                                               PlmIdA2A2 = existingRecord.idA3A6,
     //                                               AttrValueUpper = PlmDbPRoc.AttrValue.ToUpper(),
     //                                               AttrValue = PlmDbPRoc.AttrValue,
     //                                               UpdateStamp = DateTime.Now
     //                                           };

     //                                           // Güncelleme sorgusunu çalıştır
     //                                           conn.Execute(updateQuery, parameters);
     //                                       }
     //                                   }
     //                                   else
     //                                   {
     //                                       var insert = _plm2.Query($"{catalogValue}.BooleanValue").Insert(NewRecord);
     //                                       if (insert == 1)
     //                                       {
     //                                           _plm2.Query($"{catalogValue}.id_sequence").Insert(new { dummy = "x" });
     //                                       }
     //                                   }
     //                               }
     //                           }
     //                       }
     //                       if (PlmDbPRoc.DefinitionType.Contains("Timestamp"))
     //                       {

     //                           if (lastRowControl != null)
     //                           {
     //                               lastRowList.Add(lastRowControl);
     //                           }

     //                           var IdSeq = _plm2.Query($"{catalogValue}.id_sequence").OrderByDesc("value").FirstOrDefault();
     //                           if (lastRowList.Count() > 0)
     //                           {
     //                               var ID = lastRowList[0].WTPartNo;

     //                               TimestampValue NewRecord = new TimestampValue();
     //                               NewRecord.hierarchyIDA6 = PlmDbPRoc.HierId;
     //                               NewRecord.idA2A2 = Convert.ToInt64(IdSeq.value) + 100;
     //                               NewRecord.idA3A4 = Convert.ToInt64(ID);
     //                               NewRecord.classnameA2A2 = "wt.iba.value.TimestampValue";
     //                               NewRecord.idA3A5 = 0;
     //                               NewRecord.idA3A6 = Convert.ToInt64(PlmDbPRoc.idA2A2);
     //                               NewRecord.markForDeleteA2 = 0;
     //                               NewRecord.modifyStampA2 = DateTime.Now;
     //                               NewRecord.updateCountA2 = 1;
     //                               NewRecord.updateStampA2 = DateTime.Now;
     //                               NewRecord.createStampA2 = DateTime.Now;
     //                               if (importType == "EPM Document")
     //                               {
     //                                   NewRecord.classnamekeyA4 = "wt.epm.EPMDocument";
     //                               }
     //                               if (importType == "WTPart")
     //                               {
     //                                   NewRecord.classnamekeyA4 = "wt.part.WTPart";
     //                               }
     //                               NewRecord.classnamekeyA6 = PlmDbPRoc.DefinitionType;
     //                               NewRecord.value = Convert.ToDateTime(PlmDbPRoc.AttrValue).Date;
     //                               if (PlmDbPRoc.AttrValue != "")
     //                               {
     //                                   var existingRecord = _plm2.Query($"{catalogValue}.TimestampValue").Where(new
     //                                   {
     //                                       idA3A4 = ID,
     //                                       idA3A6 = PlmDbPRoc.idA2A2,
     //                                   }).Get<TimestampValue>().FirstOrDefault();

     //                                   if (existingRecord != null)
     //                                   {
     //                                       using (SqlConnection conn = new SqlConnection(connectionString))
     //                                       {
     //                                           conn.Open();

     //                                           // Güncelleme sorgusu
     //                                           var updateQuery = $@"
     //   UPDATE {catalogValue}.TimestampValue
     //   SET
     //       value = @AttrValueUpper,
     //       updateStampA2 = @UpdateStamp
     //   WHERE
     //       idA3A4 = @ID
     //       AND idA3A6 = @PlmIdA2A2";

     //                                           // Güncelleme için parametreler
     //                                           var parameters = new
     //                                           {
     //                                               ID = existingRecord.idA3A4,
     //                                               PlmIdA2A2 = existingRecord.idA3A6,
     //                                               AttrValueUpper = PlmDbPRoc.AttrValue.ToUpper(),
     //                                               AttrValue = PlmDbPRoc.AttrValue,
     //                                               UpdateStamp = DateTime.Now
     //                                           };

     //                                           // Güncelleme sorgusunu çalıştır
     //                                           conn.Execute(updateQuery, parameters);
     //                                       }
     //                                   }
     //                                   else
     //                                   {
     //                                       var insert = _plm2.Query($"{catalogValue}.TimestampValue").Insert(NewRecord);
     //                                       if (insert == 1)
     //                                       {
     //                                           _plm2.Query($"{catalogValue}.id_sequence").Insert(new { dummy = "x" });
     //                                       }
     //                                   }
     //                               }
     //                           }
     //                       }

     //                   }
					//	catch (Exception ex)
					//	{
					//		TempData["ErrorMessage"] = "HATA!" + ex.Message;
					//	}

	
					//}


				}

                return Ok(new { status = true , message = "Data Sync Success" });

            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "HATA!" + ex.Message;
                return RedirectToAction("Index");
            }
        }


		public void documentFunction(string catalogValue, string valueType,List<dynamic> data, GenericObjectViewModel PlmDbPRoc,string importType,string connectionString)
		{
            var IdSeq = _plm2.Query($"{catalogValue}.id_sequence").OrderByDesc("value").FirstOrDefault();
            if (data.Count() > 0)
            {
                long ID = 0;
                if(importType == "WTPart")
                {
                ID = data[0].WTPartNo;
                }
                if(importType == "EPM Document")
                {
                    ID = data[0].EPMDocNumber;
                }
                StringValue NewRecord = new StringValue();
                NewRecord.hierarchyIDA6 = PlmDbPRoc.HierId;
                NewRecord.idA2A2 = Convert.ToInt64(IdSeq.value) + 100;
                NewRecord.idA3A4 = Convert.ToInt64(ID);
                NewRecord.classnameA2A2 = $"wt.iba.value.{valueType}";
                NewRecord.idA3A5 = 0;
                NewRecord.idA3A6 = Convert.ToInt64(PlmDbPRoc.idA2A2);
                NewRecord.markForDeleteA2 = 0;
                NewRecord.modifyStampA2 = DateTime.Now;
                NewRecord.updateCountA2 = 1;
                NewRecord.updateStampA2 = DateTime.Now;
                NewRecord.createStampA2 = DateTime.Now;

                if (importType == "EPM Document")
                {
                    NewRecord.classnamekeyA4 = "wt.epm.EPMDocument";
                }
                if (importType == "WTPart")
                {
                    NewRecord.classnamekeyA4 = "wt.part.WTPart";
                }
                NewRecord.classnamekeyA6 = PlmDbPRoc.DefinitionType;
                NewRecord.value = PlmDbPRoc.AttrValue.ToUpper();
                if(valueType == "StringValue")
                {
                NewRecord.value2 = PlmDbPRoc.AttrValue;
                }
                if (PlmDbPRoc.AttrValue != "")
                {

                    var existingRecord = _plm2.Query($"{catalogValue}.{valueType}").Where(new
                    {
                        idA3A4 = ID,
                        idA3A6 = PlmDbPRoc.idA2A2,
                    }).Get<StringValue>().FirstOrDefault();

                    if (existingRecord != null)
                    {

                        using (SqlConnection conn = new SqlConnection(connectionString))
                        {
                            conn.Open();




                            // Güncelleme sorgusu
                            var updateQuery = "";
                            if (valueType == "StringValue")
                            {
                                 updateQuery = $@"
        UPDATE {catalogValue}.{valueType}
        SET
            value = @AttrValueUpper,
            value2 = @AttrValue,
            updateStampA2 = @UpdateStamp
        WHERE
            idA3A4 = @ID
            AND idA3A6 = @PlmIdA2A2";
                            }
                            else
                            {
                                 updateQuery = $@"
        UPDATE {catalogValue}.{valueType}
        SET
            value = @AttrValueUpper,
            updateStampA2 = @UpdateStamp
        WHERE
            idA3A4 = @ID
            AND idA3A6 = @PlmIdA2A2";
                            }
       

                            // Güncelleme için parametreler
                            var parameters = new
                            {
                                ID = existingRecord.idA3A4,
                                PlmIdA2A2 = existingRecord.idA3A6,
                                AttrValueUpper = PlmDbPRoc.AttrValue.ToUpper(),
                                AttrValue = PlmDbPRoc.AttrValue,
                                UpdateStamp = DateTime.Now
                            };

                            // Güncelleme sorgusunu çalıştır
                         conn.Execute(updateQuery, parameters);
							LogService logService = new LogService(_configuration);
							var loggedInUsername = HttpContext.User.Identity.Name;
							logService.AddNewLogEntry(importType+" da '" + PlmDbPRoc.AttrValue + "' verisi başarılı bir şekilde güncelleştirildi.", PlmDbPRoc.AttrValue, "Update",loggedInUsername);
						}

               
                    }
                    else
                    {
                        var insert = _plm2.Query($"{catalogValue}.{valueType}").Insert(NewRecord);
						LogService logService = new LogService(_configuration);
						var loggedInUsername = HttpContext.User.Identity.Name;
						logService.AddNewLogEntry(importType + " a '" + PlmDbPRoc.AttrValue + "'  verisi başarılı bir şekilde eklendi.", PlmDbPRoc.AttrValue, "Insert", loggedInUsername);
						if (insert == 1)
                        {
                            _plm2.Query($"{catalogValue}.id_sequence").Insert(new { dummy = "x" });
                        }
                    }


                }
            }
        }





        List<ExcelDetailResult> detailData = new List<ExcelDetailResult>();

        public async Task<JsonResult> ExcelFileControl(IFormCollection data,string[] HataListesi, string[] Hatasizlar,string excelFile,string importType)
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
                    LogService logService = new LogService(_configuration);
                    foreach (var hata in hataListesi)
                    {
                        //logService.AddNewLogEntry(hata);
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


                    var successData = new { success = true, HataListesi = hataListesi, Hatasizlar = hatasizlar, veriler = rest.veriler, basliklar = rest.basliklar, hataNo = rest.hataNo };
                    return Json(successData);


                }
                else
                {

                    if(importType == "EPM Document")
                    {
                        try
                        {

                       ExcelProc(data,importType);
                        TempData["SuccessMessage"] = "EPM e gönderildi.";

                        }
                        catch (Exception)
                        {
                            TempData["ErrorMessage"] = "EPM e gönderilemedi.";
                        }
                    }
                    if (importType == "WTPart")
                    {
                        try
                        {

                            ExcelProc(data, importType);
                            TempData["SuccessMessage"] = "WTPart a gönderildi.";

                        }
                        catch (Exception)
                        {
                            TempData["ErrorMessage"] = "WTPart a gönderilemedi.";
                        }
                    }

                    return Json("");

                }


                // JSON formatında verileri döndürün


              
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
