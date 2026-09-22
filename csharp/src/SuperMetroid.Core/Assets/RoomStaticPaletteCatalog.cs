using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Assets;

/// <summary>Complete immutable set of compiled base room palettes for every graphics set.</summary>
public sealed class RoomStaticPaletteCatalog
{
    private readonly Dictionary<int, RoomStaticPalette> bySource;

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

    public RoomStaticPalette Get(int sourceAddress) =>
        bySource.TryGetValue(sourceAddress, out RoomStaticPalette? palette)
            ? palette
            : throw new InvalidDataException($"No room palette for source ${sourceAddress:X6}.");
}
