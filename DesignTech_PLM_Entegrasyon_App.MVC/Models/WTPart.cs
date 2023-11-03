using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DesignTech_PLM_Entegrasyon_App.MVC.Models
{
    public class WTPart
    {
        //public string? adhocVersionInfo { get; set; } = null;
        //public byte administrativeLockIsNull { get; set; } = 0;
        //public string? typeAdministrativeLock { get; set; } = null;
        //[Column("blob$entrySetadHocAcl")]
        //public byte[]? blobentrySetAdHocAcl { get; set; } = null;
        //[Column("blob$expressionData")]
        //public byte[]? blobexpressionData { get; set; } = null;
        public byte checkoutInfoIsNull { get; set; } = 0;
        public string? stateCheckoutInfo { get; set; } = null;
        public string? classnamekeycontainerReferen { get; set; } = null;
        public long idA3ContainerReference { get; set; } = 0;
        //public string? contractNumber { get; set; } = null;
        public string? classnameKeyDomainRef { get; set; } = null;
        public long idA3DomainRef { get; set; } = 0;
        //public string? entrySetAdHocAcl { get; set; } = null;
        //public string? eventSet { get; set; } = null;
        //public string? expressionData { get; set; } = null;
        public string? classnameKeyA2FolderingInfo { get; set; } = null;
        public long idA3A2FolderingInfo { get; set; } = 0;
        public string? classnameKeyB2FolderingInfo { get; set; } = null;
        public long idA3B2FolderingInfo { get; set; } = 0;
        public byte hasHangingChange { get; set; } = 0;
        public byte hasPendingChange { get; set; } = 0;
        public byte hasResultingChange { get; set; } = 0;
        public byte hasSuspect { get; set; } = 0;
        public byte hasVariance { get; set; } = 0;
        //public string? indexersIndexerSet { get; set; } = null;
        public byte inheritedDomain { get; set; } = 0;
        public string? iopStateInteropInfo { get; set; } = null;
        public string? stateInteropInfo { get; set; } = null;
        public long branchIdIterationInfo { get; set; } = 0;
        public string? classnameKeyD2IterationInfo { get; set; } = null;
        public long idA3D2IterationInfo { get; set; } = 0;
        //public string? classnameKeyE2IterationInfo { get; set; } = null;
        public long idA3E2IterationInfo { get; set; } = 0;
        public string? iterationIdA2IterationInfo { get; set; } = null;
        public byte latestIterationInfo { get; set; } = 0;
        public string? classnameKeyB2IterationInfo { get; set; } = null;
        public long idA3B2IterationInfo { get; set; } = 0;
        //public string? noteIterationInfo { get; set; } = null;
        //public string? classnameKeyC2IterationInfo { get; set; } = null;
        public long idA3C2IterationInfo { get; set; } = 0;
        public string? stateIterationInfo { get; set; } = null;
        //public string? jobAuthorizationNumber { get; set; } = null;
        //public DateTime dateLock { get; set; } = DateTime.MinValue;
        //public string? classnameKeyA2Lock { get; set; } = null;
        public long idA3A2Lock { get; set; } = 0;
        public string? noteLock { get; set; } = null;
        public string? classnameKeyMasterReference { get; set; } = null;
        public long idA3MasterReference { get; set; } = 0;
        //public int maximumAllowed { get; set; } = 0;
        //public int minimumRequired { get; set; } = 0;
        //public string? oneOffVersionIdA2oneOffVersi { get; set; } = null;
        //public string? classnameKeyA2Ownership { get; set; } = null;
        public long idA3A2Ownership { get; set; } = 0;
        public string? partType { get; set; } = null;
        //public string? phase { get; set; } = null;
        //public string? securityLabels { get; set; } = null;
        public string? source { get; set; } = null;
        public byte atGateState { get; set; } = 0;
        public string? classnameKeyA2State { get; set; } = null;
        public long idA3A2State { get; set; } = 0;
        public string? stateState { get; set; } = null;
        //public byte teamIdIsNull { get; set; } = 0;
        //public string? classnameKeyTeamId { get; set; } = null;
        //public long idA3TeamId { get; set; } = 0;
        //public byte teamTemplateIdIsNull { get; set; } = 0;
        //public string? classnameKeyTeamTemplateId { get; set; } = null;
        //public long idA3TeamTemplateId { get; set; } = 0;
        public DateTime createStampA2 { get; set; } = DateTime.MinValue;
        public long? markForDeleteA2 { get; set; } = null;
        public DateTime modifyStampA2 { get; set; } = DateTime.MinValue;
        public string? classnameA2A2 { get; set; } = null;
        [Key]
        public long? idA2A2 { get; set; } = null;
        public int updateCountA2 { get; set; } = 0;
        public DateTime updateStampA2 { get; set; } = DateTime.MinValue;
        public byte validateUsage { get; set; } = 0;
        //public string? variation1 { get; set; } = null;
        //public string? variation2 { get; set; } = null;
        public string? versionIdA2VersionInfo { get; set; } = null;
        public int versionLevelA2VersionInfo { get; set; } = 0;
        public string? versionSortIdA2VersionInfo { get; set; } = null;
        public byte viewIsNull { get; set; } = 0;
        public long idA3View { get; set; } = 0;
        //      public byte? WT_FBI_COMPUTE_U_0_3 { get; set; }
        //public byte? WT_FBI_COMPUTE_U_0_4 { get; set; }
        //public byte? WT_FBI_COMPUTE_3_1 { get; set; }
        //public byte? WT_FBI_COMPUTE_3_2 { get; set; }
    }
}
