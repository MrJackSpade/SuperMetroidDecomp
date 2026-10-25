using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain;

/// <summary>Reads and writes the installed Zebetite colour document.</summary>
public static class ZebetiteColorFile
{
    /// <summary>Strict camel-case JSON options shared by load and write.</summary>
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    /// <summary>Loads all eight two-color pulse rows, rejecting duplicate or unknown properties, unsupported versions, incorrect dimensions, and RGB5 channels outside 0..31; preserves each supplied color edit independently of the stock symmetric pulse.</summary>
    /// <param name="json">UTF-8 JSON stream containing the versioned Zebetite color document.</param>
    /// <returns>The validated immutable barrier-color catalog.</returns>
    public static ZebetiteColorCatalog Read(Stream json)
    {
        ZebetiteColorDocument document;
        try
        {
            using var parsed = JsonDocument.Parse(json);
            RejectDuplicates(parsed.RootElement);
            document = parsed.RootElement.Deserialize<ZebetiteColorDocument>(Options)
                ?? throw new InvalidDataException("Zebetite colors are null.");
        }
        catch (JsonException error)
        {
            throw new InvalidDataException("Invalid Zebetite color JSON.", error);
        }
        if (document.Version != ZebetiteColorFormat.Version ||
            !ZebetiteColorFormat.Shape.HasRows(document.Frames))
            throw new InvalidDataException("Zebetite colors require all eight frames at the supported version.");

        var frames = new RgbColor[document.Frames.Length][];
        for (int frame = 0; frame < document.Frames.Length; frame++)
        {
            PaletteRgb5[]? colors = document.Frames[frame];
            if (!ZebetiteColorFormat.Shape.HasColumns(colors))
                throw new InvalidDataException($"Zebetite frame {frame} requires two RGB5 colors.");
            frames[frame] = new RgbColor[colors.Length];
            for (int color = 0; color < colors.Length; color++)
            {
                PaletteRgb5? rgb = colors[color];
                if (rgb is null || (uint)rgb.Red > 31 || (uint)rgb.Green > 31 || (uint)rgb.Blue > 31)
                    throw new InvalidDataException($"Zebetite frame {frame}, color {color} requires RGB5 channels.");
                frames[frame][color] = rgb.ToRgbColor();
            }
        }
        return new(frames);
    }

    /// <summary>Serializes a Zebetite color document as indented camel-case UTF-8 JSON, then validates the result through <see cref="Read"/> before returning it.</summary>
    /// <param name="document">Document containing the supported version and all eight two-color RGB5 rows.</param>
    /// <returns>Validated JSON bytes suitable for the barrier-color asset file.</returns>
    public static byte[] Write(ZebetiteColorDocument document) =>
        JsonAssetDocument.WriteValidated(document, Options, json => _ = Read(json));

    /// <summary>Rejects repeated property names before deserialization can overwrite an earlier value.</summary>
    private static void RejectDuplicates(JsonElement value) =>
        JsonAssetDocument.RejectDuplicateProperties(value, StringComparer.Ordinal,
            name => new InvalidDataException("Duplicate Zebetite color property."));
}
