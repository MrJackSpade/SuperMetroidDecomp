using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Assets;

/// <summary>Complete selected visual block definitions; no level collision or BTS data.</summary>
public sealed class RoomMetatileCatalog
{
    private readonly Dictionary<int, RoomMetatileAtlas> bySource;

    /// <summary>Builds the selected CRE/graphics-set metatile snapshot and verifies that every compiled retail tileset's block-definition source is present.</summary>
    /// <param name="cre">Compiled common-room-elements visual block definitions shared by applicable graphics sets.</param>
    /// <param name="bySource">Atlases keyed by native block-definition source identity; the dictionary is copied while the compiled atlas objects remain shared.</param>
    /// <exception cref="ArgumentNullException">The CRE atlas or source dictionary is null.</exception>
    /// <exception cref="InvalidDataException">A retail graphics-set source is missing or resolves to a null atlas.</exception>
    public RoomMetatileCatalog(RoomMetatileAtlas cre,
        IReadOnlyDictionary<int, RoomMetatileAtlas> bySource)
    {
        Cre = cre ?? throw new ArgumentNullException(nameof(cre));
        ArgumentNullException.ThrowIfNull(bySource);
        for (byte graphicsSet = 0; graphicsSet < RoomTilesetDefinitions.Count; graphicsSet++)
        {
            int source = RoomTilesetDefinitions.Get(graphicsSet).BlockDefinitionsAddress;
            if (!bySource.TryGetValue(source, out RoomMetatileAtlas? atlas) || atlas is null)
                throw new InvalidDataException(
                    $"Room metatile catalog lacks graphics set ${graphicsSet:X2} source ${source:X6}.");
        }
        this.bySource = new Dictionary<int, RoomMetatileAtlas>(bySource);
    }

    /// <summary>Selected common-room-elements visual metatiles; consumers merge these with graphics-set words when the native tileset requires CRE.</summary>
    public RoomMetatileAtlas Cre { get; }

    /// <summary>SHA-256 of selected metatile words, including flips, palettes, and priority.</summary>
    public string ContentIdentity => SelectedPresentationHash.FromTransfers(
        nameof(RoomMetatileCatalog), this.bySource, atlas => atlas.Transfer, Cre.Transfer);

    /// <summary>Resolves a compiled visual metatile atlas by native source identity, without reading ROM or selecting collision/BTS behavior.</summary>
    /// <param name="sourceAddress">Cartridge block-definition stream identity from the room's compiled tileset, not a runtime memory-read address.</param>
    /// <returns>The shared atlas containing four 8x8 BG character words per 16x16 visual block.</returns>
    /// <exception cref="InvalidDataException">No atlas is installed for the source identity.</exception>
    public RoomMetatileAtlas Get(int sourceAddress) =>
        bySource.TryGetValue(sourceAddress, out RoomMetatileAtlas? atlas)
            ? atlas
            : throw new InvalidDataException($"No room metatile atlas for source ${sourceAddress:X6}.");
}
