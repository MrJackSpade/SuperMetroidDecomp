
namespace SuperMetroid.ResourceAudit.ClosedContracts;

/// <summary>A closed presentation contract and whether it is valid.</summary>
/// <param name="Contract">The contract.</param>
/// <param name="Valid">The valid.</param>
internal readonly record struct ContractValidity(ClosedPresentationContract Contract, bool Valid);
