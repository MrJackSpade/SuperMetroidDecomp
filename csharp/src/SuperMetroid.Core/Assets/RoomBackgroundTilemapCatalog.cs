using SuperMetroid.Core.Rooms;

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
            RoomBackgroundTilemapSources.All.Any(source =>
                !bySource.TryGetValue(source, out RoomBackgroundTilemapAtlas? atlas) || atlas is null))
            throw new InvalidDataException(
                $"Room background catalog requires " +
                $"all {RoomBackgroundTilemapFormat.RetailCompressedSourceCount} required source identities with nonnull tilemaps.");
        this.bySource = new Dictionary<int, RoomBackgroundTilemapAtlas>(bySource);
    }

    /// <summary>SHA-256 of every selected library-background page in stable source order.</summary>
    public string ContentIdentity => SelectedPresentationHash.FromTransfers(
        nameof(RoomBackgroundTilemapCatalog), this.bySource, atlas => atlas.Transfer);

    public RoomBackgroundTilemapAtlas Get(int sourceAddress) =>
        bySource.TryGetValue(sourceAddress, out RoomBackgroundTilemapAtlas? atlas)
            ? atlas
            : throw new InvalidDataException(
                $"No installed room background tilemap for source ${sourceAddress:X6}.");
}
