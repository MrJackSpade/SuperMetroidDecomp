using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// What one ink of a sheet shows under a row shade (#1512): the colour its pixels take there, and whether the shade's draw
/// takes them from another row than the ink's own.
/// </summary>
/// <param name="Colour">The colour the ink's pixels show under the shade.</param>
/// <param name="Moves">Whether the shade's draw takes the ink's pixels from another row than their own.</param>
internal readonly record struct RowShadedInk(RgbColor Colour, bool Moves);
