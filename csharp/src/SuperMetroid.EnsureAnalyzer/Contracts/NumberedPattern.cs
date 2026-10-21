namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>A name pattern and the ordinal a name fills it with.</summary>
/// <param name="Pattern">The pattern.</param>
/// <param name="Ordinal">The ordinal.</param>
public readonly record struct NumberedPattern(string Pattern, int Ordinal);
