namespace SuperMetroid.ReachabilityAudit;

/// <summary>Everything the report needs: declarations, both reachability sets and the scans.</summary>
internal sealed record ReachabilityResult(
    IReadOnlyDictionary<string, Declaration> Declarations,
    HashSet<string> Reachable,
    HashSet<string> ProductionReachable,
    HashSet<string> Referenced,
    HashSet<string> SerializerAccessed,
    ReflectionScan Reflection,
    HashSet<string> VerificationSourceFiles,
    ReachabilityGraph Graph,
    IReadOnlySet<string> ProductionRoots,
    IReadOnlySet<string> Roots);
