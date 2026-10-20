namespace SuperMetroid.ResourceAudit.ClosedContracts;

/// <summary>The metadata names closed contracts match compiled types against.</summary>
internal static class ContractTypeNames
{
    /// <summary>The type's full metadata name; a type without one cannot be named by a contract.</summary>
    internal static string MetadataName(Type type) =>
        type.FullName ?? throw new InvalidOperationException($"{type} has no metadata name for a closed contract.");
}
