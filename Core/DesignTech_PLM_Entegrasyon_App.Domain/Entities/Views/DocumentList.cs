using System.ComponentModel.DataAnnotations;

namespace DesignTech_PLM_Entegrasyon_App.MVC.Models.Views
{
    public class DocumentList
    {
        [Key]
        public int idA2A2 { get; set; }
        public string WTDocumentNumber { get; set; }
        public string name { get; set; }
        public string statestate { get; set; }
        public PDMLinkProduct idA3containerReference { get; set; }
        public string containerInfo { get; set; }
        public SubFolder idA3B2folderingInfo { get; set; }
        public string EXPR1 { get; set; }
        public WTUser idA3D2iterationInfo { get; set; }
        public string fullName { get; set; }
        public int latestiterationInfo { get; set; }
        public WTUser idA3B2iterationInfo { get; set; }
        public DateTime modifyStampA2 { get; set; }
        public string EXPR2 { get; set; }
        public int versionIdA2versionInfo { get; set; }
        public int versionLevelA2versionInfo { get; set; }
        public string Version { get; set; }
        public int idA3C2iterationInfo { get; set; }
    }
}
