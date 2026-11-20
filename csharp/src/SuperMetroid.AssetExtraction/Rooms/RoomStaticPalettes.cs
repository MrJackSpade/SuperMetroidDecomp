using SuperMetroid.Core;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>The installed base room palettes.</summary>
public sealed class RoomStaticPalettes
{
    /// <summary>Copied lookup by palette; the RoomStaticPalette instances are shared and immutable.</summary>
    internal readonly Dictionary<RoomPaletteSet, RoomStaticPalette> byRoomPaletteSet;

    /// <summary>Copies the lookup and requires an entry for every retail graphics set's palette.</summary>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    /// <exception cref="InvalidDataException">A graphics set's palette is missing or null.</exception>
    public RoomStaticPalettes(IReadOnlyDictionary<RoomPaletteSet, RoomStaticPalette> palettes)
    {
        Ensure.NotNull(palettes);
        foreach (TilesetDefinition tileset in RoomTilesets.All)
        {
            RoomPaletteSet required = tileset.Palette;
            if (!palettes.TryGetValue(required, out RoomStaticPalette? value) || value is null)
                throw new InvalidDataException($"RoomStaticPalettes lacks graphics set {tileset.GraphicsSet}'s {required}.");
        }
        byRoomPaletteSet = new Dictionary<RoomPaletteSet, RoomStaticPalette>(palettes);
    }

    /// <summary>The installed RoomStaticPalette for <paramref name="set"/>.</summary>
    /// <exception cref="InvalidDataException">None is installed for <paramref name="set"/>.</exception>
    public RoomStaticPalette Get(RoomPaletteSet set) =>
        byRoomPaletteSet.TryGetValue(set, out RoomStaticPalette? value)
            ? value
            : throw new InvalidDataException($"No RoomStaticPalette is installed for {set}.");
}
