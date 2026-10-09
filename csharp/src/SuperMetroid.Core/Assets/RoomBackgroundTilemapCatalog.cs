using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Assets;

/// <summary>Complete host-selected set of compressed library-background visual tilemaps.</summary>
public sealed class RoomBackgroundTilemapCatalog
{
    private readonly Dictionary<int, RoomBackgroundTilemapAtlas> bySource;

    /// <summary>Installs all 58 required compressed-source identities selected by the bank-$8F library-background programs, copying the lookup while retaining the supplied compiled atlases.</summary>
    /// <param name="bySource">Complete mapping from full native SNES source addresses to nonnull selected tilemaps; keys identify compressed cartridge sources, not WRAM or VRAM destinations.</param>
    /// <exception cref="ArgumentNullException"><paramref name="bySource"/> is null.</exception>
    /// <exception cref="InvalidDataException">The mapping does not contain exactly every required source with a nonnull atlas.</exception>
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

    /// <summary>Resolves installed, already compiled tilemap words for a library-background decompression command; performs no cartridge read or decompression.</summary>
    /// <param name="sourceAddress">Full 24-bit native compressed-source identity selected by the command, independent of its destination and transfer size.</param>
    /// <returns>The retained atlas whose ordered page bytes the command stages in WRAM before subsequent VRAM transfers.</returns>
    /// <exception cref="InvalidDataException">No tilemap is installed for the requested source identity.</exception>
    public RoomBackgroundTilemapAtlas Get(int sourceAddress) =>
        bySource.TryGetValue(sourceAddress, out RoomBackgroundTilemapAtlas? atlas)
            ? atlas
            : throw new InvalidDataException(
                $"No installed room background tilemap for source ${sourceAddress:X6}.");
}
