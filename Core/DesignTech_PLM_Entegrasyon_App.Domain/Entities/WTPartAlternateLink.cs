using System.ComponentModel.DataAnnotations;

namespace DesignTech_PLM_Entegrasyon_App.MVC.Models
{
    public class WTPartAlternateLink
    {
        public byte? AdministrativeLockIsNull { get; set; }
        public string TypeAdministrativeLock { get; set; }
        public string ClassNameKeyDomainRef { get; set; }
        public long IdA3DomainRef { get; set; }
        public byte? InheritedDomain { get; set; }
        public string ReplacementType { get; set; }
        public string ClassNameKeyRoleAObjectRef { get; set; }
        public long IdA3A5 { get; set; }
        public string ClassNameKeyRoleBObjectRef { get; set; }
        public long IdA3B5 { get; set; }
        public string SecurityLabels { get; set; }
        public DateTime CreateStampA2 { get; set; }
        public long MarkForDeleteA2 { get; set; }
        public DateTime ModifyStampA2 { get; set; }
        public string ClassNameA2A2 { get; set; }
        [Key]
        public long IdA2A2 { get; set; }
        public int UpdateCountA2 { get; set; }
        public DateTime UpdateStampA2 { get; set; }
    }
}
