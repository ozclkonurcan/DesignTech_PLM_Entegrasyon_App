using System.ComponentModel.DataAnnotations;

namespace DesignTech_PLM_Entegrasyon_App.MVC.Models.Views
{
    public class WTPartNoList
    {
        [Key]
        public int idA2A2 { get; set; }
        public string WTPartNumber { get; set; }
        public int versionIdA2versionInfo { get; set; }
        public string name { get; set; }
        public int latestiterationInfo { get; set; }
        public int WTPartNo { get; set; }
    }
}
