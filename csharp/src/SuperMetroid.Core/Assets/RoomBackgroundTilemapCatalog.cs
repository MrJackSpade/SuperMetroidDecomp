namespace SuperMetroid.Core.Assets;

/// <summary>Complete host-selected set of compressed library-background visual tilemaps.</summary>
public sealed class RoomBackgroundTilemapCatalog
{
    private readonly Dictionary<int, RoomBackgroundTilemapAtlas> bySource;

    public RoomBackgroundTilemapCatalog(
        IReadOnlyDictionary<int, RoomBackgroundTilemapAtlas> bySource)
    {
        ArgumentNullException.ThrowIfNull(bySource);
        if (bySource.Count != RoomBackgroundTilemapFormat.RetailCompressedSourceCount ||
            bySource.Any(pair => pair.Value is null))
            throw new InvalidDataException(
                $"Room background catalog requires " +
                $"{RoomBackgroundTilemapFormat.RetailCompressedSourceCount} complete sources.");
        this.bySource = new Dictionary<int, RoomBackgroundTilemapAtlas>(bySource);
    }

    public RoomBackgroundTilemapAtlas Get(int sourceAddress) =>
        bySource.TryGetValue(sourceAddress, out RoomBackgroundTilemapAtlas? atlas)
            ? atlas
            : throw new InvalidDataException(
                $"No installed room background tilemap for source ${sourceAddress:X6}.");
}
