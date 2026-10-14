namespace SuperMetroid.ResourceAudit;

/// <summary>Collected queue sites, resolved transfer descriptors, and unresolved ownership findings.</summary>
internal sealed class VramDmaReport
{
    /// <summary>Direct queue invocations found by Roslyn semantic binding.</summary>
    public List<VramDmaSite> Sites { get; } = [];
    /// <summary>Constant or finite-domain source and count descriptors.</summary>
    public List<VramDmaTransfer> Transfers { get; } = [];
    /// <summary>Bindings and source ranges that the audit could not prove safe.</summary>
    public List<VramDmaFinding> Findings { get; } = [];
}
