namespace SuperMetroid.AssetExtraction.Enemies.Corpses;

/// <summary>The tile bounds of the dead Torizo's stationary composition in its source sheet, which crop its staged corpse.</summary>
/// <param name="Left">The left edge.</param>
/// <param name="Top">The top edge.</param>
/// <param name="Right">The right edge.</param>
/// <param name="Bottom">The bottom edge.</param>
internal readonly record struct DeadTorizoBounds(int Left, int Top, int Right, int Bottom);
