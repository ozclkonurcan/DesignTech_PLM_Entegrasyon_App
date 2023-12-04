using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Serilog;
using System.Globalization;
using static DesignTech_PLM_Entegrasyon_App.MVC.Controllers.LogController;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using ExcelDataReader;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using SqlKata.Execution;
using Azure;

namespace DesignTech_PLM_Entegrasyon_App.MVC.Helper
{
    public class LogService
    {
        private readonly IConfiguration _configuration;

        public LogService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        string currentMonthFolder;
        string logFileName;

        public void ErrorFileManagement(string message,string dosyaIsmi)
        {
            logFileName = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "errorFile", dosyaIsmi);

            // Kontrol etmek için dosya var mı?
            //if (File.Exists(logFileName))
            //{
            //    // Dosya varsa içeriğini okuyun
            //    string existingContent = File.ReadAllText(logFileName);


            //    // Aynı veri zaten dosyada mevcut mu kontrol edin
            //    if (existingContent.Contains(message))
            //    {
            //        // Aynı veri zaten kaydedilmişse bir sonraki veriye geçin
            //        return;
            //    }
            //}

            // Dosya veya veri yoksa yeni bir log oluşturun veya var olanın sonuna ekleyin
       
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File(new CustomJsonFormatter(), logFileName, shared: true)
                .CreateLogger();

            Log.Information(message);
            Log.CloseAndFlush();
        }
        //   public void ErrorFileManagement(string message)
        //{
        //    currentMonthFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "errorFile", DateTime.Now.ToString("MMMM-yyyy", CultureInfo.InvariantCulture));
        //    logFileName = Path.Combine(currentMonthFolder,  "errorFile.json");

        //    // Kontrol etmek için dosya var mı?
        //    if (File.Exists(logFileName))
        //    {
        //        // Dosya varsa içeriğini okuyun
        //        string existingContent = File.ReadAllText(logFileName);


        //        // Aynı veri zaten dosyada mevcut mu kontrol edin
        //        if (existingContent.Contains(message))
        //        {
        //            // Aynı veri zaten kaydedilmişse bir sonraki veriye geçin
        //            return;
        //        }
        //    }

        //    // Dosya veya veri yoksa yeni bir log oluşturun veya var olanın sonuna ekleyin
       
        //    Log.Logger = new LoggerConfiguration()
        //        .MinimumLevel.Information()
        //        .WriteTo.File(new CustomJsonFormatter(), logFileName, shared: true)
        //        .CreateLogger();

        //    Log.Information(message);
        //    Log.CloseAndFlush();
        //}



        public void AddNewLogEntry(string message,string fileName, string operation,string kullaniciAdi)
        {
            try
            {

          
            currentMonthFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "logs", DateTime.Now.ToString("MMMM-yyyy", CultureInfo.InvariantCulture));
            string dateFormatted = DateTime.Now.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture);
            logFileName = Path.Combine(currentMonthFolder, dateFormatted + ".json");
                var logObject = new
                {
                    ExcelDosya = fileName,
                    Text = message,
                    Operation = operation,
                    KullaniciAdi = kullaniciAdi,
                    Durum = true,

                    Properties = new { }
                };

                string json = JsonConvert.SerializeObject(logObject);
                Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File(new CustomJsonFormatter(), logFileName, shared: true)
                .CreateLogger();

			

