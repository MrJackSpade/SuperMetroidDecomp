using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>Decodes the native map highlight sequence into colors/durations, not an executable palette program.</summary>
internal static class MapPaletteCycleExtractor
{
    /// <summary>Reads timed RGB5 map-highlight frames until the native $FF duration
    /// terminator and writes the bounded editable palette-cycle document.</summary>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        var frames = new List<MapPaletteCycleFrame>();
        for (int frame = 0; frame <= MapPaletteCycleFormat.MaximumFrames; frame++)
        {
            int duration = bus.ReadCartridgeByte(MapSourceDefinitions.Animation.PaletteTiming + frame * MapSourceDefinitions.Animation.PaletteTimingStride);
            if (duration == byte.MaxValue)
            {
                using var json = new MemoryStream();
                MapPaletteCycleFile.Write(json, new() { Version = MapPaletteCycleFormat.Version, Frames = frames.ToArray() });
                return json.ToArray();
            }
            if (frame == MapPaletteCycleFormat.MaximumFrames)
                break;
            var colors = new PaletteRgb5[MapPaletteCycleFormat.ColorCount];
            for (int color = 0; color < colors.Length; color++)
            {
                int word = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
                    MapSourceDefinitions.Animation.PaletteColors + (frame * colors.Length + color) * sizeof(ushort));
                colors[color] = new() { Red = word & 31, Green = (word >> 5) & 31, Blue = (word >> 10) & 31 };
            }
            frames.Add(new() { DurationTicks = duration, Colors = colors });
        }
        throw new InvalidDataException("Map highlight cycle has no terminator within its byte-sized frame range.");
    }
}
