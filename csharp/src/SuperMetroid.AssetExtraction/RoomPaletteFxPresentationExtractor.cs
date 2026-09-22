using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.AssetExtraction;

/// <summary>Extracts authored normal-room palette animation colors to RGB5 JSON.</summary>
internal static class RoomPaletteFxPresentationExtractor
{
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        using var json = new MemoryStream();
        RoomPaletteFxPresentation.Write(json, new RoomPaletteFxPresentationDocument
        {
            Version = RoomPaletteFxPresentationFormat.Version,
            NorfairForegroundAndHeatPhase = Extract(
                NorfairEnvironmentalPaletteOwner.ForegroundAndHeatPhase),
            NorfairForegroundPalette4 = Extract(
                NorfairEnvironmentalPaletteOwner.ForegroundPalette4),
            NorfairForegroundPalette5 = Extract(
                NorfairEnvironmentalPaletteOwner.ForegroundPalette5),
            NorfairForegroundPalette6 = Extract(
                NorfairEnvironmentalPaletteOwner.ForegroundPalette6),
        });
        return json.ToArray();

        PaletteRgb5[][] Extract(NorfairEnvironmentalPaletteOwner owner)
        {
            NorfairEnvironmentalPaletteFxProgramDefinition definition =
                NorfairEnvironmentalPaletteFxProgramMechanicsDefinitions.All.Single(
                    item => item.Owner == owner);
            int frameCount = NorfairEnvironmentalPaletteFxProgramMechanicsDefinitions.FrameCount;
            int colorCount = NorfairEnvironmentalPaletteFxProgramMechanicsDefinitions.ColorsPerFrame;
            var frames = new PaletteRgb5[frameCount][];
            for (int frame = 0; frame < frameCount; frame++)
            {
                frames[frame] = new PaletteRgb5[colorCount];
                for (int index = 0; index < colorCount; index++)
                {
                    ushort pointer = definition.ColorPointer(frame, index);
                    ushort bgr555 = RomDataReader.ReadWordFixedBank(
                        bus,
                        RoomFxRomData.Banks.PaletteFx | pointer);
                    frames[frame][index] = new PaletteRgb5
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
