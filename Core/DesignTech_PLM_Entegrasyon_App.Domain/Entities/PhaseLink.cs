using System.ComponentModel.DataAnnotations;

namespace DesignTech_PLM_Entegrasyon_App.MVC.Models
{
    public class PhaseLink
    {
        public string? classnameKeyRoleAObjectRef { get; set; }
        public long idA3A5 { get; set; }
        public string? classnameKeyRoleBObjectRef { get; set; }
        public long idA3B5 { get; set; }
        public DateTime createStampA2 { get; set; }
        public long? markForDeleteA2 { get; set; }
        public DateTime modifyStampA2 { get; set; }
        public string? classnameA2A2 { get; set; }
        [Key]
        public long? idA2A2 { get; set; }
        public int updateCountA2 { get; set; }
        public DateTime updateStampA2 { get; set; }
    }
}
