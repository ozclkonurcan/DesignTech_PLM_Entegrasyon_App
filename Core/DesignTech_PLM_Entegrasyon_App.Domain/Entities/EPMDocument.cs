﻿using System.ComponentModel.DataAnnotations;

namespace DesignTech_PLM_Entegrasyon_App.MVC.Models
{
    public class EPMDocument
    {
        public bool AdministrativeLockIsNull { get; set; }
        public string TypeAdministrativeLock { get; set; }
        public string ClassnameKeyC10 { get; set; }
        public long IdA3C10 { get; set; }
        public byte AuthoringAppVersion { get; set; }
        public string ClassnameKeyB10 { get; set; }
        public long IdA3B10 { get; set; }
        public byte[] BlobEntrySetAdHocAcl { get; set; }
        public byte[] BlobExpressionData { get; set; }
        public bool BoxExtentsIsNull { get; set; }
        public float AxD10 { get; set; }
        public float AyD10 { get; set; }
        public float AzD10 { get; set; }
        public float BxD10 { get; set; }
        public float ByD10 { get; set; }
        public float BzD10 { get; set; }
        public bool CheckoutInfoIsNull { get; set; }
        public string StateCheckoutInfo { get; set; }
        public string ClassnameKeyContainerReference { get; set; }
        public long IdA3ContainerReference { get; set; }
        public int DbKeySize { get; set; }
        public bool Derived { get; set; }
        public string Description { get; set; }
        public string ClassnameKeyDomainRef { get; set; }
        public long IdA3DomainRef { get; set; }
        public string EntrySetAdHocAcl { get; set; }
        public string EventSet { get; set; }
        public string ExpressionData { get; set; }
        public bool ExtentsValid { get; set; }
        public int FamilyTableStatus { get; set; }
        public string ClassnameKeyA2FolderingInfo { get; set; }
        public long IdA3A2FolderingInfo { get; set; }
        public string ClassnameKeyB2FolderingInfo { get; set; }
        public long IdA3B2FolderingInfo { get; set; }
        public string ClassnameKeyFormat { get; set; }
        public long IdA3Format { get; set; }
        public bool HasHangingChange { get; set; }
        public bool HasPendingChange { get; set; }
        public bool HasResultingChange { get; set; }
        public bool HasSuspect { get; set; }
        public bool HasVariance { get; set; }
        public string IndexersIndexerSet { get; set; }
        public bool InheritedDomain { get; set; }
        public string IopStateInteropInfo { get; set; }
        public string StateInteropInfo { get; set; }
        public long BranchIdIterationInfo { get; set; }
        public string ClassnameKeyD2IterationInfo { get; set; }
        public long IdA3D2IterationInfo { get; set; }
        public string ClassnameKeyE2IterationInfo { get; set; }
        public long IdA3E2IterationInfo { get; set; }
        public string IterationIdA2IterationInfo { get; set; }
        public bool LatestIterationInfo { get; set; }
        public string ClassnameKeyB2IterationInfo { get; set; }
        public long IdA3B2IterationInfo { get; set; }
        public string NoteIterationInfo { get; set; }
        public string ClassnameKeyC2IterationInfo { get; set; }
        public long IdA3C2IterationInfo { get; set; }
        public string StateIterationInfo { get; set; }
        public float LengthScale { get; set; }
        public DateTime DateLock { get; set; }
        public string ClassnameKeyA2Lock { get; set; }
        public long IdA3A2Lock { get; set; }
        public string NoteLock { get; set; }
        public string ClassnameKeyMasterReference { get; set; }
        public long IdA3MasterReference { get; set; }
        public int MaximumAllowed { get; set; }
        public int MinimumRequired { get; set; }
        public bool MissingDependents { get; set; }
        public string OneOffVersionIdA2OneOffVersion { get; set; }
        public string ClassnameKeyA2Ownership { get; set; }
        public long IdA3A2Ownership { get; set; }
        public bool PlaceHolder { get; set; }
        public bool ReferenceControlIsNull { get; set; }
        public int GeomRestrE10 { get; set; }
        public bool GeomRestrRecursiveE10 { get; set; }
        public int ScopeE10 { get; set; }
        public int ViolRestrictionE10 { get; set; }
        public int RevisionNumber { get; set; }
        public string ClassnameKeyRootItemReference { get; set; }
        public long IdA3RootItemReference { get; set; }
        public string SecurityLabels { get; set; }
        public bool AtGateState { get; set; }
        public string ClassnameKeyA2State { get; set; }
        public long IdA3A2State { get; set; }
        public string StateState { get; set; }
        public bool TeamIdIsNull { get; set; }
        public string ClassnameKeyTeamId { get; set; }
        public long IdA3TeamId { get; set; }
        public bool TeamTemplateIdIsNull { get; set; }
        public string ClassnameKeyTeamTemplateId { get; set; }
        public long IdA3TeamTemplateId { get; set; }
        public bool EnabledTemplate { get; set; }
        public bool TemplatedTemplate { get; set; }
        public DateTime CreateStampA2 { get; set; }
        public long MarkForDeleteA2 { get; set; }
        public DateTime ModifyStampA2 { get; set; }
        public string ClassnameA2A2 { get; set; }
        [Key]
        public long IdA2A2 { get; set; }
        public int UpdateCountA2 { get; set; }
        public DateTime UpdateStampA2 { get; set; }
        public string VersionIdA2VersionInfo { get; set; }
        public int VersionLevelA2VersionInfo { get; set; }
        public string VersionSortIdA2VersionInfo { get; set; }
        public byte WT_FBI_COMPUTE_U_0_3 { get; set; }
        public byte WT_FBI_COMPUTE_U_0_4 { get; set; }
    }
}
