namespace SuperMetroid.Core.Map;

/// <summary>
/// The shape a map cell shows (#1466): which of the map's pictures it is, a room's outline, a corridor, a sloped
/// hallway or the empty cell. The map screens and the HUD minimap picture the same shapes under one numbering; the game
/// selects a shape and never reads anything from its number. Which shape is empty or sloped is the import pipeline's
/// knowledge, decided once into each area map's exploration rules.
/// </summary>
public readonly record struct MapCellShape
{
    /// <summary>Names map picture <paramref name="picture"/>.</summary>
    /// <param name="picture">The picture's place among the map's pictures; the installed map artwork pictures each one a map names.</param>
    public MapCellShape(int picture) => Picture = Ensure.AtLeastZero(picture);

    /// <summary>The picture's place among the map's pictures.</summary>
    public int Picture { get; }
}
