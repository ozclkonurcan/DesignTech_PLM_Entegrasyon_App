using DesignTech_PLM_Entegrasyon_App.MVC.Models;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace DesignTech_PLM_Entegrasyon_App.MVC.Controllers
{
    [Authorize]
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

                        RunSqlScript(model.Catalog, connectionString);
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



        [HttpPost]
        public async Task<IActionResult> RunSqlScript(string catalogValue, string connectionString)
        {
            try
            {
                //string catalogValue = _configuration["Catalog"]; // Kataloğunuzu buraya ekleyin

                string sqlScript = @"
            USE [" + catalogValue + @"]




            /****** Object:  View [" + catalogValue + @"].[DocumentList]    Script Date: 25.09.2023 12:29:36 ******/
            SET ANSI_NULLS ON
            GO
            SET QUOTED_IDENTIFIER ON
            GO
            CREATE VIEW [" + catalogValue + @"].[DocumentList]
            AS
            SELECT " + catalogValue + @".WTDocumentMaster.idA2A2, " + catalogValue + @".WTDocumentMaster.WTDocumentNumber, " + catalogValue + @".WTDocumentMaster.name, " + catalogValue + @".WTDocument.statestate, " + catalogValue + @".WTDocument.idA3containerReference, 
                  " + catalogValue + @".PDMLinkProduct.namecontainerInfo, " + catalogValue + @".WTDocument.idA3B2folderingInfo, " + catalogValue + @".SubFolder.name AS EXPR1, " + catalogValue + @".WTDocument.idA3D2iterationInfo, " + catalogValue + @".WTUser.fullName, " + catalogValue + @".WTDocument.latestiterationInfo, 
                  " + catalogValue + @".WTDocument.idA3B2iterationInfo, " + catalogValue + @".WTDocument.modifyStampA2, WTUser_1.fullName AS EXPR2, " + catalogValue + @".WTDocument.versionIdA2versionInfo, " + catalogValue + @".WTDocument.versionLevelA2versionInfo, 
                  " + catalogValue + @".WTDocumentMaster.name + ' (v' + CAST(" + catalogValue + @".WTDocument.versionIdA2versionInfo AS VARCHAR(5)) + '.' + CAST(" + catalogValue + @".WTDocument.iterationIdA2iterationInfo AS VARCHAR(5)) + ') ' AS Version, 
                  " + catalogValue + @".WTDocument.idA3C2iterationInfo
            FROM     " + catalogValue + @".WTDocument INNER JOIN
                  " + catalogValue + @".WTDocumentMaster ON " + catalogValue + @".WTDocument.idA3masterReference = " + catalogValue + @".WTDocumentMaster.idA2A2 INNER JOIN
                  " + catalogValue + @".PDMLinkProduct ON " + catalogValue + @".WTDocument.idA3containerReference = " + catalogValue + @".PDMLinkProduct.idA2A2 INNER JOIN
                  " + catalogValue + @".SubFolder ON " + catalogValue + @".WTDocument.idA3B2folderingInfo = " + catalogValue + @".SubFolder.idA2A2 INNER JOIN
                  " + catalogValue + @".WTUser ON " + catalogValue + @".WTDocument.idA3D2iterationInfo = " + catalogValue + @".WTUser.idA2A2 INNER JOIN
                  " + catalogValue + @".WTUser AS WTUser_1 ON " + catalogValue + @".WTDocument.idA3B2iterationInfo = WTUser_1.idA2A2
            GO

            /****** Object:  View [" + catalogValue + @"].[EPMDocNumberList]    Script Date: 25.09.2023 12:29:36 ******/
            SET ANSI_NULLS ON
            GO
            SET QUOTED_IDENTIFIER ON
            GO
            CREATE VIEW [" + catalogValue + @"].[EPMDocNumberList]
            AS
            SELECT " + catalogValue + @".EPMDocumentMaster.idA2A2, " + catalogValue + @".EPMDocumentMaster.documentNumber, " + catalogValue + @".EPMDocumentMaster.name, " + catalogValue + @".EPMDocument.latestiterationInfo, " + catalogValue + @".EPMDocument.idA2A2 AS EPMDocNumber, 
                  " + catalogValue + @".EPMDocument.versionIdA2versionInfo
            FROM     " + catalogValue + @".EPMDocument INNER JOIN
                  " + catalogValue + @".EPMDocumentMaster ON " + catalogValue + @".EPMDocument.idA3masterReference = " + catalogValue + @".EPMDocumentMaster.idA2A2
            WHERE  (" + catalogValue + @".EPMDocument.latestiterationInfo = 1)
            GO

            /****** Object:  View [" + catalogValue + @"].[WTDocList]    Script Date: 25.09.2023 12:29:36 ******/
            SET ANSI_NULLS ON
            GO
            SET QUOTED_IDENTIFIER ON
            GO
            CREATE VIEW [" + catalogValue + @"].[WTDocList]
            AS
            SELECT " + catalogValue + @".WTDocumentMaster.idA2A2, " + catalogValue + @".WTDocumentMaster.WTDocumentNumber, " + catalogValue + @".WTDocumentMaster.name, " + catalogValue + @".WTDocument.idA2A2 AS WTDocNo, " + catalogValue + @".WTDocument.latestiterationInfo
            FROM     " + catalogValue + @".WTDocumentMaster INNER JOIN
                  " + catalogValue + @".WTDocument ON " + catalogValue + @".WTDocumentMaster.idA2A2 = " + catalogValue + @".WTDocument.idA3masterReference
            WHERE  (" + catalogValue + @".WTDocument.latestiterationInfo = 1)
            GO

            /****** Object:  View [" + catalogValue + @"].[WTPartNoList]    Script Date: 25.09.2023 12:29:36 ******/
            SET ANSI_NULLS ON
            GO
            SET QUOTED_IDENTIFIER ON
            GO
            CREATE VIEW [" + catalogValue + @"].[WTPartNoList]
            AS
            SELECT " + catalogValue + @".WTPartMaster.idA2A2, " + catalogValue + @".WTPartMaster.WTPartNumber, " + catalogValue + @".WTPart.versionIdA2versionInfo, " + catalogValue + @".WTPartMaster.name, " + catalogValue + @".WTPart.latestiterationInfo, " + catalogValue + @".WTPart.idA2A2 AS WTPartNo
            FROM     " + catalogValue + @".WTPartMaster INNER JOIN
                  " + catalogValue + @".WTPart ON " + catalogValue + @".WTPartMaster.idA2A2 = " + catalogValue + @".WTPart.idA3masterReference
            WHERE  (" + catalogValue + @".WTPart.latestiterationInfo = 1)
            GO

            /****** Object:  View [" + catalogValue + @"].[IntegrationDefinitionList]    Script Date: 25.09.2023 12:29:36 ******/
            SET ANSI_NULLS ON
            GO
            SET QUOTED_IDENTIFIER ON
            GO
            CREATE VIEW [" + catalogValue + @"].[IntegrationDefinitionList] AS
            select displayName,hierarchyID,classnameA2A2,idA2A2 from " + catalogValue + @".FloatDefinition where createStampA2 > DATEADD(day, 1,CONVERT(DATETIME, (select createStampA2 from " + catalogValue + @".WTOrganization), 102))
            UNION
            select displayName,hierarchyID,classnameA2A2,idA2A2 from " + catalogValue + @".BooleanDefinition where createStampA2 > DATEADD(day, 1,CONVERT(DATETIME, (select createStampA2 from " + catalogValue + @".WTOrganization), 102))
            UNION
            select displayName,hierarchyID,classnameA2A2,idA2A2 from " + catalogValue + @".IntegerDefinition  where createStampA2 > DATEADD(day, 1,CONVERT(DATETIME, (select createStampA2 from " + catalogValue + @".WTOrganization), 102))
            UNION
            select displayName,hierarchyID,classnameA2A2,idA2A2 from " + catalogValue + @".StringDefinition  where createStampA2 > DATEADD(day, 1,CONVERT(DATETIME, (select createStampA2 from " + catalogValue + @".WTOrganization), 102))
            UNION
            select displayName,hierarchyID,classnameA2A2,idA2A2 from " + catalogValue + @".TimestampDefinition  where createStampA2 > DATEADD(day, 1,CONVERT(DATETIME, (select createStampA2 from " + catalogValue + @".WTOrganization), 102))
            UNION
            select displayName,hierarchyID,classnameA2A2,idA2A2 from " + catalogValue + @".UnitDefinition  where createStampA2 > DATEADD(day, 1,CONVERT(DATETIME, (select createStampA2 from " + catalogValue + @".WTOrganization), 102))
            ;
        ";




//                /****** Object:  View [" + catalogValue + @"].[Change_Notice]    Script Date: 19.12.2023 13:06:43 ******/
//                SET ANSI_NULLS ON
//                GO
//SET QUOTED_IDENTIFIER ON
//GO
//CREATE VIEW[" + catalogValue + @"].[Change_Notice] AS
//SELECT WTChangeOrder2Master.WTCHGORDERNUMBER AS CN_NUMBER, WTChangeOrder2Master.name AS CHANGE_NOTICE, WTChangeOrder2.statestate AS STATE, WTChangeOrder2Master.updateStampA2 AS LastUpdateTimestamp
//FROM " + catalogValue + @".WTChangeOrder2Master
//INNER JOIN " + catalogValue + @".WTChangeOrder2 ON WTChangeOrder2Master.idA2A2 = WTChangeOrder2.idA3masterReference
//GO


                //string connectionString = _configuration.GetConnectionString("Plm"); // ConnectionStrings'deki adı buraya ekleyin

                await using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string[] sqlStatements = sqlScript.Split(new[] { "GO" }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string sqlStatement in sqlStatements)
                    {
                        await using (SqlCommand command = new SqlCommand(sqlStatement, connection))
                        {
                            try
                            {

                                command.ExecuteNonQuery();
                            }
                            catch (SqlException ex)
                            {
                                if (ex.Number == 2714) // 2714, SQL'de "There is already an object named 'xxx' in the database." hatasıdır.
                                {
                                    TempData["ErrorMessage"] = "UYARI!" + ex.Message;
                                    continue;
                                }
                                throw;
                            }
                        }
                    }

                    connection.Close();
                }

                TempData["SuccessMessage"] = "SQL script başarıyla çalıştırıldı.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "HATA: " + ex.Message;

            }

            return RedirectToAction("Index");
        }
        //SQL AYARLARINI APPSETTINGS JSON A AKTARMA KISMI

    }
}
