using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Converts the title's complete cartridge CGRAM image to editable RGB5 JSON.</summary>
internal static class TitlePaletteExtractor
{
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        var source = new SnesCgram();
        source.LoadFromBus(bus, TitleSequenceRomData.Assets.PaletteAddress);
        PaletteRgb5[] colors = source.Colors.ToArray().Select(color => new PaletteRgb5
        {
            Red = color & 31,
            Green = color >> 5 & 31,
            Blue = color >> 10 & 31,
        }).ToArray();
        using var json = new MemoryStream();
        TitlePalettePresentation.Write(json, new TitlePaletteDocument
        {
            Version = TitlePaletteFormat.Version,
            Colors = colors,
            BabyMetroidTubeLight = ExtractAmbient(
                TitleScreenAmbientPaletteFxProgramOwner.BabyMetroidTubeLight),
            FlickeringDisplays = ExtractAmbient(
                TitleScreenAmbientPaletteFxProgramOwner.FlickeringDisplays),
        });
        return json.ToArray();

        PaletteRgb5[][] ExtractAmbient(TitleScreenAmbientPaletteFxProgramOwner owner)
        {
            TitleScreenAmbientPaletteFxProgramDefinition definition =
                TitleScreenAmbientPaletteFxProgramMechanicsDefinitions.All.Single(
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
                        bus,
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
