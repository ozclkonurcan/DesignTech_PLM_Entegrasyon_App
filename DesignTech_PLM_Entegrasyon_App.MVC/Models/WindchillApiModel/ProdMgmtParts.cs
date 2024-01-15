namespace DesignTech_PLM_Entegrasyon_App.MVC.Models.WindchillApiModel
{
    public class ProdMgmtParts
    {
		public string Context { get; set; }
		public List<Part> Value { get; set; }
    }

	public class Part
    {
        public DateTime CreatedOn { get; set; }
        public string ID { get; set; }
        public DateTime LastModified { get; set; }
        public string AlternateNumber { get; set; }
        public AssemblyMode AssemblyMode { get; set; }
        public string BOMType { get; set; }
        public string CADName { get; set; }
        public object CLASSIFICATION { get; set; }
        public string CabinetName { get; set; }
        public object ChangeStatus { get; set; }
        public string CheckOutStatus { get; set; }
        public string CheckoutState { get; set; }
        public object Comments { get; set; }
        public object ComponentType { get; set; }
        public ConfigurableModule ConfigurableModule { get; set; }
        public string CreatedBy { get; set; }
        public DefaultTraceCode DefaultTraceCode { get; set; }
        public DefaultUnit DefaultUnit { get; set; }
        public object DenemeNX { get; set; }
        public object Description { get; set; }
        public bool EndItem { get; set; }
        public string FolderLocation { get; set; }
        public string FolderName { get; set; }
        public bool GatheringPart { get; set; }
        public object GeneralStatus { get; set; }
        public string Identity { get; set; }
        public object KaleKod { get; set; }
        public object Kaleargenumber { get; set; }
        public bool Latest { get; set; }
        public object Length { get; set; }
        public string LifeCycleTemplateName { get; set; }
        public object Mass { get; set; }
        public object Material { get; set; }
        public string ModifiedBy { get; set; }
        public object NAME10 { get; set; }
        public object NAME20 { get; set; }
        public string NAME201_PTCC_MultipleAliasAttributeValues { get; set; }
        public object NAME201 { get; set; }
        public string Name { get; set; }
        public object Name30 { get; set; }
        public string Number { get; set; }
        public List<object> OEMPartSourcingStatus { get; set; }
        public string ObjectType { get; set; }
        public object OrganizationReference { get; set; }
        public object PARCAADI { get; set; }
        public object PTCWMNAME { get; set; }
        public bool PhantomManufacturingPart { get; set; }
        public string Revision { get; set; }
        public object ShareStatus { get; set; }
        public Source Source { get; set; }
        public object SourceDuplicate { get; set; }
        public object Standard { get; set; }
        public State State { get; set; }
        public object Supersedes { get; set; }
        public object Supplier { get; set; }
        public List<object> TalepEden { get; set; }
        public object Thickness { get; set; }
        public TypeIcon TypeIcon { get; set; }
        public object UretimYeri { get; set; }
        public string Version { get; set; }
        public string VersionID { get; set; }
        public string View { get; set; }
        public WorkInProgressState WorkInProgressState { get; set; }
    }


    public class AssemblyMode
    {
        public string AssemblyModeValue { get; set; }
        public string AssemblyModeDisplay { get; set; }
        //public string Value { get; set; }
        //public string Display { get; set; }
    }

    public class ConfigurableModule
    {
        public string Value { get; set; }
        public string Display { get; set; }
    }

    public class DefaultTraceCode
    {
        public string Value { get; set; }
        public string Display { get; set; }
    }

    public class DefaultUnit
    {
        public string Value { get; set; }
        public string Display { get; set; }
    }

    public class Source
    {
        public string Value { get; set; }
        public string Display { get; set; }
    }

    public class State
    {
        public string Value { get; set; }
        public string Display { get; set; }
    }

    public class TypeIcon
    {
        public string Path { get; set; }
        public string Tooltip { get; set; }
    }

    public class WorkInProgressState
    {
        public string Value { get; set; }
        public string Display { get; set; }
    }
}
