using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Serilog;
using System.Globalization;
using static DesignTech_PLM_Entegrasyon_App.MVC.Controllers.LogController;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Cryptography;
using ExcelDataReader;
using System.Collections.Generic;

namespace DesignTech_PLM_Entegrasyon_App.MVC.Helper
{
    public class LogService
    {
        string currentMonthFolder;
        string logFileName;

        public void ErrorFileManagement(string message)
        {
            logFileName = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "errorFile", "errorFile.json");

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



        public void AddNewLogEntry(string message)
        {
            currentMonthFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "logs", DateTime.Now.ToString("MMMM-yyyy", CultureInfo.InvariantCulture));
            string dateFormatted = DateTime.Now.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture);
            logFileName = Path.Combine(currentMonthFolder, dateFormatted + ".json");

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File(new CustomJsonFormatter(), logFileName, shared: true)
                .CreateLogger();

            Log.Information(message);
            Log.CloseAndFlush();
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
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                using (var stream = System.IO.File.Open(fileName, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        var dataSet = reader.AsDataSet();
                        var dataTable = dataSet.Tables[0];

                        List<string> basliklar = new List<string>();
                        List<Tuple<string, int, List<string>,List<string>>> eslesenSatirlar = new List<Tuple<string,int, List<string>,List<string>>>();

                        // Excel dosyasının 1. satırını başlıklar olarak al
                        var row = dataTable.Rows[0];
                        for (int i = 0; i < dataTable.Columns.Count; i++)
                        {
                            basliklar.Add(row[i].ToString());
                        }

                        for (int i = 1; i < dataTable.Rows.Count; i++)
                        {
                            row = dataTable.Rows[i];
                            if (row[sutunNo - 1].ToString() == hataNo)
                            {
                                List<string> satir = new List<string>();
                                for (int j = 0; j < dataTable.Columns.Count; j++)
                                {
                                    satir.Add(row[j].ToString());
                                }
                                eslesenSatirlar.Add(Tuple.Create(excelFileName,i, satir,basliklar));
                            }
                        }

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
