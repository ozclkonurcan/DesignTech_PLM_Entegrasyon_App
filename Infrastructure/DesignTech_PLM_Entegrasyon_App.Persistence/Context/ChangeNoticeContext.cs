using DesignTech_PLM_Entegrasyon_App.MVC.Models.SignalR;
using Microsoft.EntityFrameworkCore;

namespace DesignTech_PLM_Entegrasyon_App.MVC.Dtos
{
    public class ChangeNoticeContext : DbContext
    {
        public ChangeNoticeContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<WTChangeOrder2Master> ChangeNotices { get; set; }
    }
}
