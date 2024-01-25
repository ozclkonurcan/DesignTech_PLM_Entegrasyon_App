using Microsoft.AspNetCore.SignalR;

namespace DesignTech_PLM_Entegrasyon_App.MVC.Models.SignalR
{
    public class MyHub : Hub
    {
        public async Task SendMessage(string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", message);
        }
    }
}
