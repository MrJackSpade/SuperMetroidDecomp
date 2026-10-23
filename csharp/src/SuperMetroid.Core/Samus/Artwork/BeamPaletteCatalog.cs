using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Immutable ordinary beam colors; selection and charge/Hyper animation remain engine-owned.</summary>
public sealed class BeamPaletteCatalog
{
    /// <summary>Only selected RGB5 words that differ from calculated stock beam paint.</summary>
    internal readonly Dictionary<int, Bgr555> palettes = new();
    /// <summary>Stores independently edited colors while deriving unchanged words from stock paint.</summary>
    internal BeamPaletteCatalog(Bgr555[][] rows)
    {
        for (int index = 0; index < rows.Length; index++)
        for (int color = 0; color < BeamPaletteDefinitions.ColorCount; color++)
            if (rows[index][color] != BeamPaintDefinitions.Color(SamusBeamCombinations.FromTableIndex(index), color))
                palettes.Add(index * BeamPaletteDefinitions.ColorCount + color, rows[index][color]);
    }

    /// <summary>Gets an edited beam color when supplied, otherwise its calculated stock value.</summary>
    private Bgr555 Color(SamusBeamCombination selection, int color) =>
        palettes.TryGetValue(selection.TableIndex * BeamPaletteDefinitions.ColorCount + color, out Bgr555 supplied)
            ? supplied : BeamPaintDefinitions.Color(selection, color);
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
    public static BeamPaletteCatalog Load(Stream json)
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
        var compiled = new Bgr555[BeamTileAtlasDefinitions.SelectionCount][];
        for (int selection = 0; selection < compiled.Length; selection++)
        {
            if (!document.Palettes.TryGetValue(BeamPaletteDefinitions.Key(SamusBeamCombinations.FromTableIndex(selection)), out var colors) ||
                colors is null || colors.Length != BeamPaletteDefinitions.ColorCount)
                throw new InvalidDataException($"Missing or incomplete beam palette {selection}.");
            compiled[selection] = new Bgr555[colors.Length];
            for (int i = 0; i < colors.Length; i++)
            {
                var color = colors[i];
                if (color is null || (uint)color.Red > 31 || (uint)color.Green > 31 || (uint)color.Blue > 31)
                    throw new InvalidDataException("Beam colors require RGB components from zero through 31.");
                compiled[selection][i] = color.ToBgr555();
            }
        }
        return new(compiled);
    }

    /// <summary>Copies all sixteen colors of an ordinary beam selection to CGRAM entries 224..239 (OBJ palette 6), matching native <c>Load_Beam_Palette_withStackPrepped</c> at <c>$90:ACCD</c>; does not run charge or Hyper Beam color animation.</summary>
    /// <param name="beam">The beam colours to change.</param>
    /// <param name="selection">Retail beam combination $0..$B.</param>
    public void LoadTo(ColourRun beam, SamusBeamCombination selection)
    {
        if (!selection.IsRetail) throw new ArgumentOutOfRangeException(nameof(selection), selection, "Beam palettes cover retail combinations only.");
        for (int i = 0; i < BeamPaletteDefinitions.ColorCount; i++)
            beam.Write(i, Color(selection, i));
    }

    /// <summary>Serializes a beam-palette document as indented camel-case UTF-8 JSON and validates it through <see cref="Load"/> before returning the bytes.</summary>
    /// <param name="document">Document containing the supported version and all twelve sixteen-color RGB5 palettes.</param>
    /// <returns>Validated JSON bytes suitable for the beam-palette asset file.</returns>
    public static byte[] Write(BeamPaletteDocument document) =>
        JsonAssetDocument.WriteValidated(document, Options, json => _ = Load(json));

    /// <summary>Rejects repeated property names before JSON deserialization can overwrite a color selection.</summary>
    private static void RejectDuplicates(JsonElement value) =>
        JsonAssetDocument.RejectDuplicateProperties(value, StringComparer.Ordinal,
            name => new InvalidDataException("Duplicate beam palette property."));
}
