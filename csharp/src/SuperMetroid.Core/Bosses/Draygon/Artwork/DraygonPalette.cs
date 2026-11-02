using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Artwork;

namespace SuperMetroid.Core.Bosses.Draygon.Artwork;

/// <summary>
/// Draygon's installed colours at work: the white hurt flash and health bands resolved from their edits, the opening,
/// hurt and health-band installs, and the identity of the selected colours.
/// </summary>
internal static class DraygonPalette
{
    /// <summary>Gets one of the 16 selected white-flash colors, preserving transparent entry zero.</summary>
    internal static RgbColor WhiteFlash(Dictionary<ColourCell, RgbColor> edits, int color)
    {
        if ((uint)color >= DraygonColorDefinitions.WhiteFlashCount)
            throw new ArgumentOutOfRangeException(nameof(color));
        return edits.TryGetValue(new ColourCell(0, color), out RgbColor selected) ? selected : StockWhiteFlash(color);
    }

    /// <summary>Gets one of the four selected colors of one of Draygon's eight health bands.</summary>
    internal static RgbColor HealthBand(DraygonHealthPaint healthBands, int band, int color)
    {
        _ = CheckBand(band);
        if ((uint)color >= DraygonColorDefinitions.HealthBandColorCount)
            throw new ArgumentOutOfRangeException(nameof(color));
        return healthBands.Color(band, color);
    }

    /// <summary>The white flash preserves the backdrop; all visible inks are white.</summary>
    internal static RgbColor StockWhiteFlash(int color) => color == 0 ? DraygonMaterialPaint.ClearTarget : DraygonColorCatalog.FlashInk;

    /// <summary>Installs the selected opening colors in their place among the colours.</summary>
    internal static void ApplyIntro(DraygonColorCatalog colors, GameplayColours colours)
    {
        ColourRunSequence intro = new BossColours(colours).DraygonIntro;
        for (int color = 0; color < DraygonColorDefinitions.IntroCount; color++) intro.Write(color, colors.ResolveIntro(color));
    }

    /// <summary>Installs Draygon's normal or white-flash material colors and the active health band.</summary>
    internal static void ApplyHurt(DraygonColorCatalog colors, GameplayColours colours, bool whiteFrame, int healthBand)
    {
        if (whiteFrame)
            ApplyCalculated(new BossColours(colours).DraygonBackground, DraygonColorDefinitions.WhiteFlashCount, colors.ResolveWhiteFlash);
        else
            ApplyCalculated(new BossColours(colours).DraygonBackground, DraygonColorDefinitions.BackgroundCount, colors.ResolveBackground);
        if (!whiteFrame)
            ApplyHealthBand(colors, colours, healthBand);
        if (whiteFrame)
            ApplyCalculated(new BossColours(colours).DraygonSprite, DraygonColorDefinitions.WhiteFlashCount, colors.ResolveWhiteFlash);
        else
            ApplyCalculated(new BossColours(colours).DraygonSprite, DraygonColorDefinitions.SpriteCount, colors.ResolveSprite);
    }

    /// <summary>Installs the four-color health band 0-7.</summary>
    internal static void ApplyHealthBand(DraygonColorCatalog colors, GameplayColours colours, int band)
    {
        if (band is < 0 or >= DraygonColorDefinitions.HealthBandCount)
            throw new InvalidDataException($"Draygon health palette band {band} is not authored.");
        Ensure.NotNull(colours);
        for (int color = 0; color < DraygonColorDefinitions.HealthBandColorCount; color++)
            new BossColours(colours).DraygonHealth.Write(color,
                colors.ResolveHealthBand(band, color));
    }

    /// <summary>Validates a health band before resolving one of its colors.</summary>
    private static int CheckBand(int band) =>
        (uint)band < DraygonColorDefinitions.HealthBandCount
            ? band
            : throw new ArgumentOutOfRangeException(nameof(band));

    /// <summary>Writes the selected colors in index order to a colour run.</summary>
    private static void ApplyCalculated(ColourRun destination, int count, Func<int, RgbColor> resolve)
    {
        for (int color = 0; color < count; color++) destination.Write(color, resolve(color));
    }
}
