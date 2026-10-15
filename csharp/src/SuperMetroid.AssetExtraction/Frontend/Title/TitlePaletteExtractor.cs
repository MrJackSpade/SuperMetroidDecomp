using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Frontend.Title.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.Frontend.Title;

/// <summary>Converts the title's complete cartridge CGRAM image to editable RGB5 JSON.</summary>
[CpuBusBoundary("codec: ROM title CGRAM image → title palette")]
internal static class TitlePaletteExtractor
{
    /// <summary>Exports the full title CGRAM image, both ambient palette-FX timelines, and copyright skip colors to the versioned presentation catalog.</summary>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        var source = new SnesCgram();
        CartridgePaletteImporter.LoadToCgram(source, bus, TitleSequenceRomData.Assets.PaletteAddress);
        PaletteRgb5[] colors = new CgramPalettes(source).Image.SelectToArray(PaletteRgb5.From);
        using var json = new MemoryStream();
        TitlePalettePresentation.Write(json, new TitlePaletteDocument
        {
            Version = TitlePaletteFormat.Version,
            Colors = colors,
            BabyMetroidTubeLight = ExtractAmbient(
                TitleScreenAmbientPaletteFxProgramOwner.BabyMetroidTubeLight),
            FlickeringDisplays = ExtractAmbient(
                TitleScreenAmbientPaletteFxProgramOwner.FlickeringDisplays),
            SkipCopyrightWhite = PaletteRgb5.From(TitleSequenceRomData.Palette.CopyrightWhite),
            SkipCopyrightRed = PaletteRgb5.From(TitleSequenceRomData.Palette.CopyrightRed),
        });
        return json.ToArray();

        PaletteRgb5[][] ExtractAmbient(TitleScreenAmbientPaletteFxProgramOwner owner)
        {
            TitleScreenAmbientPaletteFxColorDefinition definition =
                TitleScreenAmbientPaletteFxColorDefinitions.All.Single(
                    item => item.Owner == owner);
            var frames = new PaletteRgb5[definition.FrameCount][];
            for (int frame = 0; frame < frames.Length; frame++)
            {
                frames[frame] = new PaletteRgb5[definition.ColorsPerFrame];
                for (int color = 0; color < frames[frame].Length; color++)
                {
                    ushort pointer = unchecked((ushort)(
                        definition.FramePointer(frame) + sizeof(ushort) +
                        color * sizeof(ushort)));
                    ushort bgr555 = RomDataReader.ReadWordFixedBank(
                        CartridgeImportSource.Require(bus),
                        RoomFxRomData.Banks.PaletteFx | pointer);
                    frames[frame][color] = new PaletteRgb5
                    {
                        Red = bgr555 & 31,
                        Green = bgr555 >> 5 & 31,
                        Blue = bgr555 >> 10 & 31,
                    };
                }
            }
            return frames;
        }
    }
}
