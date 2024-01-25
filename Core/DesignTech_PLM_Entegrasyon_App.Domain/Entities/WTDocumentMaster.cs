using System.ComponentModel.DataAnnotations;

namespace DesignTech_PLM_Entegrasyon_App.MVC.Models
{
    public class WTDocumentMaster
    {
        public string ClassnameKeyContainerReference { get; set; }
        public long IdA3ContainerReference { get; set; }
        public string DocType { get; set; }
        public string Name { get; set; }
        public string WTDocumentNumber { get; set; }
        public string ClassnameKeyOrganizationReference { get; set; }
        public long IdA3OrganizationReference { get; set; }
        public string Series { get; set; }
        public DateTime CreateStampA2 { get; set; }
        public long MarkForDeleteA2 { get; set; }
        public DateTime ModifyStampA2 { get; set; }
        public string ClassnameA2A2 { get; set; }
        [Key]
        public long IdA2A2 { get; set; }
        public int UpdateCountA2 { get; set; }
        public DateTime UpdateStampA2 { get; set; }
        public byte WT_FBI_COMPUTE_0_0 { get; set; }
        public byte WT_FBI_COMPUTE_2_0 { get; set; }
        public byte WT_FBI_COMPUTE_3_0 { get; set; }
    }
}
