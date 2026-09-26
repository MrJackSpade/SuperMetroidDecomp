using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Extracts only the four colors that the Magdollite draw hook actually cycles.</summary>
public static class MagdollitePaletteCycleExtractor
{
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        var frames = new PaletteRgb5[MagdollitePaletteRomData.FrameCount][];
        for (int frame = 0; frame < frames.Length; frame++)
        {
            frames[frame] = new PaletteRgb5[MagdollitePaletteRomData.AnimatedColorCount];
            for (int color = 0; color < frames[frame].Length; color++)
            {
                int source = MagdollitePaletteRomData.Source +
                    (frame * MagdollitePaletteRomData.SourceColorsPerFrame +
                     MagdollitePaletteRomData.FirstAnimatedColor + color) * sizeof(ushort);
                ushort native = RomDataReader.ReadWordFixedBank(bus, source);
                if ((native & 0x8000) != 0)
                    throw new InvalidDataException(
                        $"Magdollite color ${source:X6} has an unrepresentable high bit.");
                frames[frame][color] = new PaletteRgb5
                {
                    Red = native & 31,
                    Green = native >> 5 & 31,
                    Blue = native >> 10 & 31,
                };
            }
        }
        return MagdollitePaletteCycle.Write(new MagdollitePaletteCycleDocument
        {
            Version = MagdollitePaletteCycleFormat.Version,
            Frames = frames,
        });
    }
}
