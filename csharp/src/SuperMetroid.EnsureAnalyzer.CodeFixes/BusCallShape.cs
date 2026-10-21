namespace SuperMetroid.EnsureAnalyzer.CodeFixes;

/// <summary>The type, method and argument count of a bus call.</summary>
/// <param name="Type">The type.</param>
/// <param name="Method">The method.</param>
/// <param name="Count">The count.</param>
internal readonly record struct BusCallShape(string Type, string Method, int Count);
