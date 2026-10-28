namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>One pixel of one world-map character cell.</summary>
/// <param name="Cell">The character cell.</param>
/// <param name="Pixel">The pixel's position within the cell.</param>
internal readonly record struct WorldMapCellPixel(int Cell, Position16<CharacterPixels> Pixel);
