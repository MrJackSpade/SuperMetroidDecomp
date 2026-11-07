
namespace SuperMetroid.ResourceAudit.ClosedContracts;

/// <summary>A closed presentation contract and whether it is valid.</summary>
/// <param name="Contract">The contract.</param>
/// <param name="Valid">Whether its proof still holds.</param>
/// <param name="StaleReason">Why the proof no longer holds, or null when it does.</param>
internal readonly record struct ContractValidity(ClosedPresentationContract Contract, bool Valid, string? StaleReason);
