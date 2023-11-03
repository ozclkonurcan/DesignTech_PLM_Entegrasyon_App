using Serilog;
using System.Globalization;

namespace DesignTech_PLM_Entegrasyon_App.MVC.Helper
{
    public class LogService
    {
        public void AddNewLogEntry(string message)
        {

            var currentMonthFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "logs", DateTime.Now.ToString("MMMM-yyyy", CultureInfo.InvariantCulture));
            var logFileName = Path.Combine(currentMonthFolder, DateTime.Now.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture) + ".json");


            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File(new CustomJsonFormatter(), logFileName, rollingInterval: RollingInterval.Day, shared: true)
                .CreateLogger();

            Log.Information(message);
            Log.CloseAndFlush();
        }

    }
}
