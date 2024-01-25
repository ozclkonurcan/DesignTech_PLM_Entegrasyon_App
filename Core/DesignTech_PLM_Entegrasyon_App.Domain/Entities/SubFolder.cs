using System.ComponentModel.DataAnnotations;

namespace DesignTech_PLM_Entegrasyon_App.MVC.Models
{
    public class SubFolder
    {
        public bool AdministrativeLockIsNull { get; set; }
        public string TypeAdministrativeLock { get; set; }
        public byte[] BlobEntrySetAdHocAcl { get; set; }
        public string ClassnameKeyContainerReference { get; set; }
        public long IdA3ContainerReference { get; set; }
        public string ClassnameKeyB7 { get; set; }
        public long IdA3B7 { get; set; }
        public string Description { get; set; }
        public string ClassnameKeyDomainRef { get; set; }
        public long IdA3DomainRef { get; set; }
        public string EntrySetAdHocAcl { get; set; }
        public string EventSet { get; set; }
        public string ClassnameKeyA2FolderingInfo { get; set; }
        public long IdA3A2FolderingInfo { get; set; }
        public string ClassnameKeyB2FolderingInfo { get; set; }
        public long IdA3B2FolderingInfo { get; set; }
        public string IndexersIndexerSet { get; set; }
        public bool InheritedDomain { get; set; }
        public string Name { get; set; }
        public string ClassnameKeyA2Ownership { get; set; }
        public long IdA3A2Ownership { get; set; }
        public string SecurityLabels { get; set; }
        public DateTime CreateStampA2 { get; set; }
        public long MarkForDeleteA2 { get; set; }
        public DateTime ModifyStampA2 { get; set; }
        public string ClassnameA2A2 { get; set; }
        [Key]
        public long IdA2A2 { get; set; }
        public int UpdateCountA2 { get; set; }
        public DateTime UpdateStampA2 { get; set; }
        public long BranchIdA2TypeDefinitionReference { get; set; }
        public long IdA2TypeDefinitionReference { get; set; }
    }
}
