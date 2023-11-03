using System.ComponentModel.DataAnnotations;

namespace DesignTech_PLM_Entegrasyon_App.MVC.ViewModels
{
    public class IntegrationDefinitionListViewModel
    {
        public string displayName { get; set; }
        public string hierarchyID { get; set; }
        public string classnameA2A2 { get; set; }
        [Key]
        public int idA2A2 { get; set; }
        public string type { get; set; }
    }
}
