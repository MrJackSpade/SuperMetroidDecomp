using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// A written tile as one picture of it shows it (#1392, #1466): the tile, and how a picture's name spells the values of
/// the appearance inputs it depends on, empty for a tile that depends on none.
/// </summary>
/// <param name="Tile">The written tile.</param>
/// <param name="Appearance">The values of its inputs, as <see cref="AppearancePictureNames.Spelling"/> spells them.</param>
public readonly record struct TileWordAppearance(BackgroundTile Tile, string Appearance);
