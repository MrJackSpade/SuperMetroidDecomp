using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Fx.Artwork;
using SuperMetroid.AssetExtraction.Fx.PaletteFx;

namespace SuperMetroid.AssetExtraction.Fx;

/// <summary>Extracts authored room and cinematic palette animation colors to RGB5 JSON.</summary>
internal static class RoomPaletteFxPresentationExtractor
{
    /// <summary>Reads compiled room and cinematic palette-FX source colors and writes their RGB5 presentation document.</summary>
    public static byte[] Extract(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
        using var json = new MemoryStream();
        RoomPaletteFxPresentationFile.Write(json, new RoomPaletteFxPresentationDocument
        {
            Version = RoomPaletteFxPresentationFormat.Version,
            Programs = RoomPaletteFxPrograms.All.ToDictionary(program => program.Key, program => ExtractFrames(program.Runs[0].Frames), StringComparer.Ordinal),
        });
        return json.ToArray();

        PaletteRgb5[][] ExtractFrames(PaletteFxColorFrames colorFrames) => PaletteFxColorExtraction.ReadFrames(bus, colorFrames);
    }
}
