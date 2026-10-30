using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rendering;

/// <summary>The installed picture one cell of a picture grid shows (#1467), mirrored as the cell shows it.</summary>
/// <param name="Picture">The cell's picture, one cell in size.</param>
/// <param name="Mirroring">How the cell mirrors the picture.</param>
public readonly record struct GridPicture(SpriteSheetFrame Picture, TileFlip Mirroring);
