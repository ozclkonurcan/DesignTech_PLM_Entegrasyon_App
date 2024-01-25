using System.ComponentModel.DataAnnotations;

namespace DesignTech_PLM_Entegrasyon_App.MVC.Models
{
    public class WTUser
    {
        public bool AdministrativeLockIsNull { get; set; }
        public string TypeAdministrativeLock { get; set; }
        public bool AllowLDAPSynchronization { get; set; }
        public byte[] BlobEntrySetAdHocAcl { get; set; }
        public bool Disabled { get; set; }
        public string ClassnameKeyDomainRef { get; set; }
        public long IdA3DomainRef { get; set; }
        public string Email { get; set; }
        public string EntrySetAdHocAcl { get; set; }
        public string EventSet { get; set; }
        public string ClassnameKeyFormat { get; set; }
        public long IdA3Format { get; set; }
        public string FullName { get; set; }
        public bool InheritedDomain { get; set; }
        public bool Internal { get; set; }
        public string Last { get; set; }
        public DateTime LastSyncTime { get; set; }
        public string Name { get; set; }
        public string RemoteDirectoryServerId { get; set; }
        public bool RepairNeeded { get; set; }
        public string SecurityLabels { get; set; }
        public string Status { get; set; }
        public DateTime CreateStampA2 { get; set; }
        public long MarkForDeleteA2 { get; set; }
        public DateTime ModifyStampA2 { get; set; }
        public string ClassnameA2A2 { get; set; }
        [Key]
        public long IdA2A2 { get; set; }
        public int UpdateCountA2 { get; set; }
        public DateTime UpdateStampA2 { get; set; }
    }
}
