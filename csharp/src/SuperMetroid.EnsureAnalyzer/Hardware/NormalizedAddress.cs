namespace SuperMetroid.EnsureAnalyzer.Hardware;

/// <summary>An address expression's terms and constant part.</summary>
/// <param name="Terms">The terms.</param>
/// <param name="Constant">The constant.</param>
public readonly record struct NormalizedAddress(string Terms, long Constant);
