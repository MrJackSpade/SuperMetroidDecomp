using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction;

/// <summary>Extracts only Zebetite's eight authored pulse images from the pinned cartridge.</summary>
public static class ZebetiteColorExtractor
{
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        var frames = new PaletteRgb5[ZebetiteColorFormat.FrameCount][];
        for (int frame = 0; frame < frames.Length; frame++)
        {
            frames[frame] = new PaletteRgb5[ZebetiteColorFormat.ColorsPerFrame];
            for (int color = 0; color < frames[frame].Length; color++)
            {
                int address = ZebetiteDefinitions.PaletteSource +
                    (frame * ZebetiteColorFormat.ColorsPerFrame + color) * sizeof(ushort);
                ushort word = (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);
                frames[frame][color] = new PaletteRgb5
                {
                    Red = word & 31,
                    Green = word >> 5 & 31,
                    Blue = word >> 10 & 31,
                };
            }
        }
        return ZebetiteColorCatalog.Write(new() { Version = ZebetiteColorFormat.Version,
            Frames = frames });
    }
}
