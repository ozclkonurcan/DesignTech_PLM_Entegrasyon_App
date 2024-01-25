using Microsoft.AspNetCore.SignalR;

namespace DesignTech_PLM_Entegrasyon_App.MVC.Models.SignalR
{
    public class StatusHub: Hub
    {
        public async Task SendUpdate(string message)
        {
            await Clients.All.SendAsync("ReceiveDocuments", message);
        }
    }




}
