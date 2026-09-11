using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Decodes the native map highlight sequence into colors/durations, not an executable palette program.</summary>
internal static class MapPaletteCycleExtractor
{
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        var frames = new List<MapPaletteCycleFrame>();
        for (int frame = 0; frame <= MapPaletteCycleFormat.MaximumFrames; frame++)
        {
            byte duration = bus.ReadByte(MapAnimationRomData.PaletteTiming + frame * MapAnimationRomData.PaletteTimingStride);
            if (duration == byte.MaxValue)
            {
                using var json = new MemoryStream();
                MapPaletteCycle.Write(json, new() { Version = MapPaletteCycleFormat.Version, Frames = frames.ToArray() });
                return json.ToArray();
            }
            if (frame == MapPaletteCycleFormat.MaximumFrames)
                break;
            var colors = new PaletteRgb5[MapPaletteCycleFormat.ColorCount];
            for (int color = 0; color < colors.Length; color++)
            {
                ushort word = RomDataReader.ReadWordFixedBank(bus,
                    MapAnimationRomData.PaletteColors + (frame * colors.Length + color) * sizeof(ushort));
                colors[color] = new() { Red = word & 31, Green = (word >> 5) & 31, Blue = (word >> 10) & 31 };
            }
            frames.Add(new() { DurationTicks = duration, Colors = colors });
        }
        throw new InvalidDataException("Map highlight cycle has no terminator within its byte-sized frame range.");
    }
}
