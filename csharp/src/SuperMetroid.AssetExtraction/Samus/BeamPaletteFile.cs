using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Reads and writes the installed ordinary beam palette document.</summary>
public static class BeamPaletteFile
{
    /// <summary>Strict camel-case JSON settings shared by beam palette loading and writing.</summary>
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        WriteIndented = true,
    };

    /// <summary>Loads all twelve ordinary beam palette selections, rejecting duplicate or unknown properties, unsupported versions, missing keys, rows other than sixteen colors, and RGB5 channels outside 0..31; independently supplied color edits remain exact.</summary>
    /// <param name="json">UTF-8 JSON stream containing the versioned beam-palette document.</param>
    /// <returns>The validated immutable palette catalog.</returns>
    public static BeamPaletteCatalog Read(Stream json)
    {
        BeamPaletteDocument document;
        try
        {
            using var parsed = JsonDocument.Parse(json);
            RejectDuplicates(parsed.RootElement);
            document = parsed.RootElement.Deserialize<BeamPaletteDocument>(Options)
                ?? throw new InvalidDataException("Beam palettes are null.");
        }
        catch (JsonException error) { throw new InvalidDataException("Invalid beam palette JSON.", error); }
        if (document.Version != BeamPaletteDefinitions.Version || document.Palettes is null ||
            document.Palettes.Count != BeamTileAtlasDefinitions.SelectionCount)
            throw new InvalidDataException("Beam palettes require version 1 and all twelve selections.");
        var compiled = new RgbColor[BeamTileAtlasDefinitions.SelectionCount][];
        for (int selection = 0; selection < compiled.Length; selection++)
        {
            if (!document.Palettes.TryGetValue(BeamPaletteDefinitions.Key(SamusBeamCombinations.FromTableIndex(selection)), out var colors) ||
                colors is null || colors.Length != BeamPaletteDefinitions.ColorCount)
                throw new InvalidDataException($"Missing or incomplete beam palette {selection}.");
            compiled[selection] = new RgbColor[colors.Length];
            for (int i = 0; i < colors.Length; i++)
            {
                var color = colors[i];
                if (color is null || (uint)color.Red > 31 || (uint)color.Green > 31 || (uint)color.Blue > 31)
                    throw new InvalidDataException("Beam colors require RGB components from zero through 31.");
                compiled[selection][i] = color.ToRgbColor();
            }
        }
        return new(compiled);
    }

    /// <summary>Serializes a beam-palette document as indented camel-case UTF-8 JSON and validates it through <see cref="Read"/> before returning the bytes.</summary>
    /// <param name="document">Document containing the supported version and all twelve sixteen-color RGB5 palettes.</param>
    /// <returns>Validated JSON bytes suitable for the beam-palette asset file.</returns>
    public static byte[] Write(BeamPaletteDocument document) =>
        JsonAssetDocument.WriteValidated(document, Options, json => _ = Read(json));

    /// <summary>Rejects repeated property names before JSON deserialization can overwrite a color selection.</summary>
    private static void RejectDuplicates(JsonElement value) =>
        JsonAssetDocument.RejectDuplicateProperties(value, StringComparer.Ordinal,
            name => new InvalidDataException("Duplicate beam palette property."));
}
