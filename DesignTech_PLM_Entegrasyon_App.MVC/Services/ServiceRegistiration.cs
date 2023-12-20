using DesignTech_PLM_Entegrasyon_App.MVC.Helper;

namespace DesignTech_PLM_Entegrasyon_App.MVC.Services
{
    public static class ServiceRegistiration
    {
        public static void AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            try
            {
                services.AddHostedService<ChangeNoticeService>();
            }
            catch (Exception ex)
            {
                LogService logService = new LogService(configuration);
                logService.AddNewLogEntry("Auto Post Aktif edilemedi: " + ex.Message, null, "Auto Post Aktif Değil", null);
            }
        }
    }
}
