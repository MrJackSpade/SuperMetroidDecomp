namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>A method name with its sides removed, and the sides.</summary>
/// <param name="Canonical">The canonical.</param>
/// <param name="Sides">The sides.</param>
public readonly record struct CanonicalMethodName(string Canonical, string Sides);
