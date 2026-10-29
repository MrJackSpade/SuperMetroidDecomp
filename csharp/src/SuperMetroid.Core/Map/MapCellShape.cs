using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Map;

/// <summary>
/// The shape a map cell shows (#1466): which of the map's pictures it is, a room's outline, a corridor, a sloped
/// hallway or the empty cell. The map screens and the HUD minimap picture the same shapes under one numbering; the game
/// selects a shape and never reads anything from its number. Which shape is empty or sloped is the import pipeline's
/// knowledge, decided once into each area map's exploration rules.
/// </summary>
public readonly record struct MapCellShape
{
    /// <summary>The pictures a cell can name: as many as a background layer's tile set holds.</summary>
    public const int Count = BackgroundTile.TileCount;

    /// <summary>Names map picture <paramref name="picture"/>.</summary>
    /// <param name="picture">The picture's place among the map's pictures, 0 through one less than <see cref="Count"/>.</param>
    public MapCellShape(int picture) => Picture = Ensure.BetweenInclusive(0, Count - 1, picture);

    /// <summary>The picture's place among the map's pictures.</summary>
    public int Picture { get; }
}
