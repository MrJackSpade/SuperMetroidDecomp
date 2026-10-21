namespace SuperMetroid.EnsureAnalyzer.CodeFixes;

/// <summary>The type and method of an Ensure call.</summary>
/// <param name="Type">The type.</param>
/// <param name="Method">The method.</param>
internal readonly record struct EnsureCallKey(string Type, string Method);
