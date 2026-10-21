namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>A numbered name pattern and whether its members are static.</summary>
/// <param name="Pattern">The pattern.</param>
/// <param name="IsStatic">The is static.</param>
internal readonly record struct NumberedPatternKey(string Pattern, bool IsStatic);
