using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Bosses.Ridley.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses.Ridley;

/// <summary>Reads and writes the installed Ceres Ridley colour document.</summary>
public static class CeresRidleyColorFile
{
    /// <summary>Compiles RGB5 JSON into a validated immutable color catalog, rejecting unknown/duplicate properties, unsupported versions, wrong row dimensions, and channels outside 0..31.</summary>
    /// <param name="json">JSON input read from its current position; the caller retains ownership of the stream.</param>
    /// <param name="stockForLegacyOverride">Optional stock catalog required for versions one and two: supplies missing alarm colors and, for version one, missing Baby colors.</param>
    /// <returns>A catalog retaining independent supplied color edits while calculating unchanged shared paint and fade relationships.</returns>
    public static CeresRidleyColorCatalog Read(Stream json,
        CeresRidleyColorCatalog? stockForLegacyOverride = null)
    {
        Ensure.NotNull(json);
        CeresRidleyColorDocument document;
        document = JsonAssetDocument.Read<CeresRidleyColorDocument>(json, ArtworkJson.Options, "Ceres Ridley color");
        var version = (CeresRidleyColorVersion)document.Version;
        if (!Enum.IsDefined(version) ||
            version != CeresRidleyColorVersion.Current && stockForLegacyOverride is null)
            throw new InvalidDataException("Ceres Ridley colors require the supported version.");
        Bgr555[][] bodyFade = CompileRows(document.BodyFade, CeresRidleyPaletteDefinitions.BodyFadeRowCount,
            CeresRidleyPaletteDefinitions.BodyFadeColorCount, "body fade");
        Bgr555[] start = Compile(document.Start, CeresRidleyPaletteDefinitions.StartColorCount, "start");
        Bgr555[][] eyeFade = CompileRows(document.EyeFade, CeresRidleyPaletteDefinitions.EyeFadeRowCount,
            CeresRidleyPaletteDefinitions.EyeFadeColorCount, "eye fade");
        Bgr555[][] health = CompileRows(document.Health, CeresRidleyPaletteDefinitions.HealthRowCount,
            CeresRidleyPaletteDefinitions.HealthColorCount, "health");
        // A legacy override predates the alarm (and, before version two, the Baby) colours; the
        // version check above guarantees stock is supplied for the catalog to inherit them from.
        Bgr555[][]? alarm = version != CeresRidleyColorVersion.Current
            ? null
            : CompileRows(document.Alarm, CeresRidleyPaletteDefinitions.AlarmRowCount,
                CeresRidleyPaletteDefinitions.AlarmColorCount, "alarm");
        Bgr555[] retreatBg = Compile(document.RetreatBg, CeresRidleyPaletteDefinitions.RetreatBgColorCount, "retreat BG");
        Bgr555[] retreatShared = Compile(document.RetreatShared, CeresRidleyPaletteDefinitions.RetreatSharedColorCount,
            "retreat shared");
        Bgr555[][]? baby = version == CeresRidleyColorVersion.PreBaby
            ? null
            : CompileRows(document.Baby, CeresRidleyPaletteDefinitions.BabyRowCount,
                CeresRidleyPaletteDefinitions.BabyColorCount, "Baby");
        return CeresRidleyColorCatalog.Create(new CeresRidleyColorRows
        {
            Start = start, EyeFade = eyeFade, BodyFade = bodyFade, Health = health, Alarm = alarm,
            RetreatBg = retreatBg, RetreatShared = retreatShared, Baby = baby,
        }, stockForLegacyOverride);
    }

    /// <summary>Serializes a complete current-version color document as indented camel-case UTF-8 JSON, then reloads it to enforce the same schema, dimension, and RGB5 validation as asset loading.</summary>
    public static byte[] Write(CeresRidleyColorDocument document) =>
        JsonAssetDocument.WriteValidated(document, ArtworkJson.Options, json => _ = Read(json));

    /// <summary>Requires the palette family's exact row count and validates every row with its expected color width.</summary>
    private static Bgr555[][] CompileRows(PaletteRgb5[][]? rows, int count,
        int colorsPerRow, string name)
    {
        if (rows is null || rows.Length != count)
            throw new InvalidDataException($"Ceres Ridley {name} requires {count} rows.");
        var compiled = new Bgr555[count][];
        for (int row = 0; row < count; row++)
            compiled[row] = Compile(rows[row], colorsPerRow, $"{name} row {row}");
        return compiled;
    }

    /// <summary>Converts an exact-width RGB5 row to BGR555 after rejecting missing colors and channels outside five-bit range.</summary>
    private static Bgr555[] Compile(PaletteRgb5[]? colors, int count, string name) =>
        Rgb5Rows.Compile(colors, count, $"Ceres Ridley {name}");
}
