namespace SuperMetroid.ReachabilityAudit;

/// <summary>One repository declaration and every project that compiles it.</summary>
internal sealed record Declaration(string Key, string Kind, string Display, string File, int Line,
    string ContainingTypeKey, DeclarationShape Shape, HashSet<string> Projects);
