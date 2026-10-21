namespace SuperMetroid.EnsureAnalyzer.CodeFixes;

/// <summary>The address, width and region of a bus cell.</summary>
/// <param name="Address">The address.</param>
/// <param name="Width">The width.</param>
/// <param name="Region">The region.</param>
public readonly record struct CellShape(int Address, int Width, string Region);
