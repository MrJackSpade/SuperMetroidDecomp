namespace SuperMetroid.EnsureAnalyzer.CodeFixes;

/// <summary>The width and region a cell factory creates.</summary>
/// <param name="Width">The width.</param>
/// <param name="Region">The region.</param>
internal readonly record struct CellFactoryShape(int Width, string Region);
