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
            NorfairForegroundAndHeatPhase = ExtractNorfair(
                NorfairEnvironmentalPaletteOwner.ForegroundAndHeatPhase),
            NorfairForegroundPalette4 = ExtractNorfair(
                NorfairEnvironmentalPaletteOwner.ForegroundPalette4),
            NorfairForegroundPalette5 = ExtractNorfair(
                NorfairEnvironmentalPaletteOwner.ForegroundPalette5),
            NorfairForegroundPalette6 = ExtractNorfair(
                NorfairEnvironmentalPaletteOwner.ForegroundPalette6),
            MaridiaSandPits = ExtractMaridia(MaridiaEnvironmentalPaletteOwner.SandPits),
            MaridiaSandFalls = ExtractMaridia(MaridiaEnvironmentalPaletteOwner.SandFalls),
            MaridiaBackgroundWaterfalls = ExtractMaridia(
                MaridiaEnvironmentalPaletteOwner.BackgroundWaterfalls),
            WreckedShipGreenLights = ExtractWreckedShipGreenLights(),
        });
        return json.ToArray();

        PaletteRgb5[][] ExtractNorfair(NorfairEnvironmentalPaletteOwner owner)
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

        PaletteRgb5[][] ExtractMaridia(MaridiaEnvironmentalPaletteOwner owner)
        {
            MaridiaEnvironmentalPaletteFxProgramDefinition definition =
                MaridiaEnvironmentalPaletteFxProgramMechanicsDefinitions.All.Single(
                    item => item.Owner == owner);
            var frames = new PaletteRgb5[definition.FrameCount][];
            for (int frame = 0; frame < definition.FrameCount; frame++)
            {
                frames[frame] = new PaletteRgb5[definition.ColorsPerFrame];
                for (int index = 0; index < definition.ColorsPerFrame; index++)
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

        PaletteRgb5[][] ExtractWreckedShipGreenLights()
        {
            int frameCount =
                WreckedShipGreenLightPaletteFxProgramMechanicsDefinitions.FrameCount;
            int colorCount =
                WreckedShipGreenLightPaletteFxProgramMechanicsDefinitions.ColorsPerFrame;
            var frames = new PaletteRgb5[frameCount][];
            for (int frame = 0; frame < frameCount; frame++)
            {
                frames[frame] = new PaletteRgb5[colorCount];
                for (int index = 0; index < colorCount; index++)
                {
                    ushort pointer =
                        WreckedShipGreenLightPaletteFxProgramMechanicsDefinitions.ColorPointer(
                            frame, index);
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
