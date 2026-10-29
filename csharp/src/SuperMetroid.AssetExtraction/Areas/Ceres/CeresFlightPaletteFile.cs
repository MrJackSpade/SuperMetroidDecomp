using SuperMetroid.Core;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.AssetExtraction.Video;

namespace SuperMetroid.AssetExtraction.Areas.Ceres;

/// <summary>Reads and writes the installed Ceres flight palette document.</summary>
public static class CeresFlightPaletteFile
{
    /// <summary>Loads the supported palette schema with exactly 256 non-null RGB5 colors, rejecting malformed or duplicate JSON properties and channels outside 0..31; supplied edits remain independent of calculated stock repeats and ramps.</summary>
    /// <param name="json">Caller-owned UTF-8 JSON stream containing the complete Ceres approach palette in CGRAM order.</param>
    /// <returns>The immutable selected palette corresponding to compiled <c>Palettes_SpaceGunshipCeres</c> at <c>$8C:E5E9</c>.</returns>
    public static CeresFlightPalette Read(Stream json)
    {
        CeresFlightPaletteDocument document = JsonAssetDocument.Read<CeresFlightPaletteDocument>(
            json, ArtworkJson.Options, "Ceres flight palette");
        if (document.Version != CeresFlightPaletteFormat.Version ||
            document.Colors is not { Length: ScreenColourImage.ColorCount })
            throw new InvalidDataException("Ceres flight palette requires 256 RGB5 colors.");

        var compiled = new RgbColor[ScreenColourImage.ColorCount];
        for (int index = 0; index < document.Colors.Length; index++)
        {
            PaletteRgb5? color = document.Colors[index];
            if (color is null || (uint)color.Red > 31 || (uint)color.Green > 31 ||
                (uint)color.Blue > 31)
                throw new InvalidDataException(
                    $"Ceres flight palette color {index} requires red, green and blue in 0..31.");
            compiled[index] = color.ToRgbColor();
        }
        return new CeresFlightPalette(ScreenColourImage.Decode(compiled));
    }

    /// <summary>Serializes a palette document with the presentation JSON options and validates it through <see cref="Read"/> before writing any bytes to the destination.</summary>
    /// <param name="json">Caller-owned destination stream for the validated UTF-8 JSON.</param>
    /// <param name="document">Document containing the supported version and all 256 RGB5 colors.</param>
    public static void Write(Stream json, CeresFlightPaletteDocument document)
    {
        Ensure.NotNull(json);
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document,
            ArtworkJson.Options);
        _ = Read(new MemoryStream(bytes, writable: false));
        json.Write(bytes);
    }
}
