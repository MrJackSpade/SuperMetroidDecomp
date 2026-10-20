namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>The dead Torizo's collision bounds, relative to its origin.</summary>
/// <param name="Left">The left edge.</param>
/// <param name="Top">The top edge.</param>
/// <param name="Right">The right edge.</param>
/// <param name="Bottom">The bottom edge.</param>
internal readonly record struct DeadTorizoBounds(int Left, int Top, int Right, int Bottom);
