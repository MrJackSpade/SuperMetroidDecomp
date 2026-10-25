using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Artwork;

namespace SuperMetroid.Core.Bosses.Draygon.Artwork;

/// <summary>
/// Draygon's editable RGB5 opening, normal, flash, and health-band colors. Health
/// thresholds, hit reactions, and flash timing remain cartridge-mechanics code.
/// </summary>
public sealed class DraygonColorCatalog
{
    /// <summary>Canonical selected RGB5 colors and ordered rows, independent of JSON encoding.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("DraygonColorCatalog-v1", content =>
        {
            Span<RgbColor> introWords = stackalloc RgbColor[DraygonColorDefinitions.IntroCount];
            for (int color = 0; color < introWords.Length; color++) introWords[color] = intro.Color(color);
            content.AppendColors("intro", introWords);
            Span<RgbColor> backgroundWords = stackalloc RgbColor[DraygonColorDefinitions.BackgroundCount];
            for (int color = 0; color < backgroundWords.Length; color++) backgroundWords[color] = background.Color(color);
            content.AppendColors("background", backgroundWords);
            Span<RgbColor> spriteWords = stackalloc RgbColor[DraygonColorDefinitions.SpriteCount];
            for (int color = 0; color < spriteWords.Length; color++) spriteWords[color] = sprite.Color(color);
            content.AppendColors("sprite", spriteWords);
            Span<RgbColor> flash = stackalloc RgbColor[DraygonColorDefinitions.WhiteFlashCount];
            for (int color = 0; color < flash.Length; color++) flash[color] = ResolveWhiteFlash(color);
            content.AppendColors("whiteFlash", flash);
            content.Append("healthBands", DraygonColorDefinitions.HealthBandCount);
            Span<RgbColor> row = stackalloc RgbColor[DraygonColorDefinitions.HealthBandColorCount];
            for (int band = 0; band < DraygonColorDefinitions.HealthBandCount; band++)
            {
                for (int color = 0; color < row.Length; color++) row[color] = ResolveHealthBand(band, color);
                content.AppendColors("row", row);
            }
        });

    /// <summary>Selected opening colors for Draygon's intro CGRAM region.</summary>
    internal readonly DraygonIntroPaintDefinitions intro;
    /// <summary>Normal background material colors used outside white hurt flashes.</summary>
    internal readonly DraygonMaterialPaintDefinitions background;
    /// <summary>Normal sprite material colors used outside white hurt flashes.</summary>
    internal readonly DraygonMaterialPaintDefinitions sprite;
    /// <summary>Only edited flash colors; stock entries are calculated on lookup.</summary>
    internal readonly Dictionary<ColourCell, RgbColor> whiteFlash = new();
    /// <summary>Four selected colors for each of Draygon's eight health palette rows.</summary>
    internal readonly DraygonHealthPaintDefinitions healthBands;

    /// <summary>Retains validated colors and stores only flash entries that differ from stock white.</summary>
    internal DraygonColorCatalog(RgbColor[] intro, RgbColor[] background, RgbColor[] sprite,
        RgbColor[] whiteFlash, RgbColor[][] healthBands)
    {
        this.intro = new(intro);
        this.background = new(background);
        this.sprite = new(sprite);
        for (int color = 0; color < whiteFlash.Length; color++)
            if (whiteFlash[color] != StockWhiteFlash(color)) this.whiteFlash.Add(new ColourCell(0, color), whiteFlash[color]);
        this.healthBands = new(healthBands);
    }

    /// <summary>Gets one of the 25 selected RGB5 colors used by Draygon's opening palette.</summary>
    public RgbColor ResolveIntro(int color) => intro.Color(color);
    /// <summary>Gets one of the 16 selected normal background colors.</summary>
    public RgbColor ResolveBackground(int color) => background.Color(color);
    /// <summary>Gets one of the 16 selected normal sprite colors.</summary>
    public RgbColor ResolveSprite(int color) => sprite.Color(color);
    /// <summary>Gets one of the 16 selected white-flash colors, preserving transparent entry zero.</summary>
    public RgbColor ResolveWhiteFlash(int color)
    {
        if ((uint)color >= DraygonColorDefinitions.WhiteFlashCount)
            throw new ArgumentOutOfRangeException(nameof(color));
        return whiteFlash.TryGetValue(new ColourCell(0, color), out RgbColor selected) ? selected : StockWhiteFlash(color);
    }

    /// <summary>Gets one of the four selected colors in an eight-row health palette table.</summary>
    public RgbColor ResolveHealthBand(int band, int color)
    {
        _ = CheckBand(band);
        if ((uint)color >= DraygonColorDefinitions.HealthBandColorCount)
            throw new ArgumentOutOfRangeException(nameof(color));
        return healthBands.Color(band, color);
    }

    /// <summary><c>Palette_Draygon_WhiteFlash</c> at $A5:A297 preserves the backdrop; all visible inks are white.</summary>
    private static RgbColor StockWhiteFlash(int color) => color == 0 ? DraygonMaterialPaintDefinitions.ClearTarget : RgbColor.FromWord(0x7fff);

    /// <summary>Installs the selected opening colors at their native CGRAM destination.</summary>
    public void ApplyIntro(GameplayColours colours) =>
        ApplyCalculated(new BossColours(colours).DraygonIntro, DraygonColorDefinitions.IntroCount, ResolveIntro);

    /// <summary>Installs Draygon's normal or white-flash material colors and the active health band.</summary>
    public void ApplyHurt(GameplayColours colours, bool whiteFrame, int healthBand)
    {
        if (whiteFrame)
            ApplyCalculated(new BossColours(colours).DraygonBackground, DraygonColorDefinitions.WhiteFlashCount, ResolveWhiteFlash);
        else
            ApplyCalculated(new BossColours(colours).DraygonBackground, DraygonColorDefinitions.BackgroundCount, ResolveBackground);
        if (!whiteFrame)
            ApplyHealthBand(colours, healthBand);
        if (whiteFrame)
            ApplyCalculated(new BossColours(colours).DraygonSprite, DraygonColorDefinitions.WhiteFlashCount, ResolveWhiteFlash);
        else
            ApplyCalculated(new BossColours(colours).DraygonSprite, DraygonColorDefinitions.SpriteCount, ResolveSprite);
    }

    /// <summary>Installs the four-color health band 0-7.</summary>
    public void ApplyHealthBand(GameplayColours colours, int band)
    {
        if (band is < 0 or >= DraygonColorDefinitions.HealthBandCount)
            throw new InvalidDataException($"Draygon health palette band {band} is not authored.");
        Ensure.NotNull(colours);
        for (int color = 0; color < DraygonColorDefinitions.HealthBandColorCount; color++)
            new BossColours(colours).DraygonHealth.Write(color,
                ResolveHealthBand(band, color));
    }

    /// <summary>Validates a health palette row before resolving one of its colors.</summary>
    private static int CheckBand(int band) =>
        (uint)band < DraygonColorDefinitions.HealthBandCount
            ? band
            : throw new ArgumentOutOfRangeException(nameof(band));

    /// <summary>Writes the selected colors in index order to a native CGRAM palette view.</summary>
    private static void ApplyCalculated(ColourRun destination, int count, Func<int, RgbColor> resolve)
    {
        for (int color = 0; color < count; color++) destination.Write(color, resolve(color));
    }
}
