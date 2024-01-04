using System.ComponentModel.DataAnnotations;

namespace DesignTech_PLM_Entegrasyon_App.MVC.ViewModels
{
    public class WTPartMasterItemViewModel
    {
        [Key]
        public string ID { get; set; }
        public string Name { get; set; }
        public string SQLName { get; set; }
        public bool IsActive { get; set; }
    }
}
