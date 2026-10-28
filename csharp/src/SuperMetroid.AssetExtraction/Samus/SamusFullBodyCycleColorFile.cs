using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Reads and writes the installed Samus full-body cycle colour document.</summary>
public static class SamusFullBodyCycleColorFile
{
    /// <summary>Loads and validates all 48 full-body cycle palettes for three suits and four families.</summary>
    public static SamusFullBodyCycleColorCatalog Read(Stream json)
    {
        Ensure.NotNull(json);
        SamusFullBodyCycleColorDocument document;
        document = JsonAssetDocument.Read<SamusFullBodyCycleColorDocument>(json, ArtworkJson.Options, "Samus full-body cycle color");
        if (document.Version != SamusFullBodyCycleColorFormat.Version)
            throw new InvalidDataException("Samus full-body cycle colors require the supported version.");

        var palettes = new RgbColor[SamusFullBodyCycleColorFormat.PaletteCount][];
        AddFamily(palettes, SamusFullBodyCycleFamily.SpeedBooster, document.SpeedBooster);
        AddFamily(palettes, SamusFullBodyCycleFamily.ScrewAttack, document.ScrewAttack);
        AddFamily(palettes, SamusFullBodyCycleFamily.StoredShine, document.StoredShine);
        AddFamily(palettes, SamusFullBodyCycleFamily.ActiveShinespark, document.ActiveShinespark);
        return new(palettes);
    }

    /// <summary>Validates and serializes a full-body cycle color document as JSON.</summary>
    public static byte[] Write(SamusFullBodyCycleColorDocument document) =>
        JsonAssetDocument.WriteValidated(document, ArtworkJson.Options, json => _ = Read(json));

    /// <summary>Validates a family's three suits and four shades, then places RGB5 colors in their artwork rows.</summary>
    private static void AddFamily(RgbColor[][] destination,
        SamusFullBodyCycleFamily family, PaletteRgb5[][][]? source)
    {
        if (source is null || source.Length != SamusFullBodyCycleColorFormat.SuitCount)
            throw new InvalidDataException($"{family} requires three suit palettes.");
        for (int suit = 0; suit < source.Length; suit++)
        {
            PaletteRgb5[][]? shades = source[suit];
            if (shades is null || shades.Length != SamusFullBodyCycleColorFormat.ShadesPerSuit)
                throw new InvalidDataException($"{family} suit {suit} requires four shades.");
            for (int shade = 0; shade < shades.Length; shade++)
            {
                PaletteRgb5[]? color = shades[shade];
                if (color is null || color.Length != SamusFullBodyCycleColorFormat.ColorsPerPalette)
                    throw new InvalidDataException($"{family} suit {suit}, shade {shade} requires sixteen colors.");
                var words = new RgbColor[color.Length];
                for (int index = 0; index < color.Length; index++)
                {
                    PaletteRgb5? rgb = color[index];
                    if (rgb is null || (uint)rgb.Red > 31 || (uint)rgb.Green > 31 ||
                        (uint)rgb.Blue > 31)
                        throw new InvalidDataException(
                            $"{family} suit {suit}, shade {shade}, color {index} requires RGB5 channels 0..31.");
                    words[index] = rgb.ToRgbColor();
                }
                int paletteIndex = SamusFullBodyCycleColorFormat.PaletteIndex(new(family, suit, shade));
                if (destination[paletteIndex] is not null)
                    throw new InvalidDataException($"Duplicate full-body palette {family} suit {suit}, shade {shade}.");
                destination[paletteIndex] = words;
            }
        }
    }
}
