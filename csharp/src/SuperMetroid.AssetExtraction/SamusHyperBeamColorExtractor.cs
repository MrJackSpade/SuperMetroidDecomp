using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Exports the ten authored Hyper Beam palettes without exposing phase logic.</summary>
public static class SamusHyperBeamColorExtractor
{
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        var frames = new PaletteRgb5[SamusHyperBeamColorFormat.FrameCount][];
        for (int frame = 0; frame < frames.Length; frame++)
        {
            int address = SamusPaletteRomData.FullBodyCycles.HyperBeamPaletteSource(frame);
            byte[] source = RomDataReader.ReadFixedBank(bus, address,
                SamusHyperBeamColorFormat.ColorsPerFrame * sizeof(ushort));
            frames[frame] = new PaletteRgb5[SamusHyperBeamColorFormat.ColorsPerFrame];
            for (int index = 0; index < frames[frame].Length; index++)
            {
                ushort word = unchecked((ushort)(source[index * 2] | source[index * 2 + 1] << 8));
                frames[frame][index] = new PaletteRgb5
                {
                    Red = word & 31,
                    Green = (word >> 5) & 31,
                    Blue = (word >> 10) & 31,
                };
            }
        }
        return SamusHyperBeamColorCatalog.Write(new SamusHyperBeamColorDocument
        {
            Version = SamusHyperBeamColorFormat.Version,
            Frames = frames,
        });
    }
}
