
using Dapper;
using DesignTech_PLM_Entegrasyon_App.MVC.Models.SignalR;
using Microsoft.Extensions.Configuration;
using System.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DesignTech_PLM_Entegrasyon_App.MVC.Helper
{
    public class ChangeNoticeService : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IDbConnection _db;

        public ChangeNoticeService(IConfiguration configuration, IDbConnection db)
        {
            _configuration = configuration;
            _db = db;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var catalogValue = _configuration["Catalog"];
            var connectionString = _configuration.GetConnectionString("Plm");

            LogService logService = new LogService(_configuration);

            while (!stoppingToken.IsCancellationRequested)
            {
                var sql = $"SELECT [CN_NUMBER], [CHANGE_NOTICE], [STATE], [LastUpdateTimestamp] FROM {catalogValue}.dbo.Change_Notice WHERE [STATE] = 'RESOLVED'";

                var resolvedItems = await _db.QueryAsync<WTChangeOrder2Master>(sql);

                foreach (var item in resolvedItems)
                {
                    // Check if the CN_NUMBER already exists in Change_Notice_LogTable
                    var existingLog = await _db.QuerySingleOrDefaultAsync<WTChangeOrder2Master>(
                        $"SELECT [CN_NUMBER], [ProcessTimestamp], [LastUpdateTimestamp] FROM [dbo].[Change_Notice_LogTable] WHERE [CN_NUMBER] = @CN_NUMBER",
                        new { CN_NUMBER = item.CN_NUMBER });

                    if (existingLog == null)
                    {
                        // If CN_NUMBER doesn't exist, insert a new log entry
                        await _db.ExecuteAsync(
                            $"INSERT INTO [dbo].[Change_Notice_LogTable] ([CN_NUMBER], [ProcessTimestamp], [LastUpdateTimestamp]) VALUES (@CN_NUMBER, @ProcessTimestamp, @LastUpdateTimestamp)",
                            new { CN_NUMBER = item.CN_NUMBER, ProcessTimestamp = DateTime.UtcNow, LastUpdateTimestamp = item.LastUpdateTimestamp });

                        logService.AddNewLogEntry($"{item.CN_NUMBER} 'ı aktarma İşlemi gerçekleştirildi", null, "Post Edildi", null);
                    }
                    else
                    {
                        // If CN_NUMBER exists, check if LastUpdateTimestamp has changed
                        if (existingLog.LastUpdateTimestamp != item.LastUpdateTimestamp)
                        {
                            // If LastUpdateTimestamp has changed, update the log entry
                            await _db.ExecuteAsync(
                                $"UPDATE [dbo].[Change_Notice_LogTable] SET [ProcessTimestamp] = @ProcessTimestamp, [LastUpdateTimestamp] = @LastUpdateTimestamp WHERE [CN_NUMBER] = @CN_NUMBER",
                                new { CN_NUMBER = item.CN_NUMBER, ProcessTimestamp = DateTime.UtcNow, LastUpdateTimestamp = item.LastUpdateTimestamp });

                            logService.AddNewLogEntry($"{item.CN_NUMBER} 'ın güncellenmiş tarihi loglandı", null, "Post Edildi", null);
                        }
                        else
                        {
                            // If LastUpdateTimestamp has not changed, do nothing
                            //logService.AddNewLogEntry($"{item.CN_NUMBER} 'ın tarihi değişmedi, işlem yapılmadı", null, "Post Edilmedi", null);
                        }
                    }
                }

                await Task.Delay(10000, stoppingToken);
            }
        }

        //protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        //{
        //    var catalogValue = _configuration["Catalog"];
        //    var connectionString = _configuration.GetConnectionString("Plm");
        //    while (!stoppingToken.IsCancellationRequested)
        //    {
        //        var sql = $"SELECT [CN_NUMBER], [CHANGE_NOTICE], [STATE] FROM {catalogValue}.dbo.Change_Notice";

        //        var resolvedItems = await _db.QueryAsync<WTChangeOrder2Master>(sql);

        //        LogService logService = new LogService(_configuration);

        //        foreach (var item in resolvedItems.Where(x => x.STATE == "RESOLVED"))
        //        {

        //            logService.AddNewLogEntry(item.CN_NUMBER+" 'ı aktarma İşlemi gerçekleştirildi",null, "Post Edildi",null);

        //            // Burada CN_NUMBER'ı tutmak için kodlarımız
        //        }

        //        await Task.Delay(10000, stoppingToken);
        //    }
        //}
    }
}



////Kontol için kullancağımız table bu bunu otomatik sql ayarları ile birlite ekleyebiliriz çalışırsa 
//CREATE TABLE [dbo].[Change_Notice_LogTable](
//    [CN_NUMBER] [varchar](50) NOT NULL PRIMARY KEY,
//    [CHANGE_NOTICE] [varchar](max), -- CHANGE_NOTICE tipi uygunsa, max boyutunu ayarlayabilirsiniz
//    [STATE] [varchar](max), -- STATE tipi uygunsa, max boyutunu ayarlayabilirsiniz
//    [LastUpdateTimestamp] [datetime] NOT NULL,
//    [ProcessTimestamp] [datetime] NOT NULL
//);
