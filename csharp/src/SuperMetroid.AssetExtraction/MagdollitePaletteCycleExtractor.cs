using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Extracts only the four colors that the Magdollite draw hook actually cycles.</summary>
public static class MagdollitePaletteCycleExtractor
{
    /// <summary>Exports OBJ-row colors nine through twelve from each of the four Magdollite palette rows at $A8:AC1C.</summary>
    /// <param name="bus">Non-null import-capable cartridge source containing four sixteen-color source rows.</param>
    /// <returns>A new UTF-8 JSON buffer with four ordered frames of four RGB5 colors, each channel in 0..31.</returns>
    /// <remarks>The other twelve source colors and graphics-drawn-hook timing are not exported; only the hook's animated subset is replaceable here.</remarks>
    /// <exception cref="ArgumentNullException"><paramref name="bus"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="bus"/> lacks cartridge import access.</exception>
    /// <exception cref="InvalidDataException">An imported color sets bit 15, which cannot be represented by RGB5 channels.</exception>
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
                ushort native = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), source);
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