			Log.Information(json);
            Log.CloseAndFlush();
            }
            catch (Exception ex)
            {
                Log.Error("Log oluşturulurken hata oluştu: " + ex.Message);
            }
        }

		public void AddNewLogEntry2(string message, string fileName, string operation)
		{
			try
			{


				currentMonthFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "logs2/ProcessLog");
				//string dateFormatted = DateTime.Now.ToString("standartLogFile");
				logFileName = Path.Combine(currentMonthFolder, "standartLogFile.json");

				var logObject = new
				{
					ExcelDosya = fileName,
					Text = message,
					Operation = operation,
					Durum = true,

					Properties = new { }
				};

				string json = JsonConvert.SerializeObject(logObject);
				Log.Logger = new LoggerConfiguration()
				.MinimumLevel.Information()
				.WriteTo.File(new CustomJsonFormatter(), logFileName, shared: true)
				.CreateLogger();



				Log.Information(json);
				Log.CloseAndFlush();
			}
			catch (Exception ex)
			{
				Log.Error("Log oluşturulurken hata oluştu: " + ex.Message);
			}
		}







		public List<ExcelLogEntry> GetConvertedDataFromJson()
        {
            string fullPath = logFileName;
            string desiredPath = fullPath.Substring(fullPath.IndexOf("wwwroot"));
            string jsonFilePath = Path.Combine(desiredPath);

            var excelLogEntries = new List<ExcelLogEntry>();

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

                            var excelLogEntry = new ExcelLogEntry(logEntry.Message.Hata)
                            {
                                ExcelDosya = logEntry.Message.ExcelDosya,
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

            return excelLogEntries;
        }


            public class ExcelDetailResult
            {
                public List<string> Basliklar { get; set; }
                public List<object> Veriler { get; set; }
            public string Mesaj { get; set; }
            }



        public ExcelDetailResult GetExcelDetails(string excelFileName, int sutunNo, string hataNo)
        {
            var result = new ExcelDetailResult();
            try
            {
                var fileName = Directory.GetCurrentDirectory() + "\\wwwroot\\ExcelInformation\\" + excelFileName;

                var schema = _configuration["Catalog"];
                var connectionString = _configuration.GetConnectionString("Plm");
                using IDbConnection connection = new SqlConnection(connectionString);

                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                DataSet excelData;



                using (var stream = System.IO.File.Open(fileName, FileMode.Open, FileAccess.Read))
                {
                    using (IExcelDataReader reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        var dataSet = reader.AsDataSet();
                        var dataTable = dataSet.Tables[0];
                        excelData = reader.AsDataSet(new ExcelDataSetConfiguration()
                        {
                            ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                            {
                                UseHeaderRow = true
                            }
                        });

                        //var dataTable = excelData.Tables[0];

                        List<string> basliklar = new List<string>();
                        List<Tuple<string, int, List<string>, List<string>, int>> eslesenSatirlar = new List<Tuple<string, int, List<string>, List<string>, int>>();

                        // Excel dosyasının 1. satırını başlıklar olarak al
                        var row = dataTable.Rows[0];
                        for (int i = 0; i < dataTable.Columns.Count; i++)
                        {
                            basliklar.Add(row[i].ToString());
                        }

                        foreach (DataRow excelRow in excelData.Tables[0].Rows)
                        {
                            var Anaparca = excelRow["Number"].ToString();
                            var Name = excelRow["NAME"].ToString();
                            var catalogValue = _configuration["Catalog"];
                            if (!string.IsNullOrEmpty(Name))
                            {
                                using (SqlConnection conn3Sel = new SqlConnection(connectionString))
                                {
                                    conn3Sel.Open();

                                    // Eski Name değerini sakla
                                    var getOldName = $"SELECT Name FROM {catalogValue}.EPMDocumentMaster WHERE documentNumber = @Anaparca";
                                    using (SqlCommand getNameCmd = new SqlCommand(getOldName, conn3Sel))
                                    {
                                        getNameCmd.Parameters.AddWithValue("@Anaparca", Anaparca);
                                        var oldName = (string)getNameCmd.ExecuteScalar();

                                        // Eski Name değerini Excel'e yaz
                                        excelRow["ESKI_NAME"] = oldName;


                                    }


                                }
                            }


                            if (Convert.ToString(excelRow[sutunNo - 1]) == hataNo)
                            {
                                List<string> satir = new List<string>();

                                foreach (var alan in excelRow.ItemArray)
                                {
                                    satir.Add(Convert.ToString(alan));
                                }

                                eslesenSatirlar.Add(Tuple.Create(excelFileName, excelRow.Table.Rows.IndexOf(excelRow), satir, basliklar, sutunNo));
                            }



                            //if (excelRow.Field<string>(sutunNo - 1) == hataNo)
                            //{
                            //    List<string> satir = new List<string>();

                            //    foreach (var alan in excelRow.ItemArray)
                            //    {
                            //        satir.Add(alan.ToString());
                            //    }

                            //    eslesenSatirlar.Add(Tuple.Create(excelFileName, excelRow.Table.Rows.IndexOf(excelRow), satir, basliklar, sutunNo));
                            //}

                        }


                        //foreach (DataRow row in dataTable.Rows)
                        //{
                        //    if (row.Field<string>(sutunNo - 1) == hataNo)
                        //    {
                        //        List<string> satir = new List<string>();

                        //        foreach (var alan in row.ItemArray)
                        //        {
                        //            satir.Add(alan.ToString());
                        //        }

                        //        eslesenSatirlar.Add(Tuple.Create(excelFileName, row.Table.Rows.IndexOf(row), satir, basliklar, sutunNo));
                        //    }
                        //}

                        if (eslesenSatirlar.Any())
                        {
                            result.Basliklar = basliklar;
                            result.Veriler = eslesenSatirlar.Cast<object>().ToList();
                            result.Mesaj = null;
                        }
                        else
                        {
                            result.Basliklar = null;
                            result.Veriler = null;
                            result.Mesaj = "Detay bulunamadı";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.Basliklar = null;
                result.Veriler = null;
                result.Mesaj = "HATA!" + ex.Message;
            }

            return result;
        }



    
        //public ExcelDetailResult GetExcelDetails(string excelFileName, int satirNo, int sutunNo, string hataNo)
        //{
        //    var result = new ExcelDetailResult();
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

        //                List<string> basliklar = new List<string>();
        //                List<object> eslesenSatirlar = new List<object>();

        //                // Excel dosyasının 1. satırını başlıklar olarak al
        //                var row = dataTable.Rows[0];
        //                for (int i = 0; i < dataTable.Columns.Count; i++)
        //                {
        //                    basliklar.Add(row[i].ToString());
        //                }

        //                // 1. satırı başlık olarak kabul ettiğimiz için, bu satırı atlayalım
        //                for (int i = 1; i < dataTable.Rows.Count; i++)
        //                {
        //                    row = dataTable.Rows[i];
        //                    if (row[sutunNo - 1].ToString() == hataNo)
        //                    {
        //                        List<string> satir = new List<string>();
        //                        for (int j = 0; j < dataTable.Columns.Count; j++)
        //                        {
        //                            satir.Add(row[j].ToString());
        //                        }
        //                        eslesenSatirlar.Add(satir);
        //                    }
        //                }

        //                if (eslesenSatirlar.Any())
        //                {
        //                    result.Basliklar = basliklar;
        //                    result.Veriler = eslesenSatirlar;
        //                    result.Mesaj = null;
        //                }
        //                else
        //                {
        //                    result.Basliklar = null;
        //                    result.Veriler = null;
        //                    result.Mesaj = "Detay bulunamadı";
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        result.Basliklar = null;
        //        result.Veriler = null;
        //        result.Mesaj = "HATA!" + ex.Message;
        //    }

        //    result.Veriler.Distinct();
        //    result.Basliklar.Distinct();

        //    return result;
        //}


    }
}
