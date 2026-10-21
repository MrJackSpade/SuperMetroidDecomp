namespace SuperMetroid.ResourceAudit.Resources;

/// <summary>One synthetic audit contract check the self-check runs.</summary>
/// <param name="Name">The check's name, reported with its failure.</param>
/// <param name="Run">Runs the check; it throws when the contract does not hold.</param>
internal sealed record AuditContractCheck(string Name, Action Run);
