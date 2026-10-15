using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain;

/// <summary>Exports the cutscene Baby's initial and displayed fade-to-black RGB5 images.</summary>
[CpuBusBoundary("codec: ROM cutscene Baby Metroid initial and fade-to-black colors → BabyMetroidCutsceneColorCatalog")]
public static class BabyMetroidCutsceneColorExtractor
{
    /// <summary>Reads the initial Baby palette and every authored fade row, rejects colors with the native high bit set, and serializes the versioned RGB5 document.</summary>
    /// <param name="bus">Supported-cartridge address space containing the compiled cutscene palette tables.</param>
    /// <returns>UTF-8 JSON bytes for the complete Baby cutscene color catalog.</returns>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
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
                ushort native = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), address);
                if (!Bgr555.IsColorWord(native))
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
