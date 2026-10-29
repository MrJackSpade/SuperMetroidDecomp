using SuperMetroid.Core;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Complete host-selected set of compressed library-background visual tilemaps.</summary>
public sealed class RoomBackgroundTilemaps
{
    /// <summary>Copied tilemap lookup; each selected atlas remains the caller's installed compilation.</summary>
    private readonly Dictionary<RoomBackgroundTilemap, RoomBackgroundTilemapAtlas> byTilemap;

    /// <summary>Installs every background tilemap the library-background programs decompress, copying the lookup while retaining the supplied compiled atlases.</summary>
    /// <param name="byTilemap">Complete mapping from every <see cref="RoomBackgroundTilemap"/> to its nonnull selected tilemap.</param>
    /// <exception cref="ArgumentNullException"><paramref name="byTilemap"/> is null.</exception>
    /// <exception cref="InvalidDataException">The mapping does not contain exactly every tilemap with a nonnull atlas.</exception>
    public RoomBackgroundTilemaps(
        IReadOnlyDictionary<RoomBackgroundTilemap, RoomBackgroundTilemapAtlas> byTilemap)
    {
        Ensure.NotNull(byTilemap);
        RoomBackgroundTilemap[] all = Enum.GetValues<RoomBackgroundTilemap>();
        if (byTilemap.Count != all.Length ||
            all.Any(tilemap => !byTilemap.TryGetValue(tilemap, out RoomBackgroundTilemapAtlas? atlas) || atlas is null))
            throw new InvalidDataException(
                $"Room background catalog requires all {all.Length} background tilemaps with nonnull tilemaps.");
        this.byTilemap = new Dictionary<RoomBackgroundTilemap, RoomBackgroundTilemapAtlas>(byTilemap);
    }

    /// <summary>Every selected tilemap, by the background it pictures.</summary>
    public IReadOnlyDictionary<RoomBackgroundTilemap, RoomBackgroundTilemapAtlas> Tilemaps => byTilemap;

    /// <summary>Resolves installed, already compiled tilemap words for a library-background decompression command; performs no cartridge read or decompression.</summary>
    /// <param name="tilemap">The background tilemap the command decompresses.</param>
    /// <returns>The retained atlas whose ordered page bytes the command stages before its transfers.</returns>
    /// <exception cref="InvalidDataException">No tilemap is installed for <paramref name="tilemap"/>.</exception>
    public RoomBackgroundTilemapAtlas Get(RoomBackgroundTilemap tilemap) =>
        byTilemap.TryGetValue(tilemap, out RoomBackgroundTilemapAtlas? atlas)
            ? atlas
            : throw new InvalidDataException(
                $"No installed room background tilemap for {tilemap}.");
}
