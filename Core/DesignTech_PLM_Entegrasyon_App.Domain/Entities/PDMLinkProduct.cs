using System.ComponentModel.DataAnnotations;

namespace DesignTech_PLM_Entegrasyon_App.MVC.Models
{
    public class PDMLinkProduct
    {
        public byte AdministrativeLockIsNull { get; set; }
        public string TypeAdministrativeLock { get; set; }
        public byte[] BlobEntrySetAdHocAcl { get; set; }
        public byte AdministratorsRefIsNullContainer { get; set; }
        public string ClassNameKeyA2ContainerInfo { get; set; }
        public long IdA3A2ContainerInfo { get; set; }
        public byte BusinessNamespaceContainerInfo { get; set; }
        public string ClassNameKeyB2ContainerInfo { get; set; }
        public long IdA3B2ContainerInfo { get; set; }
        public byte DefaultCabinetRefIsNullContainer { get; set; }
        public string ClassNameKeyC2ContainerInfo { get; set; }
        public long IdA3C2ContainerInfo { get; set; }
        public byte DefaultDomainRefIsNullContainer { get; set; }
        public string ClassNameKeyD2ContainerInfo { get; set; }
        public long IdA3D2ContainerInfo { get; set; }
        public string DescriptionContainerInfo { get; set; }
        public string NameContainerInfo { get; set; }
        public byte OwnerRefIsNullContainerInfo { get; set; }
        public string ClassNameKeyE2ContainerInfo { get; set; }
        public long IdA3E2ContainerInfo { get; set; }
        public byte PrivateAccessContainerInfo { get; set; }
        public byte PublicParentDomainRefIsNullContainer { get; set; }
        public string ClassNameKeyF2ContainerInfo { get; set; }
        public long IdA3F2ContainerInfo { get; set; }
        public byte SharingEnabledContainerInfo { get; set; }
        public byte SystemCabinetRefIsNullContainer { get; set; }
        public string ClassNameKeyG2ContainerInfo { get; set; }
        public long IdA3G2ContainerInfo { get; set; }
        public byte SystemDomainRefIsNullContainer { get; set; }
        public string ClassNameKeyH2ContainerInfo { get; set; }
        public long IdA3H2ContainerInfo { get; set; }
        public string ClassNameKeyContainerReference { get; set; }
        public long IdA3ContainerReference { get; set; }
        public byte ActiveFlagContainerTeamManaged { get; set; }
        public DateTime ActualEndContainerTeamManaged { get; set; }
        public DateTime ActualStartContainerTeamManaged { get; set; }
        public byte AllowUserToConfigureAccessContainer { get; set; }
        public byte ContainerTeamIdIsNullContainer { get; set; }
        public string ClassNameKeyA2ContainerTeamManagedInfo { get; set; }
        public long IdA3A2ContainerTeamManagedInfo { get; set; }
        public byte ExtendableContainerTeamManaged { get; set; }
        public string InvitationMsgContainerTeamManaged { get; set; }
        public byte SendInvitationsContainerTeam { get; set; }
        public byte SharedTeamIdIsNullContainerTeam { get; set; }
        public string ClassNameKeyB2ContainerTeamManagedInfo { get; set; }
        public long IdA3B2ContainerTeamManagedInfo { get; set; }
        public string StateContainerTeamManagedInfo { get; set; }
        public string ClassNameKeyContainerTemplate { get; set; }
        public long IdA3ContainerTemplateReference { get; set; }
        public string ClassNameKeyDomainReference { get; set; }
        public long IdA3DomainReference { get; set; }
        public string EntrySetAdHocAcl { get; set; }
        public string EventSet { get; set; }
        public string IndexersIndexerSet { get; set; }
        public byte InheritedDomain { get; set; }
        public string ClassNameKeyOrganizationReference { get; set; }
        public long IdA3OrganizationReference { get; set; }
        public string ClassNameKeyA7 { get; set; }
        public long IdA3A7 { get; set; }
        public string SecurityLabels { get; set; }
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
        public byte WT_FBI_COMPUTE_U_0_1 { get; set; }
        public byte WT_FBI_COMPUTE_4_0 { get; set; }
    }
}
