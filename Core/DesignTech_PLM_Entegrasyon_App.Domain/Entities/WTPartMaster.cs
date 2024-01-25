using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;




namespace DesignTech_PLM_Entegrasyon_App.MVC.Models
{

    public class WTPartMaster
    {
        public byte collapsible { get; set; }
        public string? classnamekeycontainerReferen { get; set; }
        public long idA3containerReference { get; set; }
        public string? defaultTraceCode { get; set; }
        public string? defaultUnit { get; set; }
        public string? effCalculationStatus { get; set; }
        public byte effPropagationStop { get; set; }
        public byte endItem { get; set; }
        public string? genericType { get; set; }
        public byte hidePartInStructure { get; set; }
        public string? name { get; set; }
        public string? WTPartNumber { get; set; }
        public string? classnamekeyorganizationRefe { get; set; }
        public long idA3organizationReference { get; set; }
        //public byte phantom { get; set; }
        public string? series { get; set; }
        //public byte serviceable { get; set; }
        //public byte servicekit { get; set; }
        public DateTime createStampA2 { get; set; }
        public long markForDeleteA2 { get; set; }
        public DateTime modifyStampA2 { get; set; }
        public string? classnameA2A2 { get; set; }
        [Key]
        public long idA2A2 { get; set; }
        public int updateCountA2 { get; set; }
        public DateTime updateStampA2 { get; set; }
        //public string? authoringLanguagetranslation { get; set; }
        //public byte WT_FBI_COMPUTE_0_0 { get; set; }
        //public byte WT_FBI_COMPUTE_1_0 { get; set; }
        //public byte WT_FBI_COMPUTE_2_0 { get; set; }
    }
}
