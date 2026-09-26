using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction;

/// <summary>Reads only the authored Hyper Beam palette-FX color words from the validated cartridge.</summary>
public static class HyperBeamFxColorExtractor
{
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        var frames = new PaletteRgb5[HyperBeamFxColorFormat.FrameCount][];
        for (int frame = 0; frame < frames.Length; frame++)
        {
            ushort pointer = (ushort)(HyperBeamPaletteFxProgramDefinitions.FirstFramePointer +
                frame * HyperBeamPaletteFxProgramDefinitions.FrameByteCount + sizeof(ushort));
            frames[frame] = new PaletteRgb5[HyperBeamFxColorFormat.ColorsPerFrame];
            for (int color = 0; color < frames[frame].Length; color++)
            {
                int address = SamusPaletteRomData.Banks.PaletteFx | (pointer + color * sizeof(ushort));
                ushort word = (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);
                frames[frame][color] = new PaletteRgb5
                {
                    Red = word & 31,
                    Green = word >> 5 & 31,
                    Blue = word >> 10 & 31,
                };
            }
        }
        return HyperBeamFxColorCatalog.Write(new() { Version = HyperBeamFxColorFormat.Version, Frames = frames });
    }
}
