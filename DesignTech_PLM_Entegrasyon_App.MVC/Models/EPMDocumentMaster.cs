using System.ComponentModel.DataAnnotations;

namespace DesignTech_PLM_Entegrasyon_App.MVC.Models
{
    public class EPMDocumentMaster
    {
        public string CADName { get; set; }
        public string AuthoringApplication { get; set; }
        public byte Collapsible { get; set; }
        public string ClassNameKeyContainerReference { get; set; }
        public long IdA3ContainerReference { get; set; }
        public string DefaultUnit { get; set; }
        public string DocSubType { get; set; }
        public string DocType { get; set; }
        public string GenericType { get; set; }
        public string GlobalID { get; set; }
        public string Name { get; set; }
        public string DocumentNumber { get; set; }
        public string ClassNameKeyOrganizationReference { get; set; }
        public long IdA3OrganizationReference { get; set; }
        public string OwnerApplication { get; set; }
        public string Series { get; set; }
        public DateTime CreateStampA2 { get; set; }
        public long MarkForDeleteA2 { get; set; }
        public DateTime ModifyStampA2 { get; set; }
        public string ClassNameA2A2 { get; set; }
        [Key]
        public long IdA2A2 { get; set; }
        public int UpdateCountA2 { get; set; }
        public DateTime UpdateStampA2 { get; set; }
        public long BranchIdA2TypeDefinitionReference { get; set; }
        public long IdA2TypeDefinitionReference { get; set; }
        public string WT_FBI_COMPUTE_U_0_0 { get; set; }
        public string WT_FBI_COMPUTE_0_0 { get; set; }
        public string WT_FBI_COMPUTE_1_0 { get; set; }
        public string WT_FBI_COMPUTE_2_0 { get; set; }
        public string WT_FBI_COMPUTE_3_0 { get; set; }
        public string WT_FBI_COMPUTE_8_0 { get; set; }
    }
}
