using DesignTech_PLM_Entegrasyon_App.MVC.Models.LogTable;
using Microsoft.AspNetCore.SignalR;

namespace DesignTech_PLM_Entegrasyon_App.MVC.Models.SignalR
{
    public class DataHub : Hub
    {
        public async Task SendData(List<Change_Notice_LogTable> data)
        {
            await Clients.All.SendAsync("ReceiveData", data);
        }
    }
}
