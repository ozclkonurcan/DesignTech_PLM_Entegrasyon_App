using System.ComponentModel.DataAnnotations;

namespace DesignTech_PLM_Entegrasyon_App.MVC.Models.LogTable
{
    public class WTPartAlternateLink_LOG
    {
        public string? TransferID { get; set; }
        public string? ID { get; set; }
        public string? ObjectType { get; set; }
        public string? Name { get; set; }
        public string? Number { get; set; }
        public string? State { get; set; }
        public DateTime updateStampA2 { get; set; }
        public DateTime modifyStampA2 { get; set; }
        public DateTime ProcessTimestamp { get; set; }
    }
}
