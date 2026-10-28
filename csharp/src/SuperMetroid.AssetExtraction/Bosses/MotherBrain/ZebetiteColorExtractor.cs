using SuperMetroid.Core;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain;

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
        Ensure.NotNull(bus);
        var frames = new PaletteRgb5[ZebetiteColorFormat.FrameCount][];
        for (int frame = 0; frame < frames.Length; frame++)
        {
            frames[frame] = new PaletteRgb5[ZebetiteColorFormat.ColorsPerFrame];
            for (int color = 0; color < frames[frame].Length; color++)
            {
                int address = MotherBrainSourceDefinitions.Zebetite.PulseColors +
                    (frame * ZebetiteColorFormat.ColorsPerFrame + color) * MotherBrainSourceDefinitions.ColorWordBytes;
                ushort word = CartridgeImportSource.Require(bus).ReadCartridgeWord(address);
                frames[frame][color] = new PaletteRgb5
                {
                    Red = word & 31,
                    Green = word >> 5 & 31,
                    Blue = word >> 10 & 31,
                };
            }
        }
        return ZebetiteColorFile.Write(new() { Version = ZebetiteColorFormat.Version,
            Frames = frames });
    }
}
