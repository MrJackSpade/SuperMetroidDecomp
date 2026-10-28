using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Bosses.Draygon.Artwork;

/// <summary>
/// Draygon's editable RGB5 opening, normal, flash, and health-band colors. Health
/// thresholds, hit reactions, and flash timing remain cartridge-mechanics code.
/// </summary>
public sealed class DraygonColorCatalog
{
    /// <summary>Canonical selected RGB5 colors and ordered rows, independent of JSON encoding.</summary>
    public string ContentIdentity => DraygonPalette.ContentIdentity(this);

    /// <summary><c>Palette_Draygon_WhiteFlash</c> at $A5:A297: every visible flash ink is white.</summary>
    internal static readonly RgbColor FlashInk = RgbColor.FromWord(0x7fff);

    /// <summary>Selected opening colors for Draygon's intro CGRAM region.</summary>
    internal readonly DraygonIntroPaint intro;
    /// <summary>Normal background material colors used outside white hurt flashes.</summary>
    internal readonly DraygonMaterialPaint background;
    /// <summary>Normal sprite material colors used outside white hurt flashes.</summary>
    internal readonly DraygonMaterialPaint sprite;
    /// <summary>Only edited flash colors; stock entries are calculated on lookup.</summary>
    internal readonly Dictionary<ColourCell, RgbColor> whiteFlash;
    /// <summary>Four selected colors for each of Draygon's eight health palette rows.</summary>
    internal readonly DraygonHealthPaint healthBands;

    /// <summary>Retains validated colors and stores only flash entries that differ from stock white.</summary>
    internal DraygonColorCatalog(RgbColor[] intro, RgbColor[] background, RgbColor[] sprite,
        RgbColor[] whiteFlash, RgbColor[][] healthBands) =>
        (this.intro, this.background, this.sprite, this.whiteFlash, this.healthBands) =
            (new DraygonIntroPaint(intro), new DraygonMaterialPaint(background), new DraygonMaterialPaint(sprite),
                DraygonPalette.WhiteFlashEdits(whiteFlash), new DraygonHealthPaint(healthBands));

    /// <summary>Gets one of the 25 selected RGB5 colors used by Draygon's opening palette.</summary>
    public RgbColor ResolveIntro(int color) => intro.Color(color);
    /// <summary>Gets one of the 16 selected normal background colors.</summary>
    public RgbColor ResolveBackground(int color) => background.Color(color);
    /// <summary>Gets one of the 16 selected normal sprite colors.</summary>
    public RgbColor ResolveSprite(int color) => sprite.Color(color);
    /// <summary>Gets one of the 16 selected white-flash colors, preserving transparent entry zero.</summary>
    public RgbColor ResolveWhiteFlash(int color) => DraygonPalette.WhiteFlash(whiteFlash, color);

    /// <summary>Gets one of the four selected colors in an eight-row health palette table.</summary>
    public RgbColor ResolveHealthBand(int band, int color) => DraygonPalette.HealthBand(healthBands, band, color);

    /// <summary>Installs the selected opening colors at their native CGRAM destination.</summary>
    public void ApplyIntro(GameplayColours colours) => DraygonPalette.ApplyIntro(this, colours);

    /// <summary>Installs Draygon's normal or white-flash material colors and the active health band.</summary>
    public void ApplyHurt(GameplayColours colours, bool whiteFrame, int healthBand) =>
        DraygonPalette.ApplyHurt(this, colours, whiteFrame, healthBand);

    /// <summary>Installs the four-color health band 0-7.</summary>
    public void ApplyHealthBand(GameplayColours colours, int band) => DraygonPalette.ApplyHealthBand(this, colours, band);
}
