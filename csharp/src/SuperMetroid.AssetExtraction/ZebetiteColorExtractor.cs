using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction;

/// <summary>Extracts only Zebetite's eight authored pulse images from the pinned cartridge.</summary>
public static class ZebetiteColorExtractor
{
    /// <summary>Exports the eight two-color Zebetite pulse images from $A6:FD87 in native frame order.</summary>
    /// <param name="bus">Cartridge source for the sixteen consecutive packed color words, selected at runtime for OBJ palette two colors C and D.</param>
    /// <returns>A new UTF-8 JSON buffer with eight two-color RGB5 frames, each channel in 0..31.</returns>
    /// <remarks>Only lower-fifteen-bit channels are represented. Pulse cadence, Zebetite health/destruction, palette-slot selection, and collision behavior are not exported or changed.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
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
                ushort word = (ushort)(bus.ReadCartridgeByte(address) | bus.ReadCartridgeByte(address + 1) << 8);
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
