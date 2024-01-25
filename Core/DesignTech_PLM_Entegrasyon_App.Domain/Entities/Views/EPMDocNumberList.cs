using System.ComponentModel.DataAnnotations;

namespace DesignTech_PLM_Entegrasyon_App.MVC.Models.Views
{
    public class EPMDocNumberList
    {
        [Key]
        public int idA2A2 { get; set; }
        public string documentNumber { get; set; }
        public string name { get; set; }
        public int latestiterationInfo { get; set; }
        public int EPMDocNumber { get; set; }
        public int versionIdA2versionInfo { get; set; }
    }
}
