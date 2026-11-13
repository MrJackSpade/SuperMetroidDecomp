using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Frontend.Map.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>Decodes the native map highlight sequence into each frame's hold and highlight colour, not an executable palette program or palette rows (#1637).</summary>
internal static class MapPaletteCycleExtractor
{
    /// <summary>Reads timed map-highlight frames until the native $FF duration terminator and writes the bounded editable
    /// cycle document, each frame's look as the colour its highlight shows.</summary>
    /// <exception cref="InvalidDataException">The cycle has no terminator, or a frame's highlight colours differ.</exception>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        var frames = new List<MapPaletteCycleFrame>();
        for (int frame = 0; frame <= MapPaletteCycleFile.MaximumFrames; frame++)
        {
            int duration = bus.ReadCartridgeByte(MapSourceDefinitions.Animation.PaletteTiming + frame * MapSourceDefinitions.Animation.PaletteTimingStride);
            if (duration == byte.MaxValue)
            {
                using var json = new MemoryStream();
                MapPaletteCycleFile.Write(json, new() { Version = MapPaletteCycleFile.Version, Frames = frames.ToArray() });
                return json.ToArray();
            }
            if (frame == MapPaletteCycleFile.MaximumFrames)
                break;
            PaletteRgb5[] highlight = [.. MapPaletteCycleFormat.HighlightInks.Select(ink =>
            {
                int word = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
                    MapSourceDefinitions.Animation.PaletteColors + (frame * MapPaletteCycleFormat.ColorCount + ink) * sizeof(ushort));
                return new PaletteRgb5 { Red = word & 31, Green = (word >> 5) & 31, Blue = (word >> 10) & 31 };
            })];
            if (highlight.Distinct().Count() != 1)
                throw new InvalidDataException($"Map highlight frame {frame}'s highlight colours differ.");
            frames.Add(new() { DurationTicks = duration, Highlight = highlight[0] });
        }
        throw new InvalidDataException("Map highlight cycle has no terminator within its byte-sized frame range.");
    }
}
