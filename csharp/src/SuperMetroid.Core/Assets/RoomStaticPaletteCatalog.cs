using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Assets;

/// <summary>Complete immutable set of compiled base room palettes for every graphics set.</summary>
public sealed class RoomStaticPaletteCatalog
{
    /// <summary>Defensive copy of compiled palettes indexed by their native graphics-set source addresses.</summary>
    private readonly Dictionary<int, RoomStaticPalette> bySource;

    /// <summary>Copies the palette lookup and requires a nonnull compiled palette for every retail graphics-set source.</summary>
    /// <param name="bySource">Palettes keyed by native source address; dictionary ownership is copied while immutable palette objects remain shared.</param>
    /// <exception cref="ArgumentNullException">The source dictionary is null.</exception>
    /// <exception cref="InvalidDataException">A required graphics-set palette is missing or null.</exception>
    public RoomStaticPaletteCatalog(IReadOnlyDictionary<int, RoomStaticPalette> bySource)
    {
        ArgumentNullException.ThrowIfNull(bySource);
        for (byte graphicsSet = 0; graphicsSet < RoomTilesetDefinitions.Count; graphicsSet++)
        {
            int source = RoomTilesetDefinitions.Get(graphicsSet).PaletteAddress;
            if (!bySource.TryGetValue(source, out RoomStaticPalette? palette) || palette is null)
                throw new InvalidDataException(
                    $"Room palette catalog lacks graphics set ${graphicsSet:X2} source ${source:X6}.");
        }
        this.bySource = new Dictionary<int, RoomStaticPalette>(bySource);
    }

    /// <summary>SHA-256 of the selected, decoded room colors in stable source order.</summary>
    public string ContentIdentity => SelectedPresentationHash.FromTransfers(
        nameof(RoomStaticPaletteCatalog), this.bySource, palette => palette.Transfer);

    /// <summary>Resolves the selected base room palette by compiled tileset source identity without reading cartridge data.</summary>
    /// <param name="sourceAddress">Native palette-stream identity from the room's graphics-set definition.</param>
    /// <returns>The shared compiled palette containing native RGB5 color words for the base room upload.</returns>
    /// <exception cref="InvalidDataException">No palette is installed for the source identity.</exception>
    public RoomStaticPalette Get(int sourceAddress) =>
        bySource.TryGetValue(sourceAddress, out RoomStaticPalette? palette)
            ? palette
            : throw new InvalidDataException($"No room palette for source ${sourceAddress:X6}.");
}
