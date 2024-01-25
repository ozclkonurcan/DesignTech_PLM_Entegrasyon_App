namespace DesignTech_PLM_Entegrasyon_App.MVC.Models
{
    public class HolderToContent
    {
        public string classnamekeyroleAObjectRef { get; set; }
        public long idA3A5 { get; set; }
        public string classnamekeyroleBObjectRef { get; set; }
        public long idA3B5 { get; set; }
        public DateTime createStampA2 { get; set; }
        public long markForDeleteA2 { get; set; } // Buraya uygun bir tür belirleyin (örneğin long? ya da bool?)
        public DateTime modifyStampA2 { get; set; }
        public string classnameA2A2 { get; set; }
        public long idA2A2 { get; set; } // Buraya uygun bir tür belirleyin (örneğin long? ya da int?)
        public int updateCountA2 { get; set; }
        public DateTime updateStampA2 { get; set; }
    }
}
