namespace SuperMetroid.EnsureAnalyzer.Contracts;

/// <summary>A literal compared with a mode, and whether by equality.</summary>
/// <param name="Value">The value.</param>
/// <param name="Equality">The equality.</param>
public readonly record struct ModeLiteral(long Value, bool Equality);
