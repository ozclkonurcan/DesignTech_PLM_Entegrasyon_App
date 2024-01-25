using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DesignTech_PLM_Entegrasyon_App.MVC.Models.Templates.ProdMgmt
{
    public class Parts
    {
        public string? Context { get; set; }
        public List<Part> Value { get; set; }
    }

    public class Part
    {
        [Key]
        public string? ID { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime LastModified { get; set; }
        public string? AlternateNumber { get; set; }
        public AssemblyMode AssemblyMode { get; set; }
        public string? BOMType { get; set; }
        public string? CADName { get; set; }
        public string? CLASSIFICATION { get; set; }
        public string? CabinetName { get; set; }
        public string? ChangeStatus { get; set; }
        public string? CheckOutStatus { get; set; }
        public string? CheckoutState { get; set; }
        public string? Comments { get; set; }
        public string? ComponentType { get; set; }
        public ConfigurableModule ConfigurableModule { get; set; }
        public string? CreatedBy { get; set; }
        public DefaultTraceCode DefaultTraceCode { get; set; }
        public DefaultUnit DefaultUnit { get; set; }
        public string? DenemeNX { get; set; }
        public string? Description { get; set; }
        public bool EndItem { get; set; }
        public string? FolderLocation { get; set; }
        public string? FolderName { get; set; }
        public bool GatheringPart { get; set; }
        public string? GeneralStatus { get; set; }
        public string? Identity { get; set; }
        public string? KaleKod { get; set; }
        public string? Kaleargenumber { get; set; }
        public bool Latest { get; set; }
        public string? Length { get; set; }
        public string? LifeCycleTemplateName { get; set; }
        public double? Mass { get; set; }
        public string? Material { get; set; }
        public string? ModifiedBy { get; set; }
        public string? NAME10 { get; set; }
        public string? NAME20 { get; set; }
        public string? NAME201_PTCC_MultipleAliasAttributeValues { get; set; }
        public string? NAME201 { get; set; }
        public string? Name { get; set; }
        public string? Name30 { get; set; }
        public string? Number { get; set; }
        public List<string?> OEMPartSourcingStatus { get; set; }
        public string? ObjectType { get; set; }
        public string? OrganizationReference { get; set; }
        public string? PARCAADI { get; set; }
        public string? PTCWMNAME { get; set; }
        public bool PhantomManufacturingPart { get; set; }
        public string? Revision { get; set; }
        public string? ShareStatus { get; set; }
        public Source Source { get; set; }
        public string? SourceDuplicate { get; set; }
        public string? Standard { get; set; }
        public State State { get; set; }
        public string? Supersedes { get; set; }
        public string? Supplier { get; set; }
        public List<string?> TalepEden { get; set; }
        public string? Thickness { get; set; }
        public TypeIcon TypeIcon { get; set; }
        public string? UretimYeri { get; set; }
        public string? Version { get; set; }
        public string? VersionID { get; set; }
        public string? View { get; set; }
        public WorkInProgressState WorkInProgressState { get; set; }
    }

    public class AssemblyMode
    {
        [ForeignKey("AssemblyModeValue")]
        public string? AssemblyModeValue { get; set; }
        public string? AssemblyModeDisplay { get; set; }
        public string? Value { get; set; }
        public string? Display { get; set; }
    }

    public class ConfigurableModule
    {
        public string? Value { get; set; }
        public string? Display { get; set; }
    }

    public class DefaultTraceCode
    {
        public string? Value { get; set; }
        public string? Display { get; set; }
    }

    public class DefaultUnit
    {
        public string? Value { get; set; }
        public string? Display { get; set; }
    }

    public class Source
    {
        public string? Value { get; set; }
        public string? Display { get; set; }
    }

    public class State
    {
        public string? Value { get; set; }
        public string? Display { get; set; }
    }

    public class TypeIcon
    {
        public string? Path { get; set; }
        public string? Tooltip { get; set; }
    }

    public class WorkInProgressState
    {
        public string? Value { get; set; }
        public string? Display { get; set; }
    }
}
