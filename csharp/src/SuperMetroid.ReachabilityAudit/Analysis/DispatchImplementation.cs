namespace SuperMetroid.ReachabilityAudit.Analysis;

/// <summary>A repository member implementing a framework interface member, and the keys that show its type constructed.</summary>
/// <param name="ImplementationKey">The implementing member's key.</param>
/// <param name="InstantiationKeys">The type's constructor keys, or the type's own key for a value type.</param>
internal readonly record struct DispatchImplementation(string ImplementationKey, IReadOnlyList<string> InstantiationKeys);
