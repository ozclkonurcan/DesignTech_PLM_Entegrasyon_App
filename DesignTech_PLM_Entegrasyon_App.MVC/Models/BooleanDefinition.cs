using System.ComponentModel.DataAnnotations;

namespace DesignTech_PLM_Entegrasyon_App.MVC.Models
{
    public class BooleanDefinition
    {
        public byte AdministrativeLockIsNull { get; set; }
        public string TypeAdministrativeLock { get; set; }
        public string CounterpartID { get; set; }
        public string ClassnameKeyDefinitionParent { get; set; }
        public long IdA3DefinitionParentReference { get; set; }
        public string Description { get; set; }
        public string DisplayName { get; set; }
        public string ClassnameKeyDomainReference { get; set; }
        public long IdA3DomainReference { get; set; }
        public string HierarchyDisplayName { get; set; }
        public string HierarchyID { get; set; }
        public byte InheritedDomain { get; set; }
        public string Name { get; set; }
        public string SecurityLabels { get; set; }
        public int SiblingID { get; set; }
        public DateTime CreateStampA2 { get; set; }
        public long MarkForDeleteA2 { get; set; }
        public DateTime ModifyStampA2 { get; set; }
        public string ClassnameA2A2 { get; set; }
        [Key]
        public long IdA2A2 { get; set; }
        public int UpdateCountA2 { get; set; }
        public DateTime UpdateStampA2 { get; set; }
        public string CounterpartIDWC { get; set; }
    }
}
