using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Exports the cutscene Baby's initial and displayed fade-to-black RGB5 images.</summary>
public static class BabyMetroidCutsceneColorExtractor
{
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        var fade = new PaletteRgb5[BabyMetroidCutsceneColorRomData.FadeFrameCount][];
        for (int frame = 0; frame < fade.Length; frame++)
            fade[frame] = Read(BabyMetroidCutsceneColorRomData.FadeSource(frame + 1),
                BabyMetroidCutsceneColorRomData.FadeColorCount);
        return BabyMetroidCutsceneColorCatalog.Write(new BabyMetroidCutsceneColorDocument
        {
            Version = BabyMetroidCutsceneColorFormat.Version,
            Initial = Read(BabyMetroidCutsceneColorRomData.InitialSource,
                BabyMetroidCutsceneColorRomData.InitialColorCount),
            Fade = fade,
        });

        PaletteRgb5[] Read(int source, int count)
        {
            var colors = new PaletteRgb5[count];
            for (int color = 0; color < count; color++)
            {
                int address = source + color * sizeof(ushort);
                ushort native = RomDataReader.ReadWordFixedBank(bus, address);
                if ((native & 0x8000) != 0)
                    throw new InvalidDataException(
                        $"Cutscene Baby color ${address:X6} has an unrepresentable high bit.");
                colors[color] = new PaletteRgb5
                {
                    Red = native & 31,
                    Green = native >> 5 & 31,
                    Blue = native >> 10 & 31,
                };
            }
            return colors;
        }
    }
}
