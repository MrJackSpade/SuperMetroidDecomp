using System.Text.Json;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Bosses.Ridley.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses.Ridley;

/// <summary>Reads and writes the installed Ceres Ridley colour document.</summary>
public static class CeresRidleyColorFile
{
    /// <summary>Compiles RGB5 JSON into a validated immutable color catalog, rejecting unknown/duplicate properties, unsupported versions, wrong row dimensions, and channels outside 0..31.</summary>
    /// <param name="json">JSON input read from its current position; the caller retains ownership of the stream.</param>
    /// <returns>A catalog keeping every installed colour family as installed.</returns>
    public static CeresRidleyColorCatalog Read(Stream json)
    {
        Ensure.NotNull(json);
        CeresRidleyColorDocument document;
        document = JsonAssetDocument.Read<CeresRidleyColorDocument>(json, ArtworkJson.Options, "Ceres Ridley color");
        if (document.Version != CeresRidleyColorFormat.Version)
            throw new InvalidDataException("Ceres Ridley colors require the supported version.");
        RgbColor[][] bodyFade = CompileRows(document.BodyFade, CeresRidleyShadeDefinitions.BodyFadeShadeCount,
            CeresRidleyColorFormat.BodyFadeColorCount, "body fade");
        RgbColor[] start = Compile(document.Start, CeresRidleyColorFormat.StartColorCount, "start");
        RgbColor[][] eyeFade = CompileRows(document.EyeFade, CeresRidleyColorFormat.EyeFadeShadeCount,
            CeresRidleyColorFormat.EyeFadeColorCount, "eye fade");
        RgbColor[][] health = CompileRows(document.Health, CeresRidleyColorFormat.HealthBandCount,
            CeresRidleyColorFormat.HealthColorCount, "health");
        RgbColor[][] alarm = CompileRows(document.Alarm, CeresRidleyColorFormat.AlarmStepCount,
            CeresRidleyColorFormat.AlarmColorCount, "alarm");
        var retreat = new CeresRidleyRetreatColours
        {
            Background = Compile(document.RetreatBg, CeresRidleyColorFormat.RetreatBgColorCount, "retreat BG"),
            Shared = Compile(document.RetreatShared, CeresRidleyColorFormat.RetreatSharedColorCount, "retreat shared"),
        };
        CeresBabyShadeColours baby = CompileShades(document.BabyShades
            ?? throw new InvalidDataException("Ceres Ridley Baby requires its shades."));
        return new CeresRidleyColorCatalog(start, eyeFade, bodyFade, health, alarm, retreat, baby);
    }

    /// <summary>Serializes a complete current-version color document as indented camel-case UTF-8 JSON.</summary>
    public static byte[] Write(CeresRidleyColorDocument document) =>
        JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options);

    /// <summary>Requires the palette family's exact row count and validates every row with its expected color width.</summary>
    private static RgbColor[][] CompileRows(PaletteRgb5[][]? rows, int count,
        int colorsPerRow, string name)
    {
        if (rows is null || rows.Length != count)
            throw new InvalidDataException($"Ceres Ridley {name} requires {count} rows.");
        var compiled = new RgbColor[count][];
        for (int row = 0; row < count; row++)
            compiled[row] = Compile(rows[row], colorsPerRow, $"{name} row {row}");
        return compiled;
    }

    /// <summary>Validates each Baby shade's fifteen colours.</summary>
    private static CeresBabyShadeColours CompileShades(CeresBabyShadeColorsDocument shades) => new()
    {
        Resting = CompileShade(shades, CeresBabyShade.Resting),
        HorizontalSquish = CompileShade(shades, CeresBabyShade.HorizontalSquish),
        Round = CompileShade(shades, CeresBabyShade.Round),
        VerticalSquish = CompileShade(shades, CeresBabyShade.VerticalSquish),
    };

    /// <summary>Validates one Baby shade's fifteen colours.</summary>
    private static RgbColor[] CompileShade(CeresBabyShadeColorsDocument shades, CeresBabyShade shade) =>
        Compile(shades.Of(shade), CeresRidleyColorFormat.BabyColorCount, $"Baby {shade}");

    /// <summary>Converts an exact-width RGB5 row to BGR555 after rejecting missing colors and channels outside five-bit range.</summary>
    private static RgbColor[] Compile(PaletteRgb5[]? colors, int count, string name) =>
        Rgb5Rows.Compile(colors, count, $"Ceres Ridley {name}");
}
