using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Reads and writes the installed Crystal Flash colour document.</summary>
public static class CrystalFlashColorFile
{
    /// <summary>Loads the supported camel-case JSON schema, rejecting duplicate or unknown properties, incorrect frame dimensions, null colors, and RGB5 channels outside 0..31.</summary>
    /// <param name="json">Readable JSON stream; ownership remains with the caller.</param>
    /// <returns>Validated body and bubble colors independent of cartridge reads and gameplay timing.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is null.</exception>
    /// <exception cref="InvalidDataException">The JSON, schema version, palette dimensions, or color values are invalid.</exception>
    public static CrystalFlashColorCatalog Read(Stream json)
    {
        Ensure.NotNull(json);
        CrystalFlashColorDocument document;
        document = JsonAssetDocument.Read<CrystalFlashColorDocument>(json, ArtworkJson.Options, "Crystal Flash color");
        if (document.Version != CrystalFlashColorFormat.Version)
            throw new InvalidDataException("Crystal Flash colors require the supported version.");
        return new(Compile(document.Body, CrystalFlashColorFormat.BodyFrameCount,
                CrystalFlashColorFormat.BodyColorCount, "body"),
            Compile(document.Bubble, CrystalFlashColorFormat.BubbleFrameCount,
                CrystalFlashColorFormat.BubbleColorCount, "bubble"));
    }

    /// <summary>Serializes an editable document as indented camel-case UTF-8 JSON and validates the result through <see cref="Read"/> before returning it.</summary>
    /// <param name="document">Body and bubble RGB5 frame arrays with the supported schema version.</param>
    /// <returns>Validated JSON bytes ready to store as the catalog resource.</returns>
    /// <exception cref="InvalidDataException">The serialized document fails catalog validation.</exception>
    public static byte[] Write(CrystalFlashColorDocument document) =>
        JsonAssetDocument.WriteValidated(document, ArtworkJson.Options, json => _ = Read(json));

    /// <summary>Validates an authored palette grid's dimensions and RGB5 channels before packing its colors.</summary>
    private static RgbColor[][] Compile(PaletteRgb5[][]? source, int frameCount,
        int colorCount, string name)
    {
        if (source is null || source.Length != frameCount)
            throw new InvalidDataException($"Crystal Flash {name} requires {frameCount} frames.");
        var result = new RgbColor[frameCount][];
        for (int frame = 0; frame < frameCount; frame++)
        {
            PaletteRgb5[]? colors = source[frame];
            if (colors is null || colors.Length != colorCount)
                throw new InvalidDataException(
                    $"Crystal Flash {name} frame {frame} requires {colorCount} colors.");
            result[frame] = new RgbColor[colorCount];
            for (int index = 0; index < colorCount; index++)
            {
                PaletteRgb5? rgb = colors[index];
                if (rgb is null || (uint)rgb.Red > 31 || (uint)rgb.Green > 31 ||
                    (uint)rgb.Blue > 31)
                    throw new InvalidDataException(
                        $"Crystal Flash {name} frame {frame}, color {index} requires RGB5 channels 0..31.");
                result[frame][index] = rgb.ToRgbColor();
            }
        }
        return result;
    }
}
