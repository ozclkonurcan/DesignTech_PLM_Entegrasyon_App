namespace DesignTech_PLM_Entegrasyon_App.MVC.Models
{
    public class SablonViewModel
    {
        public string? ID { get; set; }
        public string? SablonName { get; set; }
        public string? sablonDataDurumu { get; set; }
        public List<WTPartDataSettingsViewModel> SablonDataList { get; set; }
    }
}
