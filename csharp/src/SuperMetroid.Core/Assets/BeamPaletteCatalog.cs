using System.Text.Json;
using System.Text.Json.Serialization;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Immutable ordinary beam colors; selection and charge/Hyper animation remain engine-owned.</summary>
public sealed class BeamPaletteCatalog
{
    // Reviewed exact material paint and copied target metadata have bounded dispositions;
    // every stock output calculates, while independently supplied words stay exact.
    private readonly Dictionary<int, Bgr555> palettes = new();
    private BeamPaletteCatalog(Bgr555[][] rows)
    {
        for (int index = 0; index < rows.Length; index++)
        for (int color = 0; color < BeamPaletteDefinitions.ColorCount; color++)
            if (rows[index][color] != BeamPaintDefinitions.Color(SamusBeamCombinations.FromTableIndex(index), color))
                palettes.Add(index * BeamPaletteDefinitions.ColorCount + color, rows[index][color]);
    }

    private Bgr555 Color(SamusBeamCombination selection, int color) =>
        palettes.TryGetValue(selection.TableIndex * BeamPaletteDefinitions.ColorCount + color, out Bgr555 supplied)
            ? supplied : BeamPaintDefinitions.Color(selection, color);
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
    /// <param name="cgram">Destination color memory to update.</param>
    /// <param name="selection">Retail beam combination $0..$B.</param>
    public void LoadTo(SnesCgram cgram, SamusBeamCombination selection)
    {
        if (!selection.IsRetail) throw new ArgumentOutOfRangeException(nameof(selection), selection, "Beam palettes cover retail combinations only.");
        for (int i = 0; i < BeamPaletteDefinitions.ColorCount; i++)
            cgram.SetColor(SamusProjectileRomData.Palettes.BeamDestinationIndex + i, Color(selection, i));
    }

    /// <summary>Serializes a beam-palette document as indented camel-case UTF-8 JSON and validates it through <see cref="Load"/> before returning the bytes.</summary>
    /// <param name="document">Document containing the supported version and all twelve sixteen-color RGB5 palettes.</param>
    /// <returns>Validated JSON bytes suitable for the beam-palette asset file.</returns>
    public static byte[] Write(BeamPaletteDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, Options);
        _ = Load(new MemoryStream(bytes, writable: false));
        return bytes;
    }

    private static void RejectDuplicates(JsonElement value) =>
        JsonAssetDocument.RejectDuplicateProperties(value, StringComparer.Ordinal,
            name => new InvalidDataException("Duplicate beam palette property."));
}

/// <summary>Editable JSON schema for ordinary beam-combination colors; palette selection and charge/Hyper Beam animation are engine-owned rather than document data.</summary>
public sealed record BeamPaletteDocument
{
    /// <summary>Schema revision, which must equal <see cref="BeamPaletteDefinitions.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>All twelve selections keyed by <see cref="BeamPaletteDefinitions.Key"/>, each with sixteen RGB5 colors in OBJ palette order, including color zero; combinations may independently edit colors even when their stock native pointers alias.</summary>
    public required Dictionary<string, PaletteRgb5[]> Palettes { get; init; }
}

/// <summary>Presentation geometry for the sixteen colors written by $90:ACCD.</summary>
/// <remarks>
/// The stock color source at <c>$90:C3E1..C480</c> is five contiguous
/// sixteen-word little-endian BGR555 rows: Power, Ice, Wave, Plasma, and
/// Spazer. Row <c>r = 0..4</c>, color <c>c = 0..15</c> is at
/// <c>$C3E1 + $20*r + 2*c</c>; the twelve beam combinations select rows
/// through <see cref="SamusProjectileRomData.Beams.PalettePointers"/>.
/// All 80 words match the pinned NTSC J/U v1.0 ROM and native listing.
/// Every row begins <c>$3800,$7FFF</c>; Power, Wave, Plasma, and Spazer
/// have six zero words at color indices 9..14, while Ice has none. The
/// selected material paint is owned by BeamPaintDefinitions; its exact channel
/// composition calculates all stock words and preserves independent replacements.
/// Native palette loading copies exactly sixteen colors from the selected
/// pointer; out-of-range beam selections are physical pointer-table reads,
/// not extra color rows. Investigation: #625 / #901.
/// </remarks>
public static class BeamPaletteDefinitions
{
    /// <summary>Asset filename for the ordinary beam-combination RGB5 palettes.</summary>
    public const string FileName = "beam-palettes.json";
    /// <summary>Supported revision of the twelve-selection beam-palette JSON schema.</summary>
    public const int Version = 1;
    /// <summary>Sixteen colors copied by the native beam loader, covering all entries of OBJ palette 6, including color zero.</summary>
    public const int ColorCount = 16;
    /// <summary>$90:C3C9..C3E0 selects Ice before Plasma before Wave before Spazer before Power; the first two colors alias the common Power inputs in all five rows.</summary>
    internal static SamusBeamCombination ColorSourceSelection(SamusBeamCombination selection, int color)
    {
        if (color < 2) return SamusBeamCombination.Power;
        if (selection.HasIce) return SamusBeamCombination.Ice;
        if (selection.HasPlasma) return SamusBeamCombination.Plasma;
        if (selection.HasWave) return SamusBeamCombination.Wave;
        if (selection.HasSpazer) return SamusBeamCombination.Spazer;
        return SamusBeamCombination.Power;
    }

    /// <summary>$90:C3F3..C3FE and corresponding Power/Wave/Plasma/Spazer slots9..14 are black; Ice uses independent colored entries.</summary>
    internal static bool IsBlackSlot(SamusBeamCombination selection, int color) =>
        !selection.HasIce && color is >= 9 and <= 14;
    /// <summary>Returns the canonical JSON key for an ordinary beam combination using its uppercase two-digit hexadecimal selection index.</summary>
    /// <param name="selection">Retail beam combination; simultaneous Spazer/Plasma selections are outside this catalog.</param>
    /// <returns>A key from <c>beam-00</c> through <c>beam-0B</c>.</returns>
    public static string Key(SamusBeamCombination selection)
    {
        if (!selection.IsRetail) throw new ArgumentOutOfRangeException(nameof(selection), selection, "Beam palettes cover retail combinations only.");
        return $"beam-{selection.TableIndex:X2}";
    }
}
