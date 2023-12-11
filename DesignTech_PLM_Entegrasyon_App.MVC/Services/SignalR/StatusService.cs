using DesignTech_PLM_Entegrasyon_App.MVC.Helper;
using DesignTech_PLM_Entegrasyon_App.MVC.Models.SignalR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;

namespace DesignTech_PLM_Entegrasyon_App.MVC.Services.SignalR
{
    public class StatusService : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IHubContext<StatusHub> _hub;

        private readonly ChangeNoticeRepository _repo;

        public StatusService(IHubContext<StatusHub> hub,
                      ChangeNoticeRepository repo)
        {
            _hub = hub;
            _repo = repo;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {

            LogService logService = new LogService(_configuration);
            //var loggedInUsername = HttpContext.User.Identity.Name;
            while (!cancellationToken.IsCancellationRequested)
            {
                var resolvedCNs = _repo.GetResolvedCNs();

                foreach (var cn in resolvedCNs)
                {
                    logService.AddNewLogEntry(cn.CN_NUMBER+"değeri x apisine Gönderildi", null, "İşlem Başarılı", "Name");

                    await _hub.Clients.All.SendAsync("cnStatusChanged",
                        $"CN: {cn.CN_NUMBER} resolved");
                }

                await Task.Delay(5000);
            }
        }
    }
}
