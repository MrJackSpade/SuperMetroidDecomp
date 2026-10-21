namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>A tile of the wrapped ending logo, and whether it is cropped.</summary>
/// <param name="Tile">The tile.</param>
/// <param name="Cropped">The cropped.</param>
internal readonly record struct EndingLogoWrapSelection(int Tile, bool Cropped);
