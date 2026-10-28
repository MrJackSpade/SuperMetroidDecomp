using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Reads and writes the installed Hyper Beam effect colour document.</summary>
public static class HyperBeamFxColorFile
{
    /// <summary>Strict camel-case JSON settings for the editable Hyper Beam FX palette.</summary>
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    /// <summary>Loads ten eight-color projectile palette rows, rejecting duplicate or unknown properties, unsupported versions, incorrect dimensions, and RGB5 channels outside 0..31; preserves supplied edits independently of shared-color and midpoint relationships.</summary>
    /// <param name="json">UTF-8 JSON stream containing the versioned projectile-color document.</param>
    /// <returns>The validated catalog used by the Hyper Beam palette-FX handler.</returns>
    public static HyperBeamFxColorCatalog Read(Stream json)
    {
        HyperBeamFxColorDocument document;
        try
        {
            using var parsed = JsonDocument.Parse(json);
            RejectDuplicateProperties(parsed.RootElement);
            document = parsed.RootElement.Deserialize<HyperBeamFxColorDocument>(Options)
                ?? throw new InvalidDataException("Hyper Beam FX colors are null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid Hyper Beam FX color JSON.", error);
        }

        if (document.Version != HyperBeamFxColorFormat.Version ||
            !HyperBeamFxColorFormat.Shape.HasRows(document.Frames))
            throw new InvalidDataException("Hyper Beam FX colors require ten frames at the supported version.");

        var compiled = new RgbColor[document.Frames.Length][];
        for (int frame = 0; frame < compiled.Length; frame++)
        {
            PaletteRgb5[]? colors = document.Frames[frame];
            if (!HyperBeamFxColorFormat.Shape.HasColumns(colors))
                throw new InvalidDataException($"Hyper Beam FX frame {frame} requires eight colors.");
            compiled[frame] = new RgbColor[colors.Length];
            for (int color = 0; color < colors.Length; color++)
            {
                PaletteRgb5? rgb = colors[color];
                if (rgb is null || (uint)rgb.Red > 31 || (uint)rgb.Green > 31 || (uint)rgb.Blue > 31)
                    throw new InvalidDataException($"Hyper Beam FX frame {frame}, color {color} requires RGB5 components.");
                compiled[frame][color] = rgb.ToRgbColor();
            }
        }
        return new(compiled);
    }

    /// <summary>Serializes the projectile-color document as indented camel-case UTF-8 JSON, then validates it through <see cref="Read"/> before returning the bytes.</summary>
    /// <param name="document">Document containing the supported version and all ten eight-color RGB5 rows.</param>
    /// <returns>Validated JSON bytes suitable for the projectile-color asset file.</returns>
    public static byte[] Write(HyperBeamFxColorDocument document) =>
        JsonAssetDocument.WriteValidated(document, Options, json => _ = Read(json));

    /// <summary>Rejects duplicate palette properties before document deserialization.</summary>
    private static void RejectDuplicateProperties(JsonElement value) =>
        JsonAssetDocument.RejectDuplicateProperties(value, StringComparer.Ordinal,
            name => new InvalidDataException("Duplicate Hyper Beam FX color property."));
}
