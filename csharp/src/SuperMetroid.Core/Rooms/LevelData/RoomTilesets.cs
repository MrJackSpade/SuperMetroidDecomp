namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>The artwork resources a room state's graphics set selects.</summary>
public static class RoomTilesets
{
    /// <summary>The tileset of every room graphics set, one per <see cref="RoomGraphicsSet"/>.</summary>
    public static IReadOnlyList<TilesetDefinition> All { get; } =
        [.. Enum.GetValues<RoomGraphicsSet>().Select(graphicsSet => new TilesetDefinition(graphicsSet))];

    /// <summary>The tileset definition of the room state's graphics set, which names the set's pictures.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="graphicsSet"/> is not a room graphics set.</exception>
    public static TilesetDefinition Get(RoomGraphicsSet graphicsSet) =>
        Enum.IsDefined(graphicsSet)
            ? new TilesetDefinition(graphicsSet)
            : throw new ArgumentOutOfRangeException(nameof(graphicsSet), graphicsSet, "Not a room graphics set.");
}
