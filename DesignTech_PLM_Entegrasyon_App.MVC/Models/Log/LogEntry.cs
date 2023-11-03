namespace DesignTech_PLM_Entegrasyon_App.MVC.Models.Log
{
   public class LogEntry
{
        public string ExcelAdi { get; set; }
        public string Hata { get; set; }
    public int Satir { get; set; }
    public int Sutun { get; set; }
    public bool Durum { get; set; }
        public DateTime Tarih { get; set; } = DateTime.Now;
    }
}
