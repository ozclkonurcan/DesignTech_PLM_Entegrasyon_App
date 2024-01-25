using System.ComponentModel.DataAnnotations;

namespace DesignTech_PLM_Entegrasyon_App.MVC.Models
{
    public class PhaseTemplate
    {
        public byte finalPhase { get; set; }
        public string? classnameKeyA7 { get; set; }
        public long idA3A7 { get; set; }
        public string? name { get; set; }
        public string? phaseState { get; set; }
        public string? classnameKeyB7 { get; set; }
        public long idA3B7 { get; set; }
        public byte[]? roleActorRoleMap { get; set; }
        public byte[]? rolePrincipalMap { get; set; }
        public byte[]? roleRoleMap { get; set; }
        public string? seriesSelector { get; set; }
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
