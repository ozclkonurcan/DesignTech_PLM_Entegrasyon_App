namespace DesignTech_PLM_Entegrasyon_App.MVC.ViewModels
{
    public class WTProcess
    {
        public string statecheckoutInfo { get; set; }
        public long idA3masterReference { get; set; }
        public string statestate { get; set; }
        public int latestiterationInfo { get; set; }

        public byte? endItem { get; set; } // Nullable türüne sahip
        public string name { get; set; }
        public string WTPartNumber { get; set; }

        ////
        ///
        public string phaseName { get; set; }
        public string phaseState { get; set; }
        public string yuzdeOran { get; set; }
    }
}
