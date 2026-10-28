namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>One pixel of one world-map character cell.</summary>
/// <param name="Cell">The character cell.</param>
/// <param name="X">The pixel's column within the cell.</param>
/// <param name="Y">The pixel's row within the cell.</param>
internal readonly record struct WorldMapCellPixel(int Cell, int X, int Y);
